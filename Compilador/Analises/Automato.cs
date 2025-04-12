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
            if (string.IsNullOrEmpty(palavra)) return $"ERRO : Operador relacional inválido ''"; // Removido \n

            switch (palavra) // Verifica a string inteira
            {
                case "==": return $"{palavra} eh t_igualdade"; // Removido \n
                case "!=": return $"{palavra} eh t_diferenca"; // Removido \n
                case "<": return $"{palavra} eh t_menor";       // Removido \n
                case ">": return $"{palavra} eh t_maior";       // Removido \n
                case "<=": return $"{palavra} eh t_menor_igual"; // Removido \n
                case ">=": return $"{palavra} eh t_maior_igual"; // Removido \n
            }
            // Se não for nenhum dos operadores válidos completos
            return $"ERRO : Operador relacional inválido '{palavra}'"; // Removido \n
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

        public string AutomatoOPComparacao(string palavra) // Renomeado para AutomatoOPLogico?
        {
            if (string.IsNullOrEmpty(palavra)) return $"ERRO : Operador lógico inválido ''"; // Removido \n

            switch (palavra) // Verifica a string inteira
            {
                case "&&": return $"{palavra} eh t_logico_e"; // Removido \n
                case "||": return $"{palavra} eh t_logico_ou"; // Removido \n
                case "!": return $"{palavra} eh t_logico_nao"; // Removido \n (Nota: conflito com != se o lexer não separar bem)
            }
            return $"ERRO : Operador lógico inválido '{palavra}'"; // Removido \n
        }

        // NOVO: Método para operadores de Adição/Subtração
        public string AutomatoOpAddSub(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return $"ERRO : Operador inválido ''";

            switch (palavra)
            {
                case "+": return $"{palavra} eh t_soma";
                case "-": return $"{palavra} eh t_subtracao";
            }
            return $"ERRO : Operador de adição/subtração inválido '{palavra}'";
        }

        // NOVO: Método para operadores de Multiplicação/Divisão
        public string AutomatoOpMulDiv(string palavra)
        {
            if (string.IsNullOrEmpty(palavra)) return $"ERRO : Operador inválido ''";

            switch (palavra)
            {
                case "*": return $"{palavra} eh t_multiplicacao";
                case "/": return $"{palavra} eh t_divisao";
            }
            return $"ERRO : Operador de multiplicação/divisão inválido '{palavra}'";
        }
    }
}