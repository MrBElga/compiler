
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Analises
{
    public class AnaliseSintatica
    {
        private readonly List<Token> tokens;
        private int currentTokenIndex;
        public List<string> Erros { get; }

        private readonly HashSet<string> syncTokensPrograma = new HashSet<string> { "EOF" };
        private readonly HashSet<string> syncTokensBloco = new HashSet<string> {
             "t_integer", "t_float", "t_char", "t_string", "t_boolean",
             "t_id", "t_if", "t_while", "t_abreBloco",
             "t_ponto_virgula",
             "t_fechaBloco", "EOF"
         };
        private readonly HashSet<string> syncTokensComando = new HashSet<string> { "t_id", "t_if", "t_else", "t_while", "t_abreBloco", "t_fechaBloco", "t_ponto_virgula", "EOF" };
        private readonly HashSet<string> syncTokensExpr = new HashSet<string> { "t_fechaParen", "t_virgula", "t_ponto_virgula", "t_fechaBloco", "EOF" }; // Adicionado ';' e ','


        public AnaliseSintatica(List<Token> tokens)
        {
            this.tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            this.currentTokenIndex = 0;
            this.Erros = new List<string>();
        }

        private Token CurrentToken => tokens[currentTokenIndex];
        private Token LookaheadToken => (currentTokenIndex + 1 < tokens.Count) ? tokens[currentTokenIndex + 1] : CurrentToken;
        private void Advance() { if (currentTokenIndex < tokens.Count - 1) currentTokenIndex++; }
        private bool Match(string expectedType) { if (CurrentToken.Type == expectedType) { Advance(); return true; } return false; }
        private void ReportError(string message) { string fullMessage = $"Erro Sintático na Linha {CurrentToken.LineNumber}: {message}"; if (!Erros.Any() || !Erros.Last().StartsWith($"Erro Sintático na Linha {CurrentToken.LineNumber}")) { Erros.Add(fullMessage); } }
        private void Synchronize(HashSet<string> syncSet) {  Advance(); while (CurrentToken.Type != "EOF") { if (syncSet.Contains(CurrentToken.Type)) { return; } Advance(); } }


        private void Consume(string expectedType)
        {
            if (!Match(expectedType))
            {
                HashSet<string> contextSyncSet = syncTokensBloco; 

                string expectedText = expectedType;
                if (expectedType == "t_ponto_virgula") expectedText = "';'";
                else if (expectedType == "t_abreBloco") expectedText = "'{'";
                else if (expectedType == "t_fechaBloco") expectedText = "'}'";

                ReportError($"Esperado {expectedText}, mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')");
                Synchronize(contextSyncSet);
            }
        }


        // --- Parsing Methods ---

        public void Parse()
        {
            ParsePrograma();
            if (CurrentToken.Type != "EOF" && !Erros.Any())
            {
                ReportError($"Tokens inesperados após o fim do programa, começando com '{CurrentToken.Lexeme}'");
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
                // Decide se é uma declaração ou um comando
                if (IsTypeToken(CurrentToken.Type))
                {
                    ParseDeclaracaoVariavel(); // Chamará o método que consome ';'
                }
                else if (IsComandoStartToken(CurrentToken.Type))
                {
                    ParseComando(); // Comando tratará seu próprio ';' se necessário
                }
                else
                {
                    // Token inesperado dentro do bloco
                    ReportError($"Esperado declaração ou comando dentro do bloco, mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')");
                    Synchronize(syncTokensBloco); // Tenta recuperar dentro do bloco
                }
            }

            Consume("t_fechaBloco");
        }

        // Função auxiliar para verificar se token inicia um comando
        private bool IsComandoStartToken(string type)
        {
            return type == "t_if" || type == "t_while" || type == "t_id" || type == "t_abreBloco";
        }

        // Função auxiliar para verificar se token é um tipo
        private bool IsTypeToken(string type)
        {
            return type == "t_integer" || type == "t_float" || type == "t_char" || type == "t_string" || type == "t_boolean"; ;
        }


        private void ParseDeclaracaoVariavel()
        {
            if (IsTypeToken(CurrentToken.Type))
            {
                Advance(); // Consome o tipo (Integer, Float, etc.)

                ParseIdentificadorInicializador(); // Analisa o primeiro ID e sua inicialização opcional

                while (Match("t_virgula")) // Renomeado de t_pontuacao
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
            Consume("t_id"); // Espera um identificador

            // Verifica inicialização opcional
            if (Match("t_atribuicao")) // Se encontrar '='
            {
                ParseExpr(); // Analisa a expressão de inicialização
            }
            // Se não houver '=', não faz nada 
        }


        // --- Métodos de Comando ---

        private void ParseComando()
        {
            if (CurrentToken.Type == "t_if") { ParseComandoIf(); }
            else if (CurrentToken.Type == "t_while") { ParseComandoWhile(); }
            else if (CurrentToken.Type == "t_id")
            {
                // Verifica se é atribuição
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
                ParseBloco(); // Blocos aninhados não terminam com ';' externo
            }
            else
            {
                // Não deveria chegar aqui se chamado corretamente de ParseBloco
                ReportError($"Token inesperado '{CurrentToken.Type}' no início de um comando.");
                Synchronize(syncTokensComando);
            }
        }
        private void ParseComandoAtribuicao()
        {
            Consume("t_id");
            Consume("t_atribuicao");
            ParseExpr();
            Consume("t_ponto_virgula"); // Atribuição DEVE terminar com ';'
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
                ParseBloco(); // Bloco do else
            }
            // Sem ';' após if/else
        }

        private void ParseComandoWhile()
        {
            Consume("t_while");
            Consume("t_abreParen");
            ParseExprRelacional();
            Consume("t_fechaParen");
            ParseBloco(); 
        }
        private void ParseExprRelacional() { ParseExpr(); if (IsOpRel(CurrentToken.Type)) { Advance(); ParseExpr(); } else { ReportError($"Esperado operador relacional, mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')"); Synchronize(syncTokensExpr); } }
        private bool IsOpRel(string type) {return type == "t_oprel" || type == "t_igualdade" || type == "t_diferenca" || type == "t_menor" || type == "t_maior" || type == "t_menor_igual" || type == "t_maior_igual"; }
        private void ParseExpr() {  ParseTermo(); ParseExprPrime(); }
        private void ParseExprPrime() { if (IsOpAddSub(CurrentToken.Type)) { Advance(); ParseTermo(); ParseExprPrime(); } }
        private bool IsOpAddSub(string type) { return type == "t_opaddsub" || type == "t_soma" || type == "t_subtracao"; }
        private void ParseTermo() { ParseFator(); ParseTermoPrime(); }
        private void ParseTermoPrime() {  if (IsOpMulDiv(CurrentToken.Type)) { Advance(); ParseFator(); ParseTermoPrime(); } }
        private bool IsOpMulDiv(string type) {  return type == "t_opmuldiv" || type == "t_multiplicacao" || type == "t_divisao"; }
        private void ParseFator() {if (Match("t_id")) { } else if (Match("t_numero_int") || Match("t_numero_real")) { } else if (Match("t_bool")) { }
            else if (Match("t_char_literal")) { } 
            else if (Match("t_string_literal")) { }     else if (Match("t_abreParen")) { ParseExpr(); Consume("t_fechaParen"); } else {
                ReportError($"Esperado ID, número, booleano, char, string ou '(', mas encontrou '{CurrentToken.Type}' ('{CurrentToken.Lexeme}')");
                Synchronize(syncTokensExpr);
            } }

    }
}