using System;
using System.Collections.Generic;
using System.Globalization;
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
        private int constCount = 0;

        public GeradorCodigoSimpSIM(List<string> codigoOtimizado)
        {
            this.codigoIntermediarioOtimizado = codigoOtimizado ?? throw new ArgumentNullException(nameof(codigoOtimizado));
            this.codigoSimpSIM = new List<string>();
            this.variaveisDeclaradas = new HashSet<string>();
        }

        private string NovoRotuloSimpSIM() => $"LL{tempLabelCount++}";
        private string NovaConstLabel() => $"C{constCount++}";

        private string RegistrarVariavel(string nomeVar)
        {
            if (string.IsNullOrWhiteSpace(nomeVar)) return nomeVar;

            if (decimal.TryParse(nomeVar, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) return nomeVar;

            string lowerNomeVar = nomeVar.ToLower();
            if (lowerNomeVar == "goto" || lowerNomeVar == "iffalse" || nomeVar.EndsWith(":") || (nomeVar.StartsWith("L") && nomeVar.Length > 1 && nomeVar.Substring(1).All(char.IsDigit)))
                return nomeVar;

            if (Regex.IsMatch(nomeVar, @"^R[0-9A-F]$", RegexOptions.IgnoreCase)) return nomeVar;


            if (!variaveisDeclaradas.Contains(nomeVar))
            {
                variaveisDeclaradas.Add(nomeVar);
            }
            return nomeVar;
        }

        private string RegistrarConstanteComoVariavel(string valorConstante)
        {
            string constLabel = "CONST_" + valorConstante.Replace(".", "p").Replace("-", "n");
            if (!variaveisDeclaradas.Contains(constLabel))
            {
                variaveisDeclaradas.Add(constLabel + " ; Valor original: " + valorConstante);
            }
            return constLabel;
        }


        public List<string> Gerar()
        {
            codigoSimpSIM.Clear();
            variaveisDeclaradas.Clear();
            tempLabelCount = 0;
            constCount = 0;

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

            codigoSimpSIM.Add("halt");

            if (variaveisDeclaradas.Any())
            {
                codigoSimpSIM.Add("\n; === Variaveis ===");
                foreach (string varDecl in variaveisDeclaradas.OrderBy(s => s))
                {
                    if (varDecl.StartsWith("CONST_"))
                    {
                        string originalValue = varDecl.Split(new[] { " ; Valor original: " }, StringSplitOptions.None).LastOrDefault()?.Trim()
                                                ?? varDecl.Substring("CONST_".Length).Replace("p", ".").Replace("n", "-");
                        codigoSimpSIM.Add(varDecl.Split(';').First().Trim() + ": db " + originalValue);
                    }
                    else
                    {
                        codigoSimpSIM.Add(varDecl + ": db 0");
                    }
                }
            }
            return codigoSimpSIM;
        }

        private void EmitLoad(string registrador, string operando)
        {
            if (decimal.TryParse(operando, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                codigoSimpSIM.Add($"load {registrador}, {operando}");
            }
            else
            {
                codigoSimpSIM.Add($"load {registrador}, [{RegistrarVariavel(operando)}]");
            }
        }

        private void EmitStore(string registrador, string variavelDestino)
        {
            codigoSimpSIM.Add($"store {registrador}, [{RegistrarVariavel(variavelDestino)}]");
        }

        private void EmitAdd(string regDest, string regOp1, string regOp2)
        {
            codigoSimpSIM.Add($"addi {regDest}, {regOp1}, {regOp2}");
        }

        private void EmitSubtract(string regDest, string regOp1, string regOp2, string tempReg1, string tempReg2)
        {
            string regForNegatedOp2 = tempReg1;
            if (regOp2 == tempReg1) regForNegatedOp2 = tempReg2;

            if (regOp2 != regForNegatedOp2)
            {
                codigoSimpSIM.Add($"move {regForNegatedOp2}, {regOp2}");
            }

            codigoSimpSIM.Add($"load {tempReg2}, -1");
            codigoSimpSIM.Add($"xor {regForNegatedOp2}, {regForNegatedOp2}, {tempReg2}");

            codigoSimpSIM.Add($"load {tempReg2}, 1");
            codigoSimpSIM.Add($"addi {regForNegatedOp2}, {regForNegatedOp2}, {tempReg2}");

            codigoSimpSIM.Add($"addi {regDest}, {regOp1}, {regForNegatedOp2}");
        }

        private void TraduzirLinha(string linhaCI)
        {
            if (string.IsNullOrWhiteSpace(linhaCI)) return;

            string[] partes = linhaCI.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return;

            codigoSimpSIM.Add("; CI: " + linhaCI);

            if (partes.Length == 1 && partes[0].EndsWith(":"))
            {
                codigoSimpSIM.Add(partes[0]);
            }
            else if (partes.Length == 2 && partes[0].ToLower() == "goto")
            {
                codigoSimpSIM.Add("jmp " + partes[1]);
            }
            else if (partes.Length == 3 && partes[1] == "=")
            {
                string dest = partes[0];
                string fonte = partes[2];
                EmitLoad("R1", fonte);
                EmitStore("R1", dest);
            }
            else if (partes.Length == 5 && partes[1] == "=")
            {
                string dest = partes[0];
                string op1_str = partes[2];
                string operador = partes[3];
                string op2_str = partes[4];

                EmitLoad("R1", op1_str);
                EmitLoad("R2", op2_str);

                switch (operador)
                {
                    case "+":
                        EmitAdd("R1", "R1", "R2");
                        break;
                    case "-":
                        EmitSubtract("R1", "R1", "R2", "R3", "R4");
                        break;
                    case "*":
                    case "/":
                        codigoSimpSIM.Add($"; ERRO SimpSIM: Operador '{operador}' nao suportado diretamente.");
                        EmitLoad("R1", "0");
                        break;
                    case "<":
                    case "<=":
                    case ">":
                    case ">=":
                    case "==":
                    case "!=":
                        EmitSubtract("R1", "R1", "R2", "R3", "R4");

                        string labelSetTrue = NovoRotuloSimpSIM();
                        string labelCondFim = NovoRotuloSimpSIM();

                        codigoSimpSIM.Add("load R0, 0");

                        switch (operador)
                        {
                            case "<":
                                codigoSimpSIM.Add("jmpEQ R1=R0, " + labelCondFim);
                                codigoSimpSIM.Add("jmpLE R1<=R0, " + labelSetTrue);
                                break;
                            case "<=":
                                codigoSimpSIM.Add("jmpLE R1<=R0, " + labelSetTrue);
                                break;
                            case ">":
                                codigoSimpSIM.Add("jmpLE R1<=R0, " + labelCondFim);
                                break;
                            case ">=":
                                string notGeLabel = NovoRotuloSimpSIM();
                                codigoSimpSIM.Add("jmpEQ R1=R0, " + labelSetTrue);
                                codigoSimpSIM.Add("jmpLE R1<=R0, " + notGeLabel);
                                codigoSimpSIM.Add("jmp " + labelSetTrue);
                                codigoSimpSIM.Add(notGeLabel + ":");
                                break;
                            case "==":
                                codigoSimpSIM.Add("jmpEQ R1=R0, " + labelSetTrue);
                                break;
                            case "!=":
                                codigoSimpSIM.Add("jmpEQ R1=R0, " + labelCondFim);
                                break;
                        }
                        codigoSimpSIM.Add("load R1, 0");
                        codigoSimpSIM.Add("jmp " + labelCondFim);
                        codigoSimpSIM.Add(labelSetTrue + ":");
                        codigoSimpSIM.Add("load R1, 1");
                        codigoSimpSIM.Add(labelCondFim + ":");
                        break;
                    default:
                        codigoSimpSIM.Add($"; ERRO SimpSIM: Operador binário desconhecido '{operador}'.");
                        EmitLoad("R1", "0");
                        break;
                }
                EmitStore("R1", dest);
            }
            else if (partes.Length == 4 && partes[0].ToLower() == "iffalse")
            {
                string varCondicional = partes[1];
                string labelDestino = partes[3];

                EmitLoad("R1", varCondicional);
                codigoSimpSIM.Add("load R0, 0");
                codigoSimpSIM.Add("jmpEQ R1=R0, " + labelDestino);
            }
            else
            {
                codigoSimpSIM.Add("; Traducao SimpSIM nao implementada: " + linhaCI);
            }
        }
    }
}