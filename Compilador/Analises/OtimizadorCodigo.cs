using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Analises
{
    public static class OtimizadorCodigo
    {
        public static List<string> Otimizar(List<string> codigoIntermediario)
        {
            var otimizado = new List<string>(codigoIntermediario);

            RemoverAtribuicoesInuteis(otimizado);
            PropagarConstantes(otimizado);
            CalculoConstante(otimizado);
            EliminarSubexpressoesComuns(otimizado);
            RemoverSaltosInuteis(otimizado);
            SimplificarAritmeticaTrivial(otimizado);
            PropagarCopias(otimizado);

            return otimizado;
        }

        private static void RemoverAtribuicoesInuteis(List<string> linhas)
        {
            var atribuicoes = new Dictionary<string, int>();
            for (int i = 0; i < linhas.Count; i++)
            {
                var partes = linhas[i].Split('=');
                if (partes.Length == 2 && !linhas[i].Contains("if") && !linhas[i].Contains("goto"))
                {
                    string destino = partes[0].Trim();
                    atribuicoes[destino] = i;
                }
            }
            var usados = new HashSet<string>();
            foreach (var linha in linhas)
            {
                foreach (var varPossivel in atribuicoes.Keys)
                {
                    if (linha.Contains(varPossivel) && !linha.TrimStart().StartsWith(varPossivel + " ="))
                        usados.Add(varPossivel);
                }
            }
            foreach (var kvp in atribuicoes)
            {
                if (!usados.Contains(kvp.Key))
                    linhas[kvp.Value] = "// REMOVIDO: " + linhas[kvp.Value];
            }
        }

        private static void PropagarConstantes(List<string> linhas)
        {
            var constantes = new Dictionary<string, string>();
            for (int i = 0; i < linhas.Count; i++)
            {
                var partes = linhas[i].Split('=');
                if (partes.Length == 2)
                {
                    string lhs = partes[0].Trim();
                    string rhs = partes[1].Trim();
                    if (decimal.TryParse(rhs, out _))
                    {
                        constantes[lhs] = rhs;
                    }
                    else if (constantes.TryGetValue(rhs, out string val))
                    {
                        linhas[i] = lhs + " = " + val;
                        constantes[lhs] = val;
                    }
                }
            }
        }

        private static void CalculoConstante(List<string> linhas)
        {
            for (int i = 0; i < linhas.Count; i++)
            {
                var m = System.Text.RegularExpressions.Regex.Match(linhas[i], @"(t\d+) = (\d+(\.\d+)?|\d+) ([+\-*/]) (\d+(\.\d+)?|\d+)");
                if (m.Success)
                {
                    string temp = m.Groups[1].Value;
                    string op1 = m.Groups[2].Value;
                    string oper = m.Groups[4].Value;
                    string op2 = m.Groups[5].Value;
                    try
                    {
                        decimal a = decimal.Parse(op1);
                        decimal b = decimal.Parse(op2);
                        decimal resultado;

                        if (oper == "+") resultado = a + b;
                        else if (oper == "-") resultado = a - b;
                        else if (oper == "*") resultado = a * b;
                        else if (oper == "/") resultado = a / b;
                        else resultado = 0;

                        linhas[i] = $"{temp} = {resultado}";
                    }
                    catch { }
                }
            }
        }

        private static void EliminarSubexpressoesComuns(List<string> linhas)
        {
            var exprMap = new Dictionary<string, string>();
            for (int i = 0; i < linhas.Count; i++)
            {
                var m = System.Text.RegularExpressions.Regex.Match(linhas[i], @"(t\d+) = (.+)");
                if (m.Success)
                {
                    string temp = m.Groups[1].Value;
                    string expr = m.Groups[2].Value.Trim();
                    if (exprMap.ContainsKey(expr))
                    {
                        linhas[i] = temp + " = " + exprMap[expr];
                    }
                    else
                    {
                        exprMap[expr] = temp;
                    }
                }
            }
        }

        private static void RemoverSaltosInuteis(List<string> linhas)
        {
            for (int i = 0; i < linhas.Count - 1; i++)
            {
                string linha = linhas[i].Trim();
                if (linha.StartsWith("goto "))
                {
                    string destino = linha.Substring(5).Trim();
                    string proxima = linhas[i + 1].Trim();
                    if (proxima == destino + ":")
                    {
                        linhas[i] = "// REMOVIDO: " + linhas[i];
                    }
                }
            }
        }

        private static void SimplificarAritmeticaTrivial(List<string> linhas)
        {
            for (int i = 0; i < linhas.Count; i++)
            {
                linhas[i] = linhas[i].Replace(" * 1", "");
                linhas[i] = linhas[i].Replace(" + 0", "");
                linhas[i] = linhas[i].Replace(" - 0", "");
                linhas[i] = linhas[i].Replace(" / 1", "");
            }
        }

        private static void PropagarCopias(List<string> linhas)
        {
            var copias = new Dictionary<string, string>();
            for (int i = 0; i < linhas.Count; i++)
            {
                var m = System.Text.RegularExpressions.Regex.Match(linhas[i], @"(\w+) = (\w+)");
                if (m.Success)
                {
                    string lhs = m.Groups[1].Value;
                    string rhs = m.Groups[2].Value;
                    if (!rhs.All(char.IsDigit))
                    {
                        copias[lhs] = rhs;
                    }
                }
            }

            for (int i = 0; i < linhas.Count; i++)
            {
                foreach (var kvp in copias)
                {
                    if (linhas[i].Contains(kvp.Key + " ") && !linhas[i].StartsWith(kvp.Key + " ="))
                    {
                        linhas[i] = linhas[i].Replace(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }
}
