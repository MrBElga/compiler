using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Analises
{
    public class Simbolo
    {
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public bool Inicializado { get; set; }
        public bool Usado { get; set; }
        public int LinhaDeclaracao { get; set; }
    }

    public class TabelaSimbolos
    {
        private Dictionary<string, Simbolo> simbolos = new Dictionary<string, Simbolo>();

        public void Adicionar(string nome, string tipo, int linha)
        {
            if (!simbolos.ContainsKey(nome))
            {
                simbolos[nome] = new Simbolo
                {
                    Nome = nome,
                    Tipo = tipo,
                    Inicializado = false,
                    Usado = false,
                    LinhaDeclaracao = linha
                };
            }
        }

        public Simbolo Buscar(string nome)
        {
            simbolos.TryGetValue(nome, out var simbolo);
            return simbolo;
        }

        public IEnumerable<Simbolo> Todos() => simbolos.Values;
    }

    public class Analise_Semantica
    {
        private readonly List<Token> tokens;
        private readonly TabelaSimbolos tabelaSimbolos = new TabelaSimbolos();
        private int currentTokenIndex = 0;

        public List<string> Erros { get; }

        public Analise_Semantica(List<Token> tokens)
        {
            this.tokens = tokens;
            this.Erros = new List<string>();
        }

        private Token CurrentToken => tokens[currentTokenIndex];
        private Token LookaheadToken => (currentTokenIndex + 1 < tokens.Count) ? tokens[currentTokenIndex + 1] : CurrentToken;
        private void Advance() { if (currentTokenIndex < tokens.Count - 1) currentTokenIndex++; }

        public void Analisar()
        {
            if (CurrentToken.Type == "t_programa")
            {
                Advance();
                if (CurrentToken.Type == "t_id")
                {
                    Advance();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado identificador após 'Program'.");
                }
            }

            while (CurrentToken.Type != "EOF")
            {
                AnalisarComandoOuDeclaracao();
            }

            VerificarVariaveisNaoUsadas();
        }

        private void AnalisarComandoOuDeclaracao()
        {
            if (IsTipo(CurrentToken.Type))
            {
                AnalisarDeclaracao();
            }
            else if (CurrentToken.Type == "t_id")
            {
                AnalisarAtribuicao();
            }
            else if (CurrentToken.Type == "t_if")
            {
                AnalisarComandoIf();
            }
            else if (CurrentToken.Type == "t_while")
            {
                AnalisarComandoWhile();
            }
            else if (CurrentToken.Type == "t_abreBloco")
            {
                AnalisarBloco();
            }
            else if (CurrentToken.Type == "t_else")
            {
                Advance(); // Consome Else
                AnalisarBloco();
            }
            else
            {
                Advance(); // Se não for nada disso, ignora
            }
        }

        private void AnalisarComandoIf()
        {
            Advance(); // Consome 'if'
            if (CurrentToken.Type == "t_abreParen")
            {
                Advance(); 
                AnalisarExpressaoRelacional();
                if (CurrentToken.Type == "t_fechaParen")
                {
                    Advance();
                    AnalisarBloco();
                    if (CurrentToken.Type == "t_else")
                    {
                        Advance();
                        AnalisarBloco();
                    }
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ')' após condição do if.");
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado '(' após 'if'.");
            }
        }

        private void AnalisarExpressaoRelacional()
        {
            if (CurrentToken.Type == "t_bool")
            {
                Advance(); // valor true ou false, direto
            }
            else if (CurrentToken.Type == "t_id")
            {
                var simbolo = tabelaSimbolos.Buscar(CurrentToken.Lexeme);
                if (simbolo != null && simbolo.Tipo == "t_boolean")
                {
                    simbolo.Usado = true;
                    Advance(); // variável booleana, aceita
                }
                else
                {
                    AnalisarExpressao(null);

                    if (IsOperadorRelacional(CurrentToken.Type))
                    {
                        Advance(); // consome operador
                        AnalisarExpressao(null);
                    }
                    else
                    {
                        Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado operador relacional em expressão condicional.");
                    }
                }
            }
            else
            {
                AnalisarExpressao(null);

                if (IsOperadorRelacional(CurrentToken.Type))
                {
                    Advance(); // consome operador
                    AnalisarExpressao(null);
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado operador relacional em expressão condicional.");
                }
            }
        }


        private bool IsOperadorRelacional(string tipo)
        {
            return tipo == "t_igualdade" || tipo == "t_diferenca" ||
                   tipo == "t_menor" || tipo == "t_maior" ||
                   tipo == "t_menor_igual" || tipo == "t_maior_igual";
        }



        private void AnalisarComandoWhile()
        {
            Advance(); // Consome 'while'
            if (CurrentToken.Type == "t_abreParen")
            {
                Advance();
                AnalisarExpressaoRelacional();
                if (CurrentToken.Type == "t_fechaParen")
                {
                    Advance();
                    AnalisarBloco();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ')' após condição do while.");
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado '(' após 'while'.");
            }
        }

        private void AnalisarBloco()
        {
            if (CurrentToken.Type == "t_abreBloco")
            {
                Advance();
                while (CurrentToken.Type != "t_fechaBloco" && CurrentToken.Type != "EOF")
                {
                    AnalisarComandoOuDeclaracao();
                }
                if (CurrentToken.Type == "t_fechaBloco")
                {
                    Advance();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado '}}' para fechar bloco.");
                }
            }
            else
            {
                AnalisarComandoOuDeclaracao(); // Um comando simples sem bloco
            }
        }


        private bool IsTipo(string tipo) =>
            tipo == "t_integer" || tipo == "t_float" || tipo == "t_char" || tipo == "t_string" || tipo == "t_boolean";

        private void AnalisarDeclaracao()
        {
            string tipo = CurrentToken.Type;
            Advance();

            while (CurrentToken.Type == "t_id")
            {
                string nomeVar = CurrentToken.Lexeme;
                tabelaSimbolos.Adicionar(nomeVar, tipo, CurrentToken.LineNumber);
                Advance();

                if (CurrentToken.Type == "t_atribuicao")
                {
                    Advance();
                    AnalisarExpressao(tipo);
                    var simbolo = tabelaSimbolos.Buscar(nomeVar);
                    if (simbolo != null) simbolo.Inicializado = true;
                }

                if (CurrentToken.Type == "t_virgula")
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            if (CurrentToken.Type == "t_ponto_virgula")
            {
                Advance();
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ';' após declaração.");
            }
        }

        private void AnalisarAtribuicao()
        {
            string nomeVar = CurrentToken.Lexeme;
            var simbolo = tabelaSimbolos.Buscar(nomeVar);

            if (simbolo == null)
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: variável '{nomeVar}' não declarada.");
            }
            else
            {
                simbolo.Usado = true;
            }

            Advance(); // id

            if (CurrentToken.Type == "t_atribuicao")
            {
                Advance(); // '='
                if (simbolo != null)
                {
                    AnalisarExpressao(simbolo.Tipo);
                    simbolo.Inicializado = true;
                }
                else
                {
                    AnalisarExpressao(null);
                }
            }

            if (CurrentToken.Type == "t_ponto_virgula")
            {
                Advance();
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ';' após atribuição.");
            }
        }

        private void AnalisarExpressao(string tipoEsperado)
        {
            AnalisarTermo(tipoEsperado);

            while (CurrentToken.Type == "t_soma" || CurrentToken.Type == "t_subtracao")
            {
                Advance(); // consome o operador
                AnalisarTermo(tipoEsperado);
            }
        }

        private void AnalisarTermo(string tipoEsperado)
        {
            AnalisarFator(tipoEsperado);

            while (CurrentToken.Type == "t_multiplicacao" || CurrentToken.Type == "t_divisao")
            {
                Advance(); // consome o operador
                AnalisarFator(tipoEsperado);
            }
        }

        private void AnalisarFator(string tipoEsperado)
        {
            if (CurrentToken.Type == "t_id")
            {
                var simbolo = tabelaSimbolos.Buscar(CurrentToken.Lexeme);
                if (simbolo == null)
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: variável '{CurrentToken.Lexeme}' não declarada.");
                }
                else
                {
                    if (!simbolo.Inicializado)
                    {
                        Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: variável '{CurrentToken.Lexeme}' usada sem inicialização.");
                    }

                    simbolo.Usado = true;

                    if (tipoEsperado != null && !TiposCompativeis(tipoEsperado, simbolo.Tipo))
                    {
                        Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível em expressão (esperado {tipoEsperado}, encontrado {simbolo.Tipo}).");
                    }
                }
                Advance();
            }
            else if (CurrentToken.Type.StartsWith("t_numero"))
            {
                string tipoValor = CurrentToken.Type == "t_numero_int" ? "t_integer" : "t_float";

                if (tipoEsperado != null && !TiposCompativeis(tipoEsperado, tipoValor))
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível em valor numérico (esperado {tipoEsperado}, encontrado {tipoValor}).");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_char_literal")
            {
                if (tipoEsperado != null && tipoEsperado != "t_char")
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado {tipoEsperado}, encontrado t_char).");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_string_literal")
            {
                if (tipoEsperado != null && tipoEsperado != "t_string")
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado {tipoEsperado}, encontrado t_string).");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_bool")
            {
                if (tipoEsperado != null && tipoEsperado != "t_boolean")
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado {tipoEsperado}, encontrado t_boolean).");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_abreParen")
            {
                Advance();
                AnalisarExpressao(tipoEsperado);
                if (CurrentToken.Type == "t_fechaParen")
                {
                    Advance();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ')' para fechar expressão.");
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: expressão inválida.");
                Advance();
            }
        }



        private bool TiposCompativeis(string esperado, string encontrado)
        {
            if (esperado == encontrado) return true;
            if (esperado == "t_float" && encontrado == "t_integer") return true;
            return false;
        }

        private void VerificarVariaveisNaoUsadas()
        {
            foreach (var simbolo in tabelaSimbolos.Todos())
            {
                if (!simbolo.Usado)
                {
                    Erros.Add($"Aviso Semântico na linha {simbolo.LinhaDeclaracao}: variável '{simbolo.Nome}' declarada mas nunca usada.");
                }
            }
        }
    }
}
