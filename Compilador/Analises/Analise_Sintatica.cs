using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Analises
{
    public class AnaliseSintatica
    {
        private readonly List<Token> tokens;
        private int currentTokenIndex;
        private bool comandoEncontrado = false;

        public List<string> Erros { get; }

        private readonly HashSet<string> syncTokensPrograma = new HashSet<string> { "EOF" };
        private readonly HashSet<string> syncTokensBloco = new HashSet<string> {
             "t_integer", "t_float", "t_char", "t_string", "t_boolean",
             "t_id", "t_if", "t_while", "t_abreBloco",
             "t_ponto_virgula",
             "t_fechaBloco", "EOF"
         };
        private readonly HashSet<string> syncTokensComando = new HashSet<string> { "t_id", "t_if", "t_else", "t_while", "t_abreBloco", "t_fechaBloco", "t_ponto_virgula", "EOF" };
        private readonly HashSet<string> syncTokensExpr = new HashSet<string> { "t_fechaParen", "t_virgula", "t_ponto_virgula", "t_fechaBloco", "EOF" };

        public AnaliseSintatica(List<Token> tokens)
        {
            this.tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            if (!this.tokens.Any())
            {
                this.tokens.Add(new Token("$", "EOF", 1));
            }
            else if (this.tokens.Last().Type != "EOF")
            {
                int lastLine = this.tokens.Last().LineNumber;
                this.tokens.Add(new Token("$", "EOF", lastLine));
            }

            this.currentTokenIndex = 0;
            this.Erros = new List<string>();
        }

        private Token CurrentToken => tokens[currentTokenIndex];
        private Token LookaheadToken => (currentTokenIndex + 1 < tokens.Count) ? tokens[currentTokenIndex + 1] : CurrentToken;
        private void Advance() { if (currentTokenIndex < tokens.Count - 1) currentTokenIndex++; }
        private bool Match(string expectedType) { if (CurrentToken.Type == expectedType) { Advance(); return true; } return false; }

        private void ReportError(string message, int lineNumber)
        {
            lineNumber = Math.Max(1, lineNumber);
            string fullMessage = $"Erro Sintático na Linha {lineNumber}: {message}";

            if (!Erros.Any() || !Erros.Last().StartsWith($"Erro Sintático na Linha {lineNumber}"))
            {
                Erros.Add(fullMessage);
            }
        }

        private void ReportError(string message)
        {
            ReportError(message, CurrentToken.LineNumber);
        }

        private void Synchronize(HashSet<string> syncSet)
        {
            if (CurrentToken.Type == "EOF") return;

            Advance();

            while (CurrentToken.Type != "EOF")
            {
                if (CurrentToken.Type == "t_fechaBloco")
                {
                    return;
                }

                if (IsTypeToken(CurrentToken.Type) || IsComandoStartToken(CurrentToken.Type))
                {
                    return;
                }

                Advance();
            }
        }

        private void Consume(string expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                Advance();
            }
            else
            {
                int reportLine = CurrentToken.LineNumber;

                var statementStartersOrBlockEnders = new HashSet<string> {
                    "t_integer", "t_float", "t_char", "t_string", "t_boolean",
                    "t_id",
                    "t_if", "t_while",
                    "t_abreBloco",
                    "t_fechaBloco"
                };

                if ((expectedType == "t_ponto_virgula" || expectedType == "t_fechaBloco" || expectedType == "t_fechaParen")
                    && (statementStartersOrBlockEnders.Contains(CurrentToken.Type) || CurrentToken.Type == "EOF")
                    && reportLine > 1)
                {
                    reportLine = reportLine - 1;
                }

                string expectedText = expectedType;
                switch (expectedType)
                {
                    case "t_ponto_virgula": expectedText = "';'"; break;
                    case "t_abreBloco": expectedText = "'{'"; break;
                    case "t_fechaBloco": expectedText = "'}'"; break;
                    case "t_abreParen": expectedText = "'('"; break;
                    case "t_fechaParen": expectedText = "')'"; break;
                    case "t_id": expectedText = "identificador"; break;
                }

                ReportError($"Esperado {expectedText}, mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')", reportLine);

                Synchronize(syncTokensBloco);
            }
        }

        public void Parse()
        {
            ParsePrograma();
            if (CurrentToken.Type != "EOF" && !Erros.Any())
            {
                ReportError($"Tokens inesperados após o fim do programa, começando com '{CurrentToken.Lexeme}'", CurrentToken.LineNumber);
            }
        }

        private void ParsePrograma()
        {
            Consume("t_programa");
            Consume("t_id");
            ParseBloco();
        }

        private void ParseBloco()
        {

            Consume("t_abreBloco");
            while (CurrentToken.Type != "t_fechaBloco" && CurrentToken.Type != "EOF")
            {
                // Se já foi encontrado um comando e o próximo token for uma declaração, rejeite
                if (comandoEncontrado && IsTypeToken(CurrentToken.Type))
                {
                    ReportError($"Não é permitido declarar variáveis após um comando dentro do bloco.");
                    Synchronize(syncTokensBloco);
                }
                else
                {
                    // Se encontrar um tipo de variável, parse a declaração
                    if (IsTypeToken(CurrentToken.Type))
                    {
                        ParseDeclaracaoVariavel();
                    }
                    // Se for um comando, marque que um comando foi encontrado
                    else if (IsComandoStartToken(CurrentToken.Type))
                    {
                        comandoEncontrado = true;  // Marca que um comando foi encontrado
                        ParseComando();
                    }
                    else
                    {
                        ReportError($"Esperado declaração ou comando dentro do bloco, mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')");
                        Synchronize(syncTokensBloco);
                    }
                }
            }

            Consume("t_fechaBloco");
        }


        private bool IsComandoStartToken(string type) { return type == "t_if" || type == "t_while" || type == "t_id" || type == "t_abreBloco"; }
        private bool IsTypeToken(string type) { return type == "t_integer" || type == "t_float" || type == "t_char" || type == "t_string" || type == "t_boolean"; }

        private void ParseDeclaracaoVariavel()
        {
            if (IsTypeToken(CurrentToken.Type))
            {
                Advance();
                ParseIdentificadorInicializador();
                while (Match("t_virgula"))
                {
                    ParseIdentificadorInicializador();
                }
                Consume("t_ponto_virgula");
            }
            else
            {
                ReportError($"Esperado tipo (Integer, Float, etc.) para iniciar declaração, mas encontrou '{CurrentToken.Type}'");
                Synchronize(syncTokensBloco);
            }
        }

        private void ParseIdentificadorInicializador()
        {
            Consume("t_id");
            if (Match("t_atribuicao"))
            {
                ParseExpr();
            }
        }

        private void ParseComando()
        {
            if (CurrentToken.Type == "t_if") { ParseComandoIf(); }
            else if (CurrentToken.Type == "t_while") { ParseComandoWhile(); }
            else if (CurrentToken.Type == "t_id")
            {
                if (LookaheadToken.Type == "t_atribuicao")
                {
                    ParseComandoAtribuicao();
                }
                else
                {
                    ReportError($"Identificador '{CurrentToken.Lexeme}' não inicia um comando de atribuição válido aqui.");
                    Synchronize(syncTokensComando);
                }
            }
            else if (CurrentToken.Type == "t_abreBloco")
            {
                ParseBloco();
            }
            else
            {
                ReportError($"Token inesperado '{CurrentToken.Type}' no início de um comando.");
                Synchronize(syncTokensComando);
            }
        }

        private void ParseComandoAtribuicao()
        {
            Consume("t_id");
            Consume("t_atribuicao");
            ParseExpr();
            Consume("t_ponto_virgula");
        }

        private void ParseComandoIf()
        {
            Consume("t_if");
            Consume("t_abreParen");
            ParseExprRelacional();
            Consume("t_fechaParen");
            ParseBloco();

            if (Match("t_else"))
            {
                ParseBloco();
            }
        }

        private void ParseComandoWhile()
        {
            Consume("t_while");
            Consume("t_abreParen");
            ParseExprRelacional();
            Consume("t_fechaParen");
            ParseBloco();
        }

        private void ParseExprRelacional()
        {
            ParseExpr();
            if (IsOpRel(CurrentToken.Type))
            {
                Advance();
                ParseExpr();
            }
        }
        private bool IsOpRel(string type) { return type == "t_igualdade" || type == "t_diferenca" || type == "t_menor" || type == "t_maior" || type == "t_menor_igual" || type == "t_maior_igual"; }

        private void ParseExpr() { ParseTermo(); ParseExprPrime(); }

        private void ParseExprPrime() { if (IsOpAddSub(CurrentToken.Type)) { Advance(); ParseTermo(); ParseExprPrime(); } }
        private bool IsOpAddSub(string type) { return type == "t_soma" || type == "t_subtracao"; }

        private void ParseTermo() { ParseFator(); ParseTermoPrime(); }

        private void ParseTermoPrime() { if (IsOpMulDiv(CurrentToken.Type)) { Advance(); ParseFator(); ParseTermoPrime(); } }
        private bool IsOpMulDiv(string type) { return type == "t_multiplicacao" || type == "t_divisao"; }

        private void ParseFator()
        {
            if (Match("t_id")) { }
            else if (Match("t_numero_int")) { }
            else if (Match("t_numero_real")) { }
            else if (Match("t_bool")) { }
            else if (Match("t_char_literal")) { }
            else if (Match("t_string_literal")) { }
            else if (Match("t_abreParen"))
            {
                ParseExpr();
                Consume("t_fechaParen");
            }
            else
            {
                ReportError($"Esperado ID, número, booleano, literal ou '(', mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')");
                Synchronize(syncTokensExpr);
            }
        }
    }
}