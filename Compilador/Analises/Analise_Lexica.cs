﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compilador.Analises
{
    internal class Analise_Lexica
    {
        private readonly FileStream arquivo;
        private string[,] tbReservada;
        private readonly Automato automato;

        public Analise_Lexica(FileStream arq)
        { this.arquivo = arq ?? throw new ArgumentNullException(nameof(arq)); this.automato = new Automato(); criarTabelaReservada(); }

        private void criarTabelaReservada()
        { tbReservada = new string[,] { { "Program", "t_programa" }, { "Integer", "t_integer" }, { "Float", "t_float" }, { "Char", "t_char" }, { "String", "t_string" }, { "Boolean", "t_boolean" }, { "If", "t_if" }, { "Else", "t_else" }, { "While", "t_while" }, { "{", "t_abreBloco" }, { "}", "t_fechaBloco" }, { "(", "t_abreParen" }, { ")", "t_fechaParen" }, { "=", "t_atribuicao" }, { ",", "t_virgula" }, { ".", "t_ponto" }, { ";", "t_ponto_virgula" }, { "true", "t_bool" }, { "false", "t_bool" } }; }

        public List<(string TokenDescription, int LineNumber)> AnalisadorLexico()
        {
            var relatorio = new List<(string TokenDescription, int LineNumber)>();
            var lexemaBuilder = new StringBuilder();
            int contLinha = 1;

            using (var reader = new StreamReader(arquivo, Encoding.UTF8, true, 1024, true))
            {
                if (arquivo.CanSeek) arquivo.Seek(0, SeekOrigin.Begin);

                int peekInt;
                while ((peekInt = reader.Peek()) != -1)
                {
                    char caractere = (char)peekInt;

                    if (char.IsWhiteSpace(caractere))
                    {
                        reader.Read();
                        if (caractere == '\n') contLinha++;
                        continue;
                    }

                    if (char.IsLetter(caractere))
                    {
                        reader.Read();
                        lexemaBuilder.Clear();
                        lexemaBuilder.Append(caractere);
                        while (reader.Peek() != -1 && char.IsLetterOrDigit((char)reader.Peek()))
                        {
                            lexemaBuilder.Append((char)reader.Read());
                        }
                        string lexemaPotencial = lexemaBuilder.ToString();
                        string tipoTokenReservado = BuscarPalavraReservada(lexemaPotencial);
                        if (tipoTokenReservado != null)
                        {
                            relatorio.Add(($"{lexemaPotencial} eh {tipoTokenReservado}", contLinha));
                        }
                        else
                        {
                            string resAutomatoID = automato.AutomatoID(lexemaPotencial);
                            relatorio.Add((resAutomatoID ?? $"ERRO : ID inválido ou falha no automato '{lexemaPotencial}'", contLinha));
                        }
                        continue;
                    }

                    if (char.IsDigit(caractere))
                    {
                        reader.Read();
                        lexemaBuilder.Clear();
                        lexemaBuilder.Append(caractere);
                        bool temPonto = false;
                        while (reader.Peek() != -1)
                        {
                            char proxChar = (char)reader.Peek();
                            if (char.IsDigit(proxChar))
                            {
                                lexemaBuilder.Append((char)reader.Read());
                            }
                            else if (proxChar == '.' && !temPonto)
                            {
                                reader.Read();
                                if (reader.Peek() != -1 && char.IsDigit((char)reader.Peek()))
                                {
                                    lexemaBuilder.Append('.');
                                    lexemaBuilder.Append((char)reader.Read());
                                    temPonto = true;
                                }
                                else
                                {
                                    lexemaBuilder.Append('.');
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        string numLexema = lexemaBuilder.ToString();
                        string resultadoAutomatoNum = automato.automatoNum(numLexema);
                        relatorio.Add((resultadoAutomatoNum ?? $"ERRO : Numero inválido ou falha no automato '{numLexema}'", contLinha));
                        continue;
                    }

                    if (caractere == '\'')
                    {
                        reader.Read();
                        lexemaBuilder.Clear();
                        int charInternoInt = reader.Read();
                        if (charInternoInt != -1)
                        {
                            char charInterno = (char)charInternoInt;
                            int aspaFinalInt = reader.Read();
                            if (aspaFinalInt != -1 && (char)aspaFinalInt == '\'')
                            {
                                lexemaBuilder.Append(charInterno);
                                relatorio.Add(($"'{lexemaBuilder.ToString()}' eh t_char_literal", contLinha));
                            }
                            else
                            {
                                lexemaBuilder.Append(charInterno);
                                if (aspaFinalInt != -1) lexemaBuilder.Append((char)aspaFinalInt);
                                relatorio.Add(($"ERRO : Char literal mal formado ''{lexemaBuilder.ToString()}...", contLinha));
                            }
                        }
                        else
                        {
                            relatorio.Add(($"ERRO : Fim de arquivo inesperado em char literal", contLinha));
                        }
                        continue;
                    }

                    if (caractere == '"')
                    {
                        reader.Read();
                        lexemaBuilder.Clear();
                        bool stringFechada = false;
                        while (reader.Peek() != -1)
                        {
                            char proxChar = (char)reader.Read();
                            if (proxChar == '"') { stringFechada = true; break; }
                            if (proxChar == '\n') { stringFechada = false; contLinha++; break; }
                            lexemaBuilder.Append(proxChar);
                        }
                        string strLexema = lexemaBuilder.ToString();
                        if (stringFechada) { relatorio.Add(($"\"{strLexema}\" eh t_string_literal", contLinha)); }
                        else { relatorio.Add(($"ERRO : String literal não fechada ou contém nova linha \"{strLexema}...", contLinha)); }
                        continue;
                    }
                    else
                    {
                        char charConsumido = (char)reader.Read();
                        string op1 = charConsumido.ToString();
                        string resultadoFinal = null;

                        if (IsPotentialTwoCharOpStart(charConsumido) && reader.Peek() != -1)
                        {
                            char proxChar = (char)reader.Peek();
                            string op2 = op1 + proxChar;
                            string resAutomato2 = automato.AutomatoOPRelacao(op2);
                            if (resAutomato2 == null) resAutomato2 = automato.AutomatoOPComparacao(op2);

                            if (resAutomato2 != null && !resAutomato2.StartsWith("ERRO"))
                            {
                                reader.Read();
                                resultadoFinal = resAutomato2;
                            }
                        }

                        if (resultadoFinal == null)
                        {
                            string tipoReservado = BuscarPalavraReservada(op1);
                            if (tipoReservado != null)
                            {
                                resultadoFinal = $"{op1} eh {tipoReservado}";
                            }
                            else
                            {
                                string resAutomato1 = automato.AutomatoOPRelacao(op1);
                                if (resAutomato1 == null) resAutomato1 = automato.AutomatoOPComparacao(op1);
                                if (resAutomato1 == null) resAutomato1 = automato.AutomatoOpAddSub(op1);
                                if (resAutomato1 == null) resAutomato1 = automato.AutomatoOpMulDiv(op1);

                                if (resAutomato1 != null && !resAutomato1.StartsWith("ERRO"))
                                {
                                    resultadoFinal = resAutomato1;
                                }
                            }
                        }

                        if (resultadoFinal != null)
                        {
                            relatorio.Add((resultadoFinal, contLinha));
                        }
                        else
                        {
                            relatorio.Add(($"ERRO : Caractere/Símbolo inesperado '{op1}'", contLinha));
                        }
                        continue;
                    }
                }
            }

            return relatorio;
        }

        private bool IsPotentialTwoCharOpStart(char c)
        {
            return c == '=' || c == '!' || c == '<' || c == '>' || c == '&' || c == '|';
        }

        private string BuscarPalavraReservada(string lexema)
        {
            for (int j = 0; j < tbReservada.GetLength(0); j++)
            {
                // Usa string.Equals com StringComparison.OrdinalIgnoreCase para comparar ignorando o case
                if (string.Equals(tbReservada[j, 0], lexema, StringComparison.OrdinalIgnoreCase))
                {
                    // Retorna o tipo do token associado (ex: "t_integer", "t_programa")
                    return tbReservada[j, 1];
                }
            }
            // Se não encontrou nenhuma correspondência (ignorando case), retorna null
            return null;
        }

        private string firstID(string token)
        {
            if (string.IsNullOrEmpty(token) || !char.IsLetter(token[0]) || !char.IsLower(token[0])) return null;
            return automato.AutomatoID(token);
        }

        private string firstNum(string token)
        {
            if (string.IsNullOrEmpty(token) || !char.IsDigit(token[0])) return null;
            return automato.automatoNum(token);
        }

        private string firstOpRelacao(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            char startChar = token[0];
            if (startChar == '=' || startChar == '!' || startChar == '<' || startChar == '>')
            {
                return automato.AutomatoOPRelacao(token);
            }
            return null;
        }

        private string firstOpLogico(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            char startChar = token[0];
            if (startChar == '&' || startChar == '|' || startChar == '!')
            {
                return automato.AutomatoOPComparacao(token);
            }
            return null;
        }

        private string firstOpAddSub(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            char startChar = token[0];
            if (startChar == '+' || startChar == '-')
            {
                return automato.AutomatoOpAddSub(token);
            }
            return null;
        }

        private string firstOpMulDiv(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            char startChar = token[0];
            if (startChar == '*' || startChar == '/')
            {
                return automato.AutomatoOpMulDiv(token);
            }
            return null;
        }
    }
}