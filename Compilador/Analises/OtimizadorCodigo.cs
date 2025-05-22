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

      
            RemoverAtribuicoesIdentidade(otimizado); 
            RemoverAtribuicoesInuteis(otimizado);
            PropagarConstantes(otimizado);
            CalculoConstante(otimizado);
            EliminarSubexpressoesComuns(otimizado);
            RemoverSaltosInuteis(otimizado);
            SimplificarAritmeticaTrivial(otimizado);
            PropagarCopias(otimizado); 
            return otimizado;
        }

        private static void RemoverAtribuicoesIdentidade(List<string> linhas)
        {
            for (int i = linhas.Count - 1; i >= 0; i--)
            {
                if (linhas[i] == null) continue; 
                if (linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                var m = System.Text.RegularExpressions.Regex.Match(linhas[i], @"^\s*(\w+)\s*=\s*\1\s*;?\s*$");
                if (m.Success)
                {
                    linhas.RemoveAt(i); // Remove a linha fisicamente
                }
            }
        }
        // Em OtimizadorCodigo.cs
        private static void RemoverAtribuicoesInuteis(List<string> linhas)
        {
            var atribuicoes = new Dictionary<string, int>();
            for (int i = 0; i < linhas.Count; i++)
            {
                if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                var partes = linhas[i].Split('=');
                if (partes.Length == 2 && !linhas[i].Contains("if") && !linhas[i].Contains("goto"))
                {
                    string destino = partes[0].Trim();
                    atribuicoes[destino] = i;
                }
            }

            var usados = new HashSet<string>();
            for (int i = 0; i < linhas.Count; i++)
            {
                string linhaAnalisada = linhas[i];
                if (linhaAnalisada == null || linhaAnalisada.TrimStart().StartsWith("// REMOVIDO")) continue;

                string lhsDaLinha = null;
                var matchDef = System.Text.RegularExpressions.Regex.Match(linhaAnalisada.TrimStart(), @"^\s*(\w+)\s*=");
                if (matchDef.Success)
                {
                    lhsDaLinha = matchDef.Groups[1].Value;
                }

                foreach (var varPossivel in atribuicoes.Keys)
                {
                    string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(varPossivel) + @"\b";
                    if (System.Text.RegularExpressions.Regex.IsMatch(linhaAnalisada, pattern))
                    {
                        if (varPossivel != lhsDaLinha)
                        {
                            usados.Add(varPossivel);
                        }
                        else
                        {
                            string rhsDaLinha = "";
                            int eqIndex = linhaAnalisada.IndexOf('=');
                            if (eqIndex != -1 && eqIndex + 1 < linhaAnalisada.Length)
                            {
                                rhsDaLinha = linhaAnalisada.Substring(eqIndex + 1);
                            }
                            if (System.Text.RegularExpressions.Regex.IsMatch(rhsDaLinha, pattern))
                            {
                                usados.Add(varPossivel);
                            }
                        }
                    }
                }
            }

            // Coleta os índices das linhas a serem removidas
            var indicesParaRemover = new List<int>();
            foreach (var kvp in atribuicoes)
            {
                if (!usados.Contains(kvp.Key))
                {
         
                    if (kvp.Value < linhas.Count &&
                        linhas[kvp.Value] != null &&
                        !linhas[kvp.Value].TrimStart().StartsWith("// REMOVIDO"))
                    {
                        indicesParaRemover.Add(kvp.Value);
                    }
                }
            }

            // Ordena os índices em ordem decrescente para remoção segura
            indicesParaRemover.Sort((a, b) => b.CompareTo(a));

            // Remove as linhas
            foreach (int indice in indicesParaRemover)
            {
                linhas.RemoveAt(indice);
            }
        }

        private static void PropagarConstantes(List<string> linhas)
        {
            var constantes = new Dictionary<string, string>();
            for (int i = 0; i < linhas.Count; i++)
            {
                if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                var partes = linhas[i].Split('=');
                if (partes.Length == 2)
                {
                    string lhs = partes[0].Trim();
                    string rhs = partes[1].Trim();
                    // Remove ponto e vírgula do RHS, se houver, para TryParse e armazenamento
                    rhs = rhs.TrimEnd(';');

                    if (decimal.TryParse(rhs, out _))
                    {
                        constantes[lhs] = rhs;
                    }
                    else
                    {
                        // Tenta propagar se RHS é uma variável que já é constante
                        // Também remove ponto e vírgula de 'rhs' antes de procurar em 'constantes'
                        string rhsVar = rhs.Trim(); // rhs já foi Trim()
                        if (constantes.TryGetValue(rhsVar, out string val))
                        {
                            linhas[i] = lhs + " = " + val + (linhas[i].Trim().EndsWith(";") ? ";" : ""); // Mantém o ; se existia
                            constantes[lhs] = val; // Atualiza que lhs agora também é essa constante
                        }
                    }
                }
            }
        }

        private static void CalculoConstante(List<string> linhas)
        {
            for (int i = 0; i < linhas.Count; i++)
            {
                if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                var m = System.Text.RegularExpressions.Regex.Match(linhas[i], @"(t\d+)\s*=\s*(\d+(?:\.\d+)?|\d+)\s*([+\-*/])\s*(\d+(?:\.\d+)?|\d+)");
                if (m.Success)
                {
                    string temp = m.Groups[1].Value;
                    string op1_str = m.Groups[2].Value;
                    string oper = m.Groups[3].Value;
                    string op2_str = m.Groups[4].Value;
                    try
                    {
                        decimal a = decimal.Parse(op1_str, System.Globalization.CultureInfo.InvariantCulture);
                        decimal b = decimal.Parse(op2_str, System.Globalization.CultureInfo.InvariantCulture);
                        decimal resultado = 0; // Inicialização padrão

                        switch (oper)
                        {
                            case "+": resultado = a + b; break;
                            case "-": resultado = a - b; break;
                            case "*": resultado = a * b; break;
                            case "/":
                                if (b != 0) resultado = a / b;
                                else { /* Divisão por zero, não otimizar ou tratar como erro */ continue; }
                                break;
                        }
                        linhas[i] = $"{temp} = {resultado.ToString(System.Globalization.CultureInfo.InvariantCulture)}" + (linhas[i].Trim().EndsWith(";") ? ";" : "");
                    }
                    catch (FormatException) { /* Erro ao parsear, não otimizar */ }
                    catch (OverflowException) { /* Resultado muito grande, não otimizar */ }
                }
            }
        }

        private static void EliminarSubexpressoesComuns(List<string> linhas)
        {
            var exprMap = new Dictionary<string, string>();
            for (int i = 0; i < linhas.Count; i++)
            {
                if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                // Regex para capturar "tX = expressao_qualquer"
                // A expressão pode conter espaços, então (.+) é usado.
                var m = System.Text.RegularExpressions.Regex.Match(linhas[i], @"^\s*(t\d+)\s*=\s*(.+)$");
                if (m.Success)
                {
                    string tempDefinido = m.Groups[1].Value;
                    string expressao = m.Groups[2].Value.Trim();
                    // Remove ponto e vírgula do final da expressão, se houver
                    expressao = expressao.TrimEnd(';');

                    if (exprMap.TryGetValue(expressao, out string tempExistente))
                    {
                        // Se a expressão já foi calculada e armazenada em tempExistente,
                        // e tempExistente não é o mesmo que tempDefinido (evitar t1 = t1)
                        if (tempDefinido != tempExistente)
                        {
                            linhas[i] = tempDefinido + " = " + tempExistente + (linhas[i].Trim().EndsWith(";") ? ";" : "");
                        }
                    }
                    else
                    {
                        exprMap[expressao] = tempDefinido;
                    }
                }
            }
        }

        private static void RemoverSaltosInuteis(List<string> linhas)
        {
            for (int i = 0; i < linhas.Count - 1; i++)
            {
                if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;
                if (linhas[i + 1] == null) continue; // Linha seguinte pode ter sido removida

                string linhaAtual = linhas[i].Trim();
                if (linhaAtual.StartsWith("goto "))
                {
                    string destinoSalto = linhaAtual.Substring(5).Trim();
                    destinoSalto = destinoSalto.TrimEnd(';');

                    string proximaLinhaLabel = linhas[i + 1].Trim();
                    // Verifica se a proximaLinhaLabel é exatamente "destinoSalto:"
                    if (proximaLinhaLabel == destinoSalto + ":")
                    {
                        linhas[i] = "// REMOVIDO (SALTO INUTIL): " + linhas[i];
                    }
                }
            }
        }

        private static void SimplificarAritmeticaTrivial(List<string> linhas)
        {
            for (int i = 0; i < linhas.Count; i++)
            {
                if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                // Usar Regex para substituições mais seguras e precisas
                // x = y * 1  -> x = y
                linhas[i] = System.Text.RegularExpressions.Regex.Replace(linhas[i], @"\s*\*\s*1\b", "");
                // x = y + 0  -> x = y
                linhas[i] = System.Text.RegularExpressions.Regex.Replace(linhas[i], @"\s*\+\s*0\b", "");
                // x = y - 0  -> x = y
                linhas[i] = System.Text.RegularExpressions.Regex.Replace(linhas[i], @"\s*-\s*0\b", "");
                // x = y / 1  -> x = y
                linhas[i] = System.Text.RegularExpressions.Regex.Replace(linhas[i], @"\s*/\s*1\b", "");
            }
        }

        // Função auxiliar para PropagarCopias (deve estar na classe)
        private static bool VariavelRedefinidaEntre(string varNome, int linhaDaCopiaOriginal, int linhaDeUsoAtual, List<string> codigo)
        {
            for (int k = linhaDaCopiaOriginal + 1; k < linhaDeUsoAtual; k++)
            {
                if (codigo[k] == null || codigo[k].TrimStart().StartsWith("// REMOVIDO")) continue;

                string linhaK = codigo[k].Trim();
                var matchDef = System.Text.RegularExpressions.Regex.Match(linhaK, @"^\s*(\w+)\s*=");
                if (matchDef.Success && matchDef.Groups[1].Value == varNome)
                {
                    return true;
                }
            }
            return false;
        }

        // Versão mais recente e corrigida de PropagarCopias
        private static void PropagarCopias(List<string> linhas)
        {
            bool houveMudancaGeralNaPassada;
            do
            {
                houveMudancaGeralNaPassada = false;
                for (int i = 0; i < linhas.Count; i++)
                {
                    if (linhas[i] == null || linhas[i].TrimStart().StartsWith("// REMOVIDO")) continue;

                    var matchCopia = System.Text.RegularExpressions.Regex.Match(linhas[i], @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*;?\s*$");

                    if (matchCopia.Success)
                    {
                        string lhsCopia = matchCopia.Groups[1].Value;
                        string rhsCopia = matchCopia.Groups[2].Value;

                        if (lhsCopia == rhsCopia)
                            continue;

                        if (decimal.TryParse(rhsCopia, out _))
                            continue;

                        for (int j = i + 1; j < linhas.Count; j++)
                        {
                            if (linhas[j] == null || linhas[j].TrimStart().StartsWith("// REMOVIDO")) continue;

                            if (VariavelRedefinidaEntre(lhsCopia, i, j, linhas))
                                break;

                            if (VariavelRedefinidaEntre(rhsCopia, i, j, linhas))
                                break;

                            string linhaDeUsoAtual = linhas[j];
                            string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(lhsCopia) + @"\b";
                            var matchDefNaLinhaDeUso = System.Text.RegularExpressions.Regex.Match(linhaDeUsoAtual, @"^\s*(\w+)\s*=");
                            bool lhsCopiaEhDefinidoNestaLinha = matchDefNaLinhaDeUso.Success && matchDefNaLinhaDeUso.Groups[1].Value == lhsCopia;

                            if (!lhsCopiaEhDefinidoNestaLinha && System.Text.RegularExpressions.Regex.IsMatch(linhaDeUsoAtual, pattern))
                            {
                                // Preserva o ponto e vírgula se a linha original o possuía
                                bool endsWithSemicolon = linhaDeUsoAtual.Trim().EndsWith(";");
                                string linhaModificadaSemSemicolon = System.Text.RegularExpressions.Regex.Replace(linhaDeUsoAtual.TrimEnd(';'), pattern, rhsCopia);
                                string linhaModificada = linhaModificadaSemSemicolon + (endsWithSemicolon ? ";" : "");

                                if (linhas[j] != linhaModificada)
                                {
                                    linhas[j] = linhaModificada;
                                    houveMudancaGeralNaPassada = true;
                                }
                            }
                        }
                    }
                }
            } while (houveMudancaGeralNaPassada);
        }
    }
}