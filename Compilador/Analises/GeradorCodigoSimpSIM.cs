using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compilador.Analises
{
    public class GeradorCodigoSimpSIM
    {
        private readonly List<string> codigoIntermediarioOtimizado;
        private List<string> codigoSimpSIM;
        private HashSet<string> variaveisDeclaradas;
        private int tempLabelCount = 0;

        public GeradorCodigoSimpSIM(List<string> codigoOtimizado)
        {
            this.codigoIntermediarioOtimizado = codigoOtimizado ?? throw new ArgumentNullException(nameof(codigoOtimizado));
            this.codigoSimpSIM = new List<string>();
            this.variaveisDeclaradas = new HashSet<string>();
        }

        private string NovoRotuloSimpSIM() => $"LL{tempLabelCount++}";

        private string RegistrarVariavel(string nomeVar)
        {
            if (string.IsNullOrWhiteSpace(nomeVar)) return nomeVar;

            if (decimal.TryParse(nomeVar, out _)) return nomeVar;

            if (nomeVar.ToLower() == "goto" || nomeVar.ToLower() == "iffalse" || nomeVar.EndsWith(":") || nomeVar.StartsWith("L") && nomeVar.Substring(1).All(char.IsDigit))
                return nomeVar;

            if (!variaveisDeclaradas.Contains(nomeVar))
            {
                variaveisDeclaradas.Add(nomeVar);
            }
            return nomeVar;
        }

        public List<string> Gerar()
        {
            codigoSimpSIM.Clear();
            variaveisDeclaradas.Clear();
            tempLabelCount = 0;

            foreach (string linhaCIOriginal in codigoIntermediarioOtimizado)
            {
                string linhaCI = linhaCIOriginal.Trim();
                if (linhaCI.StartsWith("// REMOVIDO")) continue;

                string[] partes = linhaCI.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 0) continue;

                if (partes.Length >= 3 && partes[1] == "=")
                {
                    RegistrarVariavel(partes[0]);
                    if (partes.Length == 3)
                    {
                        RegistrarVariavel(partes[2]);
                    }
                    else if (partes.Length == 5)
                    {
                        RegistrarVariavel(partes[2]);
                        RegistrarVariavel(partes[4]);
                    }
                }
                else if (partes.Length == 4 && partes[0].ToLower() == "iffalse")
                {
                    RegistrarVariavel(partes[1]);
                }
            }

            foreach (string linhaCIOriginal in codigoIntermediarioOtimizado)
            {
                string linhaCI = linhaCIOriginal.Trim();
                if (linhaCI.StartsWith("// REMOVIDO"))
                {
                    codigoSimpSIM.Add("; " + linhaCI);
                    continue;
                }
                TraduzirLinha(linhaCI);
            }

            codigoSimpSIM.Add("HALT");

            if (variaveisDeclaradas.Any())
            {
                codigoSimpSIM.Add("\n; === Variaveis ===");
                foreach (string varNome in variaveisDeclaradas.OrderBy(s => s))
                {
                    codigoSimpSIM.Add(varNome + ": DC 0");
                }
            }

            return codigoSimpSIM;
        }

        private void TraduzirLinha(string linhaCI)
        {
            if (string.IsNullOrWhiteSpace(linhaCI)) return;

            string[] partes = linhaCI.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return;

            if (partes.Length == 1 && partes[0].EndsWith(":"))
            {
                codigoSimpSIM.Add(partes[0]);
            }
            else if (partes.Length == 2 && partes[0].ToLower() == "goto")
            {
                codigoSimpSIM.Add("JUMP " + partes[1]);
            }
            else if (partes.Length == 3 && partes[1] == "=")
            {
                string dest = partes[0];
                string fonte = partes[2];

                if (decimal.TryParse(fonte, out _))
                {
                    codigoSimpSIM.Add("LOADI " + fonte);
                }
                else
                {
                    codigoSimpSIM.Add("LOAD " + fonte);
                }
                codigoSimpSIM.Add("STORE " + dest);
            }
            else if (partes.Length == 5 && partes[1] == "=" && (partes[3] == "+" || partes[3] == "-" || partes[3] == "*" || partes[3] == "/"))
            {
                string dest = partes[0];
                string op1 = partes[2];
                string operador = partes[3];
                string op2 = partes[4];

                if (decimal.TryParse(op1, out _)) codigoSimpSIM.Add("LOADI " + op1);
                else codigoSimpSIM.Add("LOAD " + op1);

                string instrucaoSimpSIM = "";
                switch (operador)
                {
                    case "+": instrucaoSimpSIM = "ADD"; break;
                    case "-": instrucaoSimpSIM = "SUB"; break;
                    case "*": instrucaoSimpSIM = "MUL"; break;
                    case "/": instrucaoSimpSIM = "DIV"; break;
                }
                if (decimal.TryParse(op2, out _))
                {
                    if (instrucaoSimpSIM == "ADD") codigoSimpSIM.Add("ADDI " + op2);
                    else if (instrucaoSimpSIM == "SUB") codigoSimpSIM.Add("SUBI " + op2);
                    else if (instrucaoSimpSIM == "MUL") codigoSimpSIM.Add("MULI " + op2);
                    else if (instrucaoSimpSIM == "DIV") codigoSimpSIM.Add("DIVI " + op2);
                    else
                    {
                        string constLabel = "C_" + op2.Replace(".", "_").Replace("-", "N");
                        RegistrarVariavel(constLabel);
                        codigoSimpSIM.Add(instrucaoSimpSIM + " " + constLabel + " ; op2 era constante " + op2);
                    }
                }
                else
                {
                    codigoSimpSIM.Add(instrucaoSimpSIM + " " + op2);
                }
                codigoSimpSIM.Add("STORE " + dest);
            }
            else if (partes.Length == 4 && partes[0].ToLower() == "iffalse")
            {
                string varCondicional = partes[1];
                string labelDestino = partes[3];

                string definicaoDeVarCondicional = "";
                foreach (string linhaAnteriorCI in codigoIntermediarioOtimizado)
                {
                    string linhaTrimmed = linhaAnteriorCI.Trim();
                    if (linhaTrimmed.StartsWith("// REMOVIDO")) continue;

                    if (linhaTrimmed.StartsWith(varCondicional + " ="))
                    {
                        definicaoDeVarCondicional = linhaTrimmed;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(definicaoDeVarCondicional))
                {
                    var matchDefRelacional = Regex.Match(definicaoDeVarCondicional,
                        @"^\s*\w+\s*=\s*([a-zA-Z_0-9][a-zA-Z_0-9.-]*)\s*([<>=!]+)\s*([a-zA-Z_0-9][a-zA-Z_0-9.-]*)\s*;?\s*$");

                    var matchDefBooleanaSimples = Regex.Match(definicaoDeVarCondicional,
                        @"^\s*\w+\s*=\s*(0|1|true|false)\s*;?\s*$", RegexOptions.IgnoreCase);

                    if (matchDefRelacional.Success)
                    {
                        string opA_str = matchDefRelacional.Groups[1].Value;
                        string relOp = matchDefRelacional.Groups[2].Value;
                        string opB_str = matchDefRelacional.Groups[3].Value;

                        if (decimal.TryParse(opA_str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                            codigoSimpSIM.Add("LOADI " + opA_str);
                        else
                            codigoSimpSIM.Add("LOAD " + RegistrarVariavel(opA_str));

                        if (decimal.TryParse(opB_str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                            codigoSimpSIM.Add("SUBI " + opB_str);
                        else
                            codigoSimpSIM.Add("SUB " + RegistrarVariavel(opB_str));

                        switch (relOp)
                        {
                            case "<":
                                codigoSimpSIM.Add("JPOS " + labelDestino);
                                codigoSimpSIM.Add("JZERO " + labelDestino);
                                break;

                            case "<=":
                                codigoSimpSIM.Add("JPOS " + labelDestino);
                                break;

                            case ">":
                                codigoSimpSIM.Add("JNEG " + labelDestino);
                                codigoSimpSIM.Add("JZERO " + labelDestino);
                                break;

                            case ">=":
                                codigoSimpSIM.Add("JNEG " + labelDestino);
                                break;

                            case "==":
                                string skipLabelTrueEq = NovoRotuloSimpSIM();
                                codigoSimpSIM.Add("JZERO " + skipLabelTrueEq);
                                codigoSimpSIM.Add("JUMP " + labelDestino);
                                codigoSimpSIM.Add(skipLabelTrueEq + ":");
                                break;

                            case "!=":
                                codigoSimpSIM.Add("JZERO " + labelDestino);
                                break;

                            default:
                                codigoSimpSIM.Add("; ERRO SimpSIM: Operador relacional desconhecido '" + relOp + "' na definicao de " + varCondicional);
                                break;
                        }
                    }
                    else if (matchDefBooleanaSimples.Success)
                    {
                        codigoSimpSIM.Add("LOAD " + RegistrarVariavel(varCondicional));
                        codigoSimpSIM.Add("JZERO " + labelDestino);
                    }
                    else
                    {
                        codigoSimpSIM.Add("LOAD " + RegistrarVariavel(varCondicional));
                        codigoSimpSIM.Add("JZERO " + labelDestino);
                    }
                }
                else
                {
                    codigoSimpSIM.Add("LOAD " + RegistrarVariavel(varCondicional));
                    codigoSimpSIM.Add("JZERO " + labelDestino);
                }
            }
            else if (partes.Length == 5 && partes[1] == "=" && (partes[3] == "<" || partes[3] == "<=" || partes[3] == ">" || partes[3] == ">=" || partes[3] == "==" || partes[3] == "!="))
            {
                string dest = RegistrarVariavel(partes[0]);
                string opA_str = partes[2];
                string relOp = partes[3];
                string opB_str = partes[4];

                if (decimal.TryParse(opA_str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                    codigoSimpSIM.Add("LOADI " + opA_str);
                else
                    codigoSimpSIM.Add("LOAD " + RegistrarVariavel(opA_str));

                if (decimal.TryParse(opB_str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                    codigoSimpSIM.Add("SUBI " + opB_str);
                else
                    codigoSimpSIM.Add("SUB " + RegistrarVariavel(opB_str));

                string labelSetTrue = NovoRotuloSimpSIM();
                string labelCondFim = NovoRotuloSimpSIM();

                switch (relOp)
                {
                    case "<":
                        codigoSimpSIM.Add("JNEG " + labelSetTrue);
                        break;

                    case "<=":
                        codigoSimpSIM.Add("JNEG " + labelSetTrue);
                        codigoSimpSIM.Add("JZERO " + labelSetTrue);
                        break;

                    case ">":
                        codigoSimpSIM.Add("JPOS " + labelSetTrue);
                        break;

                    case ">=":
                        codigoSimpSIM.Add("JPOS " + labelSetTrue);
                        codigoSimpSIM.Add("JZERO " + labelSetTrue);
                        break;

                    case "==":
                        codigoSimpSIM.Add("JZERO " + labelSetTrue);
                        break;

                    case "!=":
                        codigoSimpSIM.Add("JPOS " + labelSetTrue);
                        codigoSimpSIM.Add("JNEG " + labelSetTrue);
                        break;

                    default:
                        codigoSimpSIM.Add("; ERRO SimpSIM: Operador relacional desconhecido '" + relOp + "' na atribuicao para " + dest);
                        codigoSimpSIM.Add(labelSetTrue + ":");
                        codigoSimpSIM.Add(labelCondFim + ":");
                        codigoSimpSIM.Add("LOADI 0 ; Valor de erro para " + dest);
                        codigoSimpSIM.Add("STORE " + dest);
                        return;
                }

                codigoSimpSIM.Add("LOADI 0       ; Condicao FALSA");
                codigoSimpSIM.Add("STORE " + dest);
                codigoSimpSIM.Add("JUMP " + labelCondFim);

                codigoSimpSIM.Add(labelSetTrue + ":");
                codigoSimpSIM.Add("LOADI 1       ; Condicao VERDADEIRA");
                codigoSimpSIM.Add("STORE " + dest);

                codigoSimpSIM.Add(labelCondFim + ":");
            }
            else
            {
                codigoSimpSIM.Add("; Traducao SimpSIM nao implementada: " + linhaCI);
            }
        }
    }
}