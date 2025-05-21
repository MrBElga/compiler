using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compilador.Analises
{
    public class GeradorCodigoIntermediario
    {
        private List<Token> tokens;
        private int index;
        private List<string> codigoIntermediario;
        private int tempCount = 0;

        public GeradorCodigoIntermediario(List<Token> tokens)
        {
            this.tokens = tokens;
            this.index = 0;
            this.codigoIntermediario = new List<string>();
        }

        public List<string> Gerar()
        {
            if (Match("t_programa"))
            {
                Avancar();
                if (Match("t_id"))
                    Avancar();
            }

            if (Match("t_abreBloco"))
            {
                Avancar();
                while (!Match("t_fechaBloco") && !Match("EOF"))
                {
                    Comando();
                }
                Avancar(); // fecha bloco
            }

            return codigoIntermediario;
        }

        private void Comando()
        {
            if (Match("t_id") && Lookahead().Type == "t_atribuicao")
            {
                Atribuicao();
            }
            else if (Match("t_if"))
            {
                ComandoIf();
            }
            else if (Match("t_while"))
            {
                ComandoWhile();
            }
            else
            {
                Avancar(); // ignora token inválido ou irrelevante
            }
        }

        private void Atribuicao()
        {
            string id = Current().Lexeme;
            Avancar(); // id
            Avancar(); // =
            string temp = Expressao();
            codigoIntermediario.Add($"{id} = {temp}");
            if (Match("t_ponto_virgula"))
                Avancar();
        }

        private void ComandoIf()
        {
            Avancar(); // if
            Avancar(); // (
            string cond = ExpressaoRelacional();
            Avancar(); // )
            string rotuloFimIf = NovoRotulo();
            codigoIntermediario.Add($"ifFalse {cond} goto {rotuloFimIf}");
            Avancar(); // {
            while (!Match("t_fechaBloco"))
            {
                Comando();
            }
            Avancar(); // }
            codigoIntermediario.Add($"{rotuloFimIf}:");
        }

        private void ComandoWhile()
        {
            string rotuloInicio = NovoRotulo();
            string rotuloFim = NovoRotulo();
            codigoIntermediario.Add($"{rotuloInicio}:");
            Avancar(); // while
            Avancar(); // (
            string cond = ExpressaoRelacional();

            Avancar(); // )
            codigoIntermediario.Add($"ifFalse {cond} goto {rotuloFim}");
            Avancar(); // {
            while (!Match("t_fechaBloco"))
            {
                Comando();
            }
            Avancar(); // }
            codigoIntermediario.Add($"goto {rotuloInicio}");
            codigoIntermediario.Add($"{rotuloFim}:");
        }

        private string Expressao()
        {
            string termo1 = Termo();
            while (Match("t_soma") || Match("t_subtracao"))
            {
                string op = Current().Lexeme;
                Avancar();
                string termo2 = Termo();
                string temp = NovoTemp();
                codigoIntermediario.Add($"{temp} = {termo1} {op} {termo2}");
                termo1 = temp;
            }
            return termo1;
        }

        private string ExpressaoRelacional()
        {
            string ladoEsquerdo = Expressao();
            if (Match("t_igualdade") || Match("t_diferenca") || Match("t_menor") || Match("t_maior") || Match("t_menor_igual") || Match("t_maior_igual"))
            {
                string operador = Current().Lexeme;
                Avancar();
                string ladoDireito = Expressao();
                string temp = NovoTemp();
                codigoIntermediario.Add($"{temp} = {ladoEsquerdo} {operador} {ladoDireito}");
                return temp;
            }

            return ladoEsquerdo; // se não for relacional, trata como expressão simples (e.g., booleano)
        }


        private string Termo()
        {
            string fator1 = Fator();
            while (Match("t_multiplicacao") || Match("t_divisao"))
            {
                string op = Current().Lexeme;
                Avancar();
                string fator2 = Fator();
                string temp = NovoTemp();
                codigoIntermediario.Add($"{temp} = {fator1} {op} {fator2}");
                fator1 = temp;
            }
            return fator1;
        }

        private string Fator()
        {
            if (Match("t_id") || Match("t_numero_int") || Match("t_numero_real"))
            {
                string val = Current().Lexeme;
                Avancar();
                return val;
            }
            else if (Match("t_abreParen"))
            {
                Avancar();
                string expr = Expressao();
                Avancar(); // fecha paren
                return expr;
            }
            return "0";
        }

        private string NovoTemp() => $"t{tempCount++}";
        private string NovoRotulo() => $"L{tempCount++}";

        private Token Current() => tokens[index];
        private Token Lookahead() => (index + 1 < tokens.Count) ? tokens[index + 1] : tokens[index];
        private bool Match(string tipo) => Current().Type == tipo;
        private void Avancar() { if (index < tokens.Count - 1) index++; }
    }
}
