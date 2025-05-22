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

            // Adicionar constantes numéricas usadas com ADDI, SUBI, etc., se o SimpSIM não as suportar diretamente
            // e precisarem ser carregadas da memória. Esta parte é mais complexa e depende
            // de como você lida com constantes nas operações (ver TraduzirLinha).

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
                string dest = partes[0]; // RegistrarVariavel já foi chamado na passada inicial
                string fonte = partes[2];

                if (decimal.TryParse(fonte, out _)) // x = constante
                {
                    codigoSimpSIM.Add("LOADI " + fonte);
                }
                else // x = y (variável)
                {
                    codigoSimpSIM.Add("LOAD " + fonte);
                }
                codigoSimpSIM.Add("STORE " + dest);
            }
            // Operação Aritmética: x = y op z
            else if (partes.Length == 5 && partes[1] == "=" && (partes[3] == "+" || partes[3] == "-" || partes[3] == "*" || partes[3] == "/"))
            {
                string dest = partes[0];
                string op1 = partes[2];
                string operador = partes[3];
                string op2 = partes[4];

                // LOAD op1
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
                foreach (string linhaAnterior in codigoIntermediarioOtimizado)
                {
                    string linhaTrimmed = linhaAnterior.Trim();
                    if (linhaTrimmed.StartsWith("// REMOVIDO")) continue;
                    if (linhaTrimmed.StartsWith(varCondicional + " ="))
                    {
                        definicaoDeVarCondicional = linhaTrimmed;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(definicaoDeVarCondicional))
                {
                    var matchDef = Regex.Match(definicaoDeVarCondicional,
                                     @"^\s*\w+\s*=\s*([a-zA-Z_0-9][a-zA-Z_0-9.]*)\s*([<>=!]+)\s*([a-zA-Z_0-9][a-zA-Z_0-9.]*)\s*$");
                    if (matchDef.Success)
                    {
                        string opA_str = matchDef.Groups[1].Value;
                        string relOp = matchDef.Groups[2].Value;
                        string opB_str = matchDef.Groups[3].Value;
                        if (decimal.TryParse(opA_str, out _)) codigoSimpSIM.Add("LOADI " + opA_str);
                        else codigoSimpSIM.Add("LOAD " + opA_str);
                        if (decimal.TryParse(opB_str, out _)) codigoSimpSIM.Add("SUBI " + opB_str);
                        else codigoSimpSIM.Add("SUB " + opB_str);
                        switch (relOp)
                        {
                            case "<": 
                                codigoSimpSIM.Add("JPOS " + labelDestino + "  ; Salta se A > B (A-B > 0)");
                                codigoSimpSIM.Add("JZERO " + labelDestino + " ; Salta se A = B (A-B = 0)");
                                break;
                            case "<=": 
                                codigoSimpSIM.Add("JPOS " + labelDestino + "  ; Salta se A > B (A-B > 0)");
                                break;
                            case ">": 
                                codigoSimpSIM.Add("JNEG " + labelDestino + "  ; Salta se A < B (A-B < 0)");
                                codigoSimpSIM.Add("JZERO " + labelDestino + " ; Salta se A = B (A-B = 0)");
                                break;
                            case ">=":
                                codigoSimpSIM.Add("JNEG " + labelDestino + "  ; Salta se A < B (A-B < 0)");
                                break;
                            case "==":
                                string skipLabelTrue = NovoRotuloSimpSIM();
                                codigoSimpSIM.Add("JZERO " + skipLabelTrue + " ; Se A == B, não salta para destino, pula o JUMP");
                                codigoSimpSIM.Add("JUMP " + labelDestino + "   ; A != B, então salta");
                                codigoSimpSIM.Add(skipLabelTrue + ":");
                                break;
                            case "!=":
                                codigoSimpSIM.Add("JZERO " + labelDestino + " ; Salta se A == B (A-B = 0)");
                                break;
                            default:
                                codigoSimpSIM.Add("; ERRO SimpSIM: Operador relacional desconhecido em ifFalse: " + relOp);
                                break;
                        }
                    }
                    else
                    {
                        codigoSimpSIM.Add("; ERRO SimpSIM: Nao foi possivel parsear a definicao da condicao: " + definicaoDeVarCondicional);
                        codigoSimpSIM.Add("LOAD " + varCondicional + " ; Fallback condicional");
                        codigoSimpSIM.Add("JZERO " + labelDestino + " ; Salta se " + varCondicional + " (condicao) for 0 (false)");
                    }
                }
                else
                {
                    codigoSimpSIM.Add("; ERRO SimpSIM: Definicao da variavel condicional " + varCondicional + " nao encontrada.");
                    codigoSimpSIM.Add("LOAD " + varCondicional + " ; Fallback condicional");
                    codigoSimpSIM.Add("JZERO " + labelDestino + " ; Salta se " + varCondicional + " (condicao) for 0 (false)");
                }
            }
            else
            {
                codigoSimpSIM.Add("; Traducao SimpSIM nao implementada: " + linhaCI);
            }
        }
    }
}