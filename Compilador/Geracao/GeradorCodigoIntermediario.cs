// GeradorCodigoIntermediario.cs
using System;
using System.Collections.Generic;
// using Compilador.Analises; // Se NoAST ou TabelaDeSimbolos vierem daqui
// using Compilador.Sintese; // Se InstrucaoTresEnderecos estiver aqui

namespace Compilador.Geracao // Ou Compilador.Sintese
{
    // Supondo que exista uma classe NoAST representando os nós da Árvore Sintática Abstrata
    // Exemplo simplificado de NoAST (adapte à sua estrutura real)
    public class NoAST
    {
        public string TipoNo { get; set; } // Ex: "Atribuicao", "If", "While", "ExpressaoBinaria", "Numero", "Identificador"
        public string Valor { get; set; } // Ex: nome da var, valor do num, operador
        public NoAST Esquerda { get; set; }
        public NoAST Direita { get; set; }
        public List<NoAST> Filhos { get; set; } // Para nós com múltiplos filhos, como blocos de comandos

        public NoAST(string tipoNo, string valor = null)
        {
            TipoNo = tipoNo;
            Valor = valor;
            Filhos = new List<NoAST>();
        }
    }

    public class GeradorCodigoIntermediario
    {
        private List<InstrucaoTresEnderecos> _codigo;
        private int _contadorTemporarios;
        private int _contadorLabels;
        // private TabelaDeSimbolos _tabelaDeSimbolos; // Pode ser necessária

        public GeradorCodigoIntermediario(/* TabelaDeSimbolos tabelaDeSimbolos */)
        {
            _codigo = new List<InstrucaoTresEnderecos>();
            _contadorTemporarios = 0;
            _contadorLabels = 0;
            // _tabelaDeSimbolos = tabelaDeSimbolos;
        }

        private string NovoTemporario()
        {
            return $"t{_contadorTemporarios++}";
        }

        private string NovoLabel()
        {
            return $"L{_contadorLabels++}";
        }

        public List<InstrucaoTresEnderecos> Gerar(NoAST raizAST)
        {
            _codigo.Clear();
            _contadorTemporarios = 0;
            _contadorLabels = 0;
            VisitarNo(raizAST);
            return _codigo;
        }

        private string VisitarNo(NoAST no)
        {
            if (no == null) return null;

            switch (no.TipoNo)
            {
                case "Programa":
                    foreach (var filho in no.Filhos.FindAll(f => f.TipoNo == "DeclaracaoVariavel"))
                    {
                        VisitarNo(filho);
                    }
                    foreach (var filho in no.Filhos.FindAll(f => f.TipoNo != "DeclaracaoVariavel"))
                    {
                        VisitarNo(filho);
                    }
                    break;

                case "BlocoComandos":
                    foreach (var comando in no.Filhos)
                    {
                        VisitarNo(comando);
                    }
                    break;

                case "Atribuicao":
                    string varNome = no.Esquerda.Valor;
                    string tempExpr = VisitarNo(no.Direita);
                    _codigo.Add(InstrucaoTresEnderecos.CriarAtribuicaoOuUnaria(TipoOperacao.ASSIGN, varNome, tempExpr));
                    return varNome;

                case "ExpressaoBinaria":
                    string tempArg1 = VisitarNo(no.Esquerda);
                    string tempArg2 = VisitarNo(no.Direita);
                    string tempResultado = NovoTemporario();
                    TipoOperacao op = MapearOperador(no.Valor);
                    _codigo.Add(InstrucaoTresEnderecos.CriarOperacaoBinaria(op, tempResultado, tempArg1, tempArg2));
                    return tempResultado;

                case "NumeroLiteral":
                case "CharLiteral":
                case "StringLiteral":
                case "BooleanLiteral": // Assumindo que t_bool da gramática vira um literal booleano
                    return no.Valor;

                case "Identificador":
                    return no.Valor;

                case "If":
                    string tempCond = VisitarNo(no.Filhos[0]); // Condição (pode ser uma ExpressaoRelacional ou um t_bool)
                    string labelElse = NovoLabel();
                    string labelFimIf = NovoLabel();

                    _codigo.Add(InstrucaoTresEnderecos.CriarSaltoCondicional(TipoOperacao.IF_FALSE_GOTO, tempCond, labelElse));
                    VisitarNo(no.Filhos[1]); // Bloco Then

                    if (no.Filhos.Count > 2 && no.Filhos[2] != null) // Existe Else
                    {
                        _codigo.Add(InstrucaoTresEnderecos.CriarGoto(labelFimIf));
                        _codigo.Add(InstrucaoTresEnderecos.CriarLabel(labelElse));
                        VisitarNo(no.Filhos[2]); // Bloco Else
                        _codigo.Add(InstrucaoTresEnderecos.CriarLabel(labelFimIf));
                    }
                    else // Não existe Else
                    {
                        _codigo.Add(InstrucaoTresEnderecos.CriarLabel(labelElse)); // labelElse é o fim do if
                    }
                    break;

                case "While":
                    string labelInicioWhile = NovoLabel();
                    string labelCorpoWhile = NovoLabel(); // Pode ser o mesmo que labelInicioWhile se a condição for testada antes do label
                    string labelFimWhile = NovoLabel();

                    _codigo.Add(InstrucaoTresEnderecos.CriarLabel(labelInicioWhile));
                    string tempCondWhile = VisitarNo(no.Filhos[0]); // Avalia condição
                    _codigo.Add(InstrucaoTresEnderecos.CriarSaltoCondicional(TipoOperacao.IF_FALSE_GOTO, tempCondWhile, labelFimWhile));
                    // _codigo.Add(InstrucaoTresEnderecos.CriarLabel(labelCorpoWhile)); // Se o corpo precisar de um label distinto do início
                    VisitarNo(no.Filhos[1]); // Corpo do While
                    _codigo.Add(InstrucaoTresEnderecos.CriarGoto(labelInicioWhile));
                    _codigo.Add(InstrucaoTresEnderecos.CriarLabel(labelFimWhile));
                    break;

                case "ExpressaoRelacional": // t_expr operador_relacional t_expr | t_expr | t_bool
                    // Se for apenas t_expr ou t_bool, o próprio valor (ou temporário) já representa a condição.
                    // Se for t_expr op t_expr:
                    if (no.Filhos.Count == 2 && no.Valor != null) // Assumindo que no.Valor tem o operador e Filhos[0] e Filhos[1] são os operandos
                    {
                        string relArg1 = VisitarNo(no.Filhos[0]); // ou no.Esquerda
                        string relArg2 = VisitarNo(no.Filhos[1]); // ou no.Direita
                        string relTemp = NovoTemporario();
                        TipoOperacao relOp = MapearOperador(no.Valor); // Usar MapearOperador que já inclui os relacionais
                        _codigo.Add(InstrucaoTresEnderecos.CriarOperacaoBinaria(relOp, relTemp, relArg1, relArg2));
                        return relTemp; // Este temporário guardará true/false (ou 0/1)
                    }
                    else if (no.Filhos.Count == 1) // Apenas t_expr ou t_bool
                    {
                        return VisitarNo(no.Filhos[0]); // Retorna o resultado da expressão/booleano
                    }
                    // Se for só t_bool direto como no.TipoNo = "BooleanLiteral" e no.Valor = "true" / "false", já foi tratado.
                    // Se o nó "ExpressaoRelacional" puder ser diretamente um booleano (sem filhos), precisaria tratar aqui.
                    // Por exemplo, if (no.Valor == "true" || no.Valor == "false") return no.Valor;
                    break;

                // Adicionar outros casos conforme sua gramática e AST:
                // case "ComandoRead": // Exemplo, se tiver um nó específico para read
                //    string varRead = no.Filhos[0].Valor; // Supondo que o filho é o identificador
                //    _codigo.Add(InstrucaoTresEnderecos.CriarRead(varRead));
                //    break;
                // case "ComandoWrite": // Exemplo
                //    string valWrite = VisitarNo(no.Filhos[0]); // Supondo que o filho é a expressão a ser escrita
                //    _codigo.Add(InstrucaoTresEnderecos.CriarWrite(valWrite));
                //    break;

                default:
                    Console.WriteLine($"AVISO: Geração de código intermediário não implementada para o tipo de nó: {no.TipoNo}");
                    // Para nós que apenas agrupam ou não geram código diretamente, pode ser necessário visitar filhos:
                    // if (no.Filhos != null) foreach(var filho in no.Filhos) VisitarNo(filho);
                    break;
            }
            return null;
        }

        private TipoOperacao MapearOperador(string op)
        {
            switch (op)
            {
                case "+": return TipoOperacao.ADD;
                case "-": return TipoOperacao.SUB;
                case "*": return TipoOperacao.MULT;
                case "/": return TipoOperacao.DIV;
                case "%": return TipoOperacao.MOD; 

                case "==": return TipoOperacao.EQUAL;
                case "!=": return TipoOperacao.NOT_EQUAL;
                case "<": return TipoOperacao.LESS_THAN;
                case "<=": return TipoOperacao.LESS_EQUAL;
                case ">": return TipoOperacao.GREATER_THAN;
                case ">=": return TipoOperacao.GREATER_EQUAL;
                default: throw new ArgumentException($"Operador desconhecido: {op}");
            }
        }

        // MapearOperadorRelacional não é mais estritamente necessário se MapearOperador cobrir tudo,
        // mas pode ser mantido se a estrutura da sua AST para expressões relacionais for muito distinta.
        // Se MapearOperador já trata os operadores relacionais, pode remover este método.
        /*
        private TipoOperacao MapearOperadorRelacional(string op)
        {
            switch (op)
            {
                case "==": return TipoOperacao.EQUAL;
                case "!=": return TipoOperacao.NOT_EQUAL;
                case "<": return TipoOperacao.LESS_THAN;
                case "<=": return TipoOperacao.LESS_EQUAL;
                case ">": return TipoOperacao.GREATER_THAN;
                case ">=": return TipoOperacao.GREATER_EQUAL;
                default: throw new ArgumentException($"Operador relacional desconhecido: {op}");
            }
        }
        */
    }
}