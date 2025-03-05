using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analises
{
    internal class Automato
    {

        public Automato() { }


        public string AutomatoID(string palavra)
        {
            for (int i = 0; i < palavra.Length; i++)
            {
                if ((palavra.ElementAt(0) > 96 && palavra.ElementAt(0) <= 122) || (palavra.ElementAt(0) > 47 && palavra.ElementAt(0) <= 57))
                {
                    i++;
                }
                else
                {
                    return "ERRO :'" + palavra + "' não percente a Gramatica \n";
                }
            }
            return palavra + " eh t_id \n";
        }

        public string AutomatoOPRelacao(string palavra)
        {
            int i = 0;

            if (palavra[i].Equals('='))
            {
                i++;
                if (i < palavra.Length)
                {
                    if (palavra[i].Equals('='))
                    {
                        return palavra + " eh t_igual \n";
                    }
                    else
                    {
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    }

                }
                else
                {
                    return palavra + " eh t_maior\n";
                }
            }

            if (palavra[i].Equals('<'))
            {
                i++;
                if (i < palavra.Length)
                {
                    if (palavra[i].Equals('='))
                    {
                        return palavra + " eh t_menorigual \n";
                    }
                    else
                    {
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    }

                }
                else
                {
                    return palavra + " eh t_menor\n";
                }


            }

            if (palavra[i].Equals('>'))
            {
                i++;
                if (i < palavra.Length)
                {
                    if (palavra[i].Equals('='))
                    {
                        return palavra + " eh t_maiorigual \n";
                    }
                    else
                    {
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    }

                }
                else
                {
                    return palavra + " eh t_maior\n";
                }
            }
            if (palavra[i].Equals('!'))
            {
                i++;
                if (i < palavra.Length)
                {
                    if (palavra[i].Equals('='))
                    {
                        return palavra + " eh t_dif \n";
                    }
                    else
                    {
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    }

                }
                else
                {
                    return null;
                }
            }

            return "ERRO : '" + palavra + "' não percente a Gramatica \n";
        }

        public string automatoNum(string palavra)
        {
            if (palavra.ElementAt(0) > 47 && palavra.ElementAt(0) <= 57)
            {
                int i = 1;
                while (i < palavra.Length && !palavra.ElementAt(i).Equals('.') && !palavra.ElementAt(i).Equals('E'))
                {
                    if (palavra.ElementAt(i) <= 47 || palavra.ElementAt(i) > 57)
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    i++;
                }

                if (i < palavra.Length)
                {
                    while (i < palavra.Length && !palavra.ElementAt(i).Equals('E'))
                    {
                        if (palavra.ElementAt(i) <= 47 || palavra.ElementAt(i) > 57)
                            return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                        i++;
                    }
                    if (i < palavra.Length && palavra.ElementAt(i).Equals('E'))
                    {
                        i++;
                        if (palavra.ElementAt(i).Equals('+') || palavra.ElementAt(i).Equals('-') || palavra.ElementAt(i) > 47 && palavra.ElementAt(i) <= 57)
                        {
                            while (i < palavra.Length)
                            {
                                if (palavra.ElementAt(i) <= 47 || palavra.ElementAt(i) > 57)
                                    return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                                i++;
                            }
                            if (i == palavra.Length)
                            {
                                return palavra + " eh t_num \n";

                            }
                            else
                            {
                                return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                            }
                        }
                        else
                        {
                            return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                        }

                    }
                    else
                    {
                        return palavra + " eh t_num \n";
                    }
                }
                else
                {
                    return palavra + " eh t_num \n";
                }




            }

            return "ERRO : '" + palavra + "' não percente a Gramatica \n";
        }
        public string AutomatoOPComparacao(string palavra)
        {
            int i = 0;

            if (palavra[i].Equals('&'))
            {
                i++;
                if (i < palavra.Length)
                {
                    if (palavra[i].Equals('&'))
                    {
                        return palavra + " eh t_and \n";
                    }
                    else
                    {
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    }

                }
                else
                {
                    return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                }
            }
            else if (palavra[i].Equals('|'))
            {
                i++;
                if (i < palavra.Length)
                {
                    if (palavra[i].Equals('|'))
                    {
                        return palavra + " eh t_or \n";
                    }
                    else
                    {
                        return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                    }

                }
                else
                {
                    return "ERRO : '" + palavra + "' não percente a Gramatica \n";
                }
            }
            else if (palavra[i].Equals('!'))
            {
                return palavra + " eh t_not \n";
            }
            return "ERRO : '" + palavra + "' não percente a Gramatica \n";
        }
    }
}
