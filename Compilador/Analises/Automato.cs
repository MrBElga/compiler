using System;
using System;
using System.IO;
using System.Text.RegularExpressions;


namespace Compilador.Analises
{
    internal class Automato
    {
        public Automato() { }

        public string AutomatoID(string palavra)
        {
            if (!char.IsLetter(palavra[0]) || !char.IsLower(palavra[0]))
                return "ERRO : '" + palavra + "' não pertence à Gramática\n";

            for (int i = 1; i < palavra.Length; i++)
            {
                if (!char.IsLetterOrDigit(palavra[i]))
                    return "ERRO : '" + palavra + "' não pertence à Gramática\n";
            }
            return palavra + " eh t_id\n";
        }

        public string AutomatoOPRelacao(string palavra)
        {
            if (palavra.Length < 1) return "ERRO : '" + palavra + "' não pertence à Gramática\n";

            switch (palavra[0])
            {
                case '=':
                    if (palavra.Length == 1) return palavra + " eh t_atribuicao\n";
                    if (palavra.Length == 2 && palavra[1] == '=') return palavra + " eh t_igual\n";
                    break;
                case '<':
                    if (palavra.Length == 1) return palavra + " eh t_menor\n";
                    if (palavra.Length == 2 && palavra[1] == '=') return palavra + " eh t_menorigual\n";
                    break;
                case '>':
                    if (palavra.Length == 1) return palavra + " eh t_maior\n";
                    if (palavra.Length == 2 && palavra[1] == '=') return palavra + " eh t_maiorigual\n";
                    break;
                case '!':
                    if (palavra.Length == 2 && palavra[1] == '=') return palavra + " eh t_dif\n";
                    break;
            }
            return "ERRO : '" + palavra + "' não pertence à Gramática\n";
        }

        public string automatoNum(string palavra)
        {
            if (!char.IsDigit(palavra[0])) return "ERRO : '" + palavra + "' não pertence à Gramática\n";

            int i = 0;
            bool hasDot = false;
            bool hasE = false;

            while (i < palavra.Length)
            {
                if (palavra[i] == '.' && !hasDot && i + 1 < palavra.Length && char.IsDigit(palavra[i + 1]))
                {
                    hasDot = true;
                    i++;
                    continue;
                }
                if (palavra[i] == 'E' && !hasE && i + 1 < palavra.Length)
                {
                    hasE = true;
                    i++;
                    if (i < palavra.Length && (palavra[i] == '+' || palavra[i] == '-'))
                        i++;
                    if (i >= palavra.Length || !char.IsDigit(palavra[i]))
                        return "ERRO : '" + palavra + "' não pertence à Gramática\n";
                }
                if (!char.IsDigit(palavra[i]))
                    return "ERRO : '" + palavra + "' não pertence à Gramática\n";
                i++;
            }
            return palavra + " eh t_num\n";
        }

        public string AutomatoOPComparacao(string palavra)
        {
            if (palavra.Length < 1) return "ERRO : '" + palavra + "' não pertence à Gramática\n";

            switch (palavra[0])
            {
                case '&':
                    if (palavra.Length == 2 && palavra[1] == '&') return palavra + " eh t_and\n";
                    break;
                case '|':
                    if (palavra.Length == 2 && palavra[1] == '|') return palavra + " eh t_or\n";
                    break;
                case '!':
                    if (palavra.Length == 1) return palavra + " eh t_not\n";
                    break;
            }
            return "ERRO : '" + palavra + "' não pertence à Gramática\n";
        }
    }
}