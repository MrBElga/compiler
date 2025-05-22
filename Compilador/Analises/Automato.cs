using System.Text.RegularExpressions;

namespace Compilador.Analises
{
    internal class Automato
    {
        public Automato()
        { }

        public string AutomatoID(string palavra)
        {
            if (!char.IsLetter(palavra[0]) || !char.IsLower(palavra[0]))
                return $"ERRO : Identificador inválido '{palavra}' (deve iniciar com letra minúscula)";

            for (int i = 1; i < palavra.Length; i++)
            {
                if (!char.IsLetterOrDigit(palavra[i]))
                    return $"ERRO : Identificador inválido '{palavra}' (contém caractere não permitido)";
            }
            return $"{palavra} eh t_id";
        }

        public string AutomatoOPRelacao(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "==": return $"{palavra} eh t_igualdade";
                case "!=": return $"{palavra} eh t_diferenca";
                case "<": return $"{palavra} eh t_menor";
                case ">": return $"{palavra} eh t_maior";
                case "<=": return $"{palavra} eh t_menor_igual";
                case ">=": return $"{palavra} eh t_maior_igual";
                default: return null;
            }
        }

        public string automatoNum(string palavra)
        {
            if (Regex.IsMatch(palavra, @"^\d+$"))
            {
                return $"{palavra} eh t_numero_int";
            }
            if (Regex.IsMatch(palavra, @"^\d+\.\d+$"))
            {
                return $"{palavra} eh t_numero_real";
            }

            return $"ERRO : Numero inválido '{palavra}'";
        }

        public string AutomatoOPComparacao(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "&&": return $"{palavra} eh t_logico_e";
                case "||": return $"{palavra} eh t_logico_ou";
                case "!": return $"{palavra} eh t_logico_nao";
                default: return null;
            }
        }

        public string AutomatoOpAddSub(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "+": return $"{palavra} eh t_soma";
                case "-": return $"{palavra} eh t_subtracao";
                default: return null;
            }
        }

        public string AutomatoOpMulDiv(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "*": return $"{palavra} eh t_multiplicacao";
                case "/": return $"{palavra} eh t_divisao";
                default: return null;
            }
        }
    }
}