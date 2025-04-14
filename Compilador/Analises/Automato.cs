// Automato.cs (Ajustado)
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
            // Valida ID (começa com minúscula, seguido por letras/dígitos)
            if (!char.IsLetter(palavra[0]) || !char.IsLower(palavra[0]))
                return $"ERRO : Identificador inválido '{palavra}' (deve iniciar com letra minúscula)"; // Removido \n

            for (int i = 1; i < palavra.Length; i++)
            {
                if (!char.IsLetterOrDigit(palavra[i]))
                    return $"ERRO : Identificador inválido '{palavra}' (contém caractere não permitido)"; // Removido \n
            }
            return $"{palavra} eh t_id"; // Removido \n
        }

        public string AutomatoOPRelacao(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null; // Ou erro se preferir, mas null é mais limpo aqui
            switch (palavra)
            {
                case "==": return $"{palavra} eh t_igualdade";
                case "!=": return $"{palavra} eh t_diferenca";
                case "<": return $"{palavra} eh t_menor";
                case ">": return $"{palavra} eh t_maior";
                case "<=": return $"{palavra} eh t_menor_igual";
                case ">=": return $"{palavra} eh t_maior_igual";
                default: return null; // <--- RETORNA NULL SE NÃO FOR NENHUM DESTES
            }
        }

        public string automatoNum(string palavra)
        {
            // Usando Regex para simplificar a validação de int/float
            // Permite inteiros (123) ou floats (123.456)
            // Não permite '.' inicial ou final, nem múltiplos '.'
            // Não trata notação científica 'E' aqui, adicione se necessário
            if (Regex.IsMatch(palavra, @"^\d+$")) // Inteiro
            {
                return $"{palavra} eh t_numero_int"; // Removido \n
            }
            if (Regex.IsMatch(palavra, @"^\d+\.\d+$")) // Float simples
            {
                return $"{palavra} eh t_numero_real"; // Removido \n
            }

            return $"ERRO : Numero inválido '{palavra}'"; // Removido \n
        }

        public string AutomatoOPComparacao(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "&&": return $"{palavra} eh t_logico_e";
                case "||": return $"{palavra} eh t_logico_ou";
                case "!": return $"{palavra} eh t_logico_nao";
                default: return null; // <--- RETORNA NULL SE NÃO FOR NENHUM DESTES
            }
        }

        // NOVO: Método para operadores de Adição/Subtração
        public string AutomatoOpAddSub(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "+": return $"{palavra} eh t_soma";
                case "-": return $"{palavra} eh t_subtracao";
                default: return null; // <--- RETORNA NULL SE NÃO FOR NENHUM DESTES
            }
        }

        // NOVO: Método para operadores de Multiplicação/Divisão
        public string AutomatoOpMulDiv(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return null;
            switch (palavra)
            {
                case "*": return $"{palavra} eh t_multiplicacao";
                case "/": return $"{palavra} eh t_divisao";
                default: return null; // <--- RETORNA NULL SE NÃO FOR NENHUM DESTES
            }
        }
    }
}