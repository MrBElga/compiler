// Analise_Lexica.cs (Ajustado para usar Automato para Operadores)
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
        private Automato automato; // Instanciado no construtor

        public Analise_Lexica(FileStream arq)
        {
            this.arquivo = arq;
            this.automato = new Automato(); // Instancia Automato aqui
            criarTabelaReservada();
        }

        private void criarTabelaReservada()
        {
            // Tabela SEM os operadores que serão tratados pelos automatos via firstOp*
            tbReservada = new string[,] {
                {"Program", "t_programa"}, {"Integer", "t_integer"}, {"Float", "t_float"},
                {"Char", "t_char"}, {"String", "t_string"}, {"If", "t_if"},
                {"Else", "t_else"}, {"While", "t_while"}, {"{", "t_abreBloco"},
                {"}", "t_fechaBloco"}, {"(", "t_abreParen"}, {")", "t_fechaParen"},
                {"=", "t_atribuicao"}, // Mantém '=' para atribuição
                {",", "t_virgula"}, // Renomeado/Mantido
                {".", "t_ponto"},   // Renomeado/Mantido
                {";", "t_ponto_virgula"}, // Ponto e vírgula adicionado
                {"true", "t_bool"}, {"false", "t_bool"}
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
                // !!! ALERTA: Split ainda é a fonte de problemas para operadores juntos !!!
                string[] tokens = linha.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        bool isReserved = false;
                        // 1. Verificar Reservadas
                        for (int j = 0; j < tbReservada.GetLength(0); j++)
                        {
                            if (tbReservada[j, 0] == token)
                            {
                                relatorio.Add(($"{token} eh {tbReservada[j, 1]}", contLinha));
                                isReserved = true;
                                break;
                            }
                        }

                        // 2. Se não reservado, chamar AnalyzeToken
                        if (!isReserved)
                        {
                            string resultadoAnalise = AnalyzeToken(token);
                            // Adiciona o resultado (seja token válido ou erro)
                            relatorio.Add((resultadoAnalise, contLinha));
                        }
                    }
                }
                contLinha++;
            }
            streamReader.Close();
            return relatorio;
        }

        // AnalyzeToken chama os first* na ordem correta
        private string AnalyzeToken(string token)
        {
            string retorno = null;

            retorno = firstID(token);
            if (retorno == null) retorno = firstNum(token);
            if (retorno == null) retorno = firstOpRelacao(token); // Chamando para ==, !=, <, >, <=, >=
            if (retorno == null) retorno = firstOpLogico(token);   // Chamando para &&, ||, !
            if (retorno == null) retorno = firstOpAddSub(token); // Chamando para +, -
            if (retorno == null) retorno = firstOpMulDiv(token); // Chamando para *, /

            if (retorno == null)
            {
                // Nenhum método classificou, retorna erro sem \n
                retorno = $"ERRO : '{token}' não pertence à Gramática ou é inválido";
            }

            // Os métodos chamados já devem retornar sem \n
            return retorno;
        }

        // --- Métodos first* ---
        // Verificam caractere inicial e chamam Automato correspondente

        private string firstID(string token)
        {
            // Condição inicial pode permanecer
            if (string.IsNullOrEmpty(token) || !char.IsLetter(token[0]) || !char.IsLower(token[0])) return null;
            // Chama Automato e retorna o resultado (que já está formatado ou é erro)
            return automato.AutomatoID(token);
        }

        private string firstNum(string token)
        {
            if (string.IsNullOrEmpty(token) || !char.IsDigit(token[0])) return null;
            // Chama Automato e retorna o resultado (que já está formatado ou é erro)
            return automato.automatoNum(token);
        }

        private string firstOpRelacao(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            // Verifica se PODE começar como um op relacional
            char startChar = token[0];
            if (startChar == '=' || startChar == '!' || startChar == '<' || startChar == '>')
            {
                // Chama Automato e retorna o resultado (formatado ou erro)
                return automato.AutomatoOPRelacao(token);
            }
            return null;
        }

        private string firstOpLogico(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            // Verifica se PODE começar como um op lógico
            char startChar = token[0];
            if (startChar == '&' || startChar == '|' || startChar == '!') // Adapte se usar outros símbolos
            {
                // Chama Automato e retorna o resultado (formatado ou erro)
                // Atenção ao nome do método em Automato.cs (usei AutomatoOPComparacao antes, mudei para AutomatoOPLogico para clareza)
                return automato.AutomatoOPComparacao(token); // Ou automato.AutomatoOPLogico(token) se renomeou
            }
            return null;
        }

        private string firstOpAddSub(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            // Verifica se PODE começar como + ou -
            char startChar = token[0];
            if (startChar == '+' || startChar == '-')
            {
                // Chama o novo método do Automato
                return automato.AutomatoOpAddSub(token);
            }
            return null;
        }

        private string firstOpMulDiv(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            // Verifica se PODE começar como * ou /
            char startChar = token[0];
            if (startChar == '*' || startChar == '/')
            {
                // Chama o novo método do Automato
                return automato.AutomatoOpMulDiv(token);
            }
            return null;
        }
    }
}