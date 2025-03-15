using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Compilador.Analises
{
    internal class Analise_Lexica
    {
        private FileStream arquivo;
        private string[,] tbReservada;

        public Analise_Lexica(FileStream arq)
        {
            this.arquivo = arq;
            criarTabelaReservada();
        }

        private void criarTabelaReservada()
        {
            tbReservada = new string[,] {
                {"Program", "t_programa"},
                {"Integer", "t_integer"},
                {"Float", "t_float"},
                {"Char", "t_char"},
                {"String", "t_string"},
                {"If", "t_if"},
                {"Else", "t_else"},
                {"While", "t_while"},
                {"{", "t_abreBloco"},
                {"}", "t_fechaBloco"},
                {"(", "t_abreParen"},
                {")", "t_fechaParen"},
                {"=", "t_atribuicao"},
                {",", "t_pontuacao"},
                {".", "t_pontuacao"},
                {"true", "t_bool"},
                {"false", "t_bool"}
            };
        }

        public List<(string TokenDescription, int LineNumber)> AnalisadorLexico()
        {
            List<(string TokenDescription, int LineNumber)> relatorio = new List<(string, int)>();
            int contLinha = 1;

            StreamReader streamReader = new StreamReader(arquivo);
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string linha;
            while ((linha = streamReader.ReadLine()) != null)
            {
                string[] tokens = linha.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        bool isReserved = false;
                        for (int j = 0; j < tbReservada.GetLength(0); j++)
                        {
                            if (tbReservada[j, 0] == token)
                            {
                                relatorio.Add(($"{token} eh {tbReservada[j, 1]}", contLinha));
                                isReserved = true;
                                break;
                            }
                        }

                        if (!isReserved)
                        {
                            string retorno = AnalyzeToken(token);
                            if (retorno.Contains("ERRO"))
                            {
                                relatorio.Add((retorno.Replace("\n", " "), contLinha));
                            }
                            else
                            {
                                relatorio.Add((retorno, contLinha));
                            }
                        }
                    }
                }
                contLinha++;
            }
            streamReader.Close();
            return relatorio;
        }

        private string AnalyzeToken(string token)
        {
            string retorno = null;

            retorno = firstID(token);
            if (retorno == null) retorno = firstNum(token);
            if (retorno == null) retorno = firstOpRelacao(token);
            if (retorno == null) retorno = firstOpLogico(token);
            if (retorno == null) retorno = firstOpAddSub(token);
            if (retorno == null) retorno = firstOpMulDiv(token);
            if (retorno == null) retorno = "ERRO : '" + token + "' não pertence à Gramática\n";

            return retorno;
        }

        private string firstID(string token)
        {
            if (char.IsLetter(token[0]) && char.IsLower(token[0]))
            {
                Automato automato = new Automato();
                return automato.AutomatoID(token);
            }
            return null;
        }

        private string firstNum(string token)
        {
            if (char.IsDigit(token[0]))
            {
                Automato automato = new Automato();
                return automato.automatoNum(token);
            }
            return null;
        }

        private string firstOpRelacao(string token)
        {
            if (token[0] == '=' || token[0] == '!' || token[0] == '<' || token[0] == '>')
            {
                Automato automato = new Automato();
                return automato.AutomatoOPRelacao(token);
            }
            return null;
        }

        private string firstOpLogico(string token)
        {
            if (token[0] == '&' || token[0] == '|' || token[0] == '!')
            {
                Automato automato = new Automato();
                return automato.AutomatoOPComparacao(token);
            }
            return null;
        }

        private string firstOpAddSub(string token)
        {
            if (token == "+" || token == "-")
            {
                return token + " eh t_opaddsub\n";
            }
            return null;
        }

        private string firstOpMulDiv(string token)
        {
            if (token == "*" || token == "/")
            {
                return token + " eh t_opmuldiv\n";
            }
            return null;
        }
    }
}