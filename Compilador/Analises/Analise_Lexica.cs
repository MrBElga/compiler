using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analises
{
    internal class Analise_Lexica
    {
        private FileStream arquivo;
        string[,] tbReservada;
        public Analise_Lexica(FileStream arq) { 
            this.arquivo = arq;
        }
        private void criarTabelaReservada()
        {
            tbReservada = new string[,] {
                {"Program","t_program"},
                {"While","t_while"}, 
                { "If", "t_if" },
                { "Else", "t_else" }, 
                { "{", "t_inibloco" },
                { "}", "t_fimbloco" },
                { "(", "t_abreparen" },
                { ")", "t_fechaparen" },
                { "=", "t_atribuicao" },
                { "Integer", "t_integer" },
                { "Float", "t_float" },
                { "Char", "t_char" },
                { "String", "t_string" },
                { ".", "t_ponto" },
                { ",", "t_virgula" },

            };
        }

        public string AnalisadorLexico()
        {
            string[] palavras;
            int i = 0,j=0;
            string relatorio = "";
            string retorno = null;

            StreamReader streamReader = new StreamReader(arquivo); //Criação do leitor
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string linha = streamReader.ReadLine(); //Ler uma linha e armazenar na variável
            int contLinha = 1;
            

            while (linha != null)
            {
                palavras = linha.Split(' ');
                i = 0;
                while (i< palavras.Length )
                {
                    if (palavras[i] != "") { 
                        if ((palavras[i].ElementAt(0) > 64 && palavras[i].ElementAt(0) <= 90) ||
                            palavras[i].Equals("{") || palavras[i].Equals("}") || 
                            palavras[i].Equals("(") || palavras[i].Equals(")") ||
                            palavras[i].Equals("=") || palavras[i].Equals(".") || palavras[i].Equals(","))
                        {
                            //Chamar tabelas de palavras reservadas
                            criarTabelaReservada();
                            j = 0;
                            while (j< tbReservada.Length/2 && palavras[i] != tbReservada[j, 0])
                                j++;
                            if (j < tbReservada.Length / 2 && palavras[i] == tbReservada[j, 0])
                            {
                                relatorio += tbReservada[j,0] +" eh "+ tbReservada[j,1] + "\n";
                            }
                            else
                            {
                                relatorio += "ERRO :'" + palavras[i] + "' não percente a Gramatica => linha :"+contLinha+" \n";
                            }
                        }
                        else
                        {
                            // Aplicando os first
                            retorno = firstID(palavras[i]);
                            if (retorno == null)
                            {
                                retorno = firstOpRelacao(palavras[i]);
                                if (retorno == null)
                                {
                                    retorno = firstNum(palavras[i]);
                                    if (retorno == null)
                                    {
                                        retorno = firstSomaMenos(palavras[i]);
                                        if (retorno == null)
                                        {
                                            retorno = firstMultDiv(palavras[i]);
                                            if (retorno == null)
                                            {
                                                retorno = firstOpComparacao(palavras[i]);
                                            }
                                        }
                                    }
                                }
                            }

                            if (retorno.Contains("ERRO"))
                            {
                                retorno = retorno.Replace('\n',' ');
                                retorno = retorno+" => linha :" + contLinha + "\n";
                            }
                            relatorio += retorno;
                        }
                     }

                    i++;
                }
                linha = streamReader.ReadLine(); //Leitura da nova linha
                contLinha++;
            }
            streamReader.Close();
            return relatorio;
        }

        private string firstID(string palavra)
        {
            if (palavra.ElementAt(0) > 96 && palavra.ElementAt(0)<=122)
            {
                Automato automato = new Automato();
                return automato.AutomatoID(palavra);
            }
            else
            {
                return null;
            }
            
        }

        private string firstOpRelacao(string palavra)
        {
            if (palavra.ElementAt(0).Equals('=') || palavra.ElementAt(0).Equals('>') || palavra.ElementAt(0).Equals('<') || palavra.ElementAt(0).Equals('!'))
            {
                Automato automato = new Automato();
                return automato.AutomatoOPRelacao(palavra);
            }
            else
            {
                return null;
            }

        }

        private string firstNum(string palavra)
        {
            if (palavra.ElementAt(0) > 47 && palavra.ElementAt(0) <= 57)
            {
                Automato automato = new Automato();
                return automato.automatoNum(palavra);
            }
            else
            {
                return null;
            }

        }

        private string firstSomaMenos(string palavra)
        {
            if (palavra.ElementAt(0) == 43 || palavra.ElementAt(0) == 45)
            {
                return palavra + " eh t_opsomamenos \n";
            }
            else
            {
                return null;
            }

        }

        private string firstMultDiv(string palavra)
        {
            if (palavra.ElementAt(0) == 42 || palavra.ElementAt(0) == 92)
            {
                return palavra + " eh t_opmultdiv \n";
            }
            else
            {
                return null;
            }

        }

        private string firstOpComparacao(string palavra)
        {
            if (palavra.ElementAt(0).Equals('&') || palavra.ElementAt(0).Equals('|') || palavra.ElementAt(0).Equals('!'))
            {
                Automato automato = new Automato();
                return automato.AutomatoOPComparacao(palavra);
            }
            else
            {
                return null;
            }

        }



    }
}
