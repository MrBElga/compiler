using System;
using System.Collections.Generic;
using System.Globalization;

namespace Compilador.Geracao // Ou Compilador.Sintese
{
    public class OtimizadorCodigo
    {
        private List<InstrucaoTresEnderecos> _codigoOtimizado;

        public OtimizadorCodigo()
        {
            _codigoOtimizado = new List<InstrucaoTresEnderecos>();
        }

        public List<InstrucaoTresEnderecos> Otimizar(List<InstrucaoTresEnderecos> codigoIntermediario)
        {
            _codigoOtimizado = new List<InstrucaoTresEnderecos>(codigoIntermediario); // Começa com uma cópia

            bool mudancaFeitaGlobal;
            int maxPassadas = 10; // Limite para evitar loops infinitos em otimizações complexas
            int passadaAtual = 0;

            do
            {
                mudancaFeitaGlobal = false;
                passadaAtual++;

                // É importante definir uma ordem ou estratégia para aplicar as otimizações.
                // Algumas otimizações podem habilitar outras.
                // Iterar até que nenhuma mudança ocorra (ou um limite de passadas seja atingido).

                mudancaFeitaGlobal |= AplicarDobramentoDeConstantes();
                mudancaFeitaGlobal |= AplicarSimplificacaoAlgebrica();
                mudancaFeitaGlobal |= AplicarReducaoDeForca();
                mudancaFeitaGlobal |= AplicarPropagacaoCopias(); // Pode gerar código morto
                mudancaFeitaGlobal |= AplicarEliminacaoCodigoMorto(); // Bom executar após propagações
                mudancaFeitaGlobal |= AplicarEliminacaoSaltosSobreSaltos();
                mudancaFeitaGlobal |= AplicarRemocaoLabelsNaoUtilizados(); // Geralmente uma das últimas

                // Adicionar outras otimizações escolhidas aqui.
                // Ex: mudancaFeitaGlobal |= AplicarEliminacaoSubexpressoesComuns(); (mais complexa)


                if (passadaAtual >= maxPassadas && mudancaFeitaGlobal)
                {
                    Console.WriteLine("Otimizador: Limite de passadas atingido, otimização pode não estar completa.");
                    break;
                }

            } while (mudancaFeitaGlobal);

            if (passadaAtual > 1) Console.WriteLine($"Otimizador: Concluído em {passadaAtual - 1} passada(s) de otimização.");
            else Console.WriteLine("Otimizador: Nenhuma otimização aplicada ou concluído em 1 passada.");

            return _codigoOtimizado;
        }

        // --- Implementações das Regras de Otimização ---

        // 1. Dobramento de Constantes (Constant Folding)
        private bool AplicarDobramentoDeConstantes()
        {
            bool mudou = false;
            for (int i = 0; i < _codigoOtimizado.Count; i++)
            {
                var inst = _codigoOtimizado[i];
                if (inst.Argumento1 != null && inst.Argumento2 != null && // Garante que os operandos existem
                    IsArithmetic(inst.Operacao) &&
                    IsConstant(inst.Argumento1) && IsConstant(inst.Argumento2))
                {
                    // Tentar converter para double para cálculos, depois para string.
                    // Considerar inteiros se sua linguagem só tiver inteiros ou para certas operações.
                    if (double.TryParse(inst.Argumento1, NumberStyles.Any, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(inst.Argumento2, NumberStyles.Any, CultureInfo.InvariantCulture, out double val2))
                    {
                        double? resultadoCalc = null;
                        switch (inst.Operacao)
                        {
                            case TipoOperacao.ADD: resultadoCalc = val1 + val2; break;
                            case TipoOperacao.SUB: resultadoCalc = val1 - val2; break;
                            case TipoOperacao.MULT: resultadoCalc = val1 * val2; break;
                            case TipoOperacao.DIV:
                                if (val2 != 0) resultadoCalc = val1 / val2; // Evitar divisão por zero
                                else continue; // Não otimizar, pode ser erro em tempo de execução
                                break;
                            case TipoOperacao.MOD:
                                if (val2 != 0) resultadoCalc = val1 % val2; // Cuidado com MOD para não inteiros
                                else continue;
                                break;
                        }

                        if (resultadoCalc.HasValue)
                        {
                            string resultadoStr = resultadoCalc.Value.ToString(CultureInfo.InvariantCulture);
                            // Se o resultado for inteiro, formatar sem casas decimais (opcional)
                            if (resultadoCalc.Value == Math.Truncate(resultadoCalc.Value))
                            {
                                resultadoStr = ((long)resultadoCalc.Value).ToString(CultureInfo.InvariantCulture);
                            }

                            var originalInstStr = inst.ToString();
                            _codigoOtimizado[i] = InstrucaoTresEnderecos.CriarAtribuicaoOuUnaria(TipoOperacao.ASSIGN, inst.Resultado, resultadoStr);
                            mudou = true;
                            Console.WriteLine($"Otimização (Dobramento Const): '{originalInstStr}' => '{_codigoOtimizado[i]}'");
                        }
                    }
                }
            }
            return mudou;
        }

        // 2. Propagação de Cópias
        private bool AplicarPropagacaoCopias()
        {
            bool mudouGeral = false;
            for (int i = 0; i < _codigoOtimizado.Count; i++)
            {
                var instCopia = _codigoOtimizado[i];
                if ((instCopia.Operacao == TipoOperacao.ASSIGN || instCopia.Operacao == TipoOperacao.COPY) &&
                    instCopia.Resultado != null && instCopia.Argumento1 != null &&
                    !IsConstant(instCopia.Argumento1)) // Propagar t1 = x, não t1 = 5
                {
                    string varDestino = instCopia.Resultado;
                    string varOrigem = instCopia.Argumento1;

                    if (varDestino == varOrigem) continue; // x = x não faz nada

                    for (int j = i + 1; j < _codigoOtimizado.Count; j++)
                    {
                        var instSubsequenteOriginal = _codigoOtimizado[j];
                        // Precisamos criar uma NOVA instrução se houver substituição,
                        // pois as instâncias de InstrucaoTresEnderecos são imutáveis após a criação pelos métodos fábrica.
                        string novoArg1 = instSubsequenteOriginal.Argumento1;
                        string novoArg2 = instSubsequenteOriginal.Argumento2;
                        bool substituidoAqui = false;

                        if (novoArg1 == varDestino)
                        {
                            novoArg1 = varOrigem;
                            substituidoAqui = true;
                        }
                        if (novoArg2 != null && novoArg2 == varDestino)
                        {
                            novoArg2 = varOrigem;
                            substituidoAqui = true;
                        }

                        if (substituidoAqui)
                        {
                            InstrucaoTresEnderecos novaInstSubsequente;
                            // Recriar a instrução subsequente com os argumentos modificados
                            // Isso requer um mapeamento de volta para os métodos fábrica ou um construtor flexível (que não temos mais publicamente)
                            // A maneira mais segura é ter métodos fábrica que podem clonar e modificar, ou ser muito cuidadoso aqui.
                            // Por simplicidade, vamos assumir que podemos recriar baseado na operação.
                            // Esta parte pode ficar complexa devido à imutabilidade.
                            // Uma forma mais simples é se as propriedades de InstrucaoTresEnderecos fossem settable (o que eram antes da refatoração).
                            // Se elas SÃO settable (private set), então a modificação direta como antes funcionaria.
                            // Assumindo que são private set e podemos modificar internamente na classe ou por métodos específicos (que não temos).
                            // SOLUÇÃO: Se as propriedades têm 'private set', podemos expor métodos internos de modificação
                            // ou, para esta demonstração, vamos assumir que a classe InstrucaoTresEnderecos permite a modificação controlada
                            // ou que recriamos a instrução completamente.

                            // Se InstrucaoTresEnderecos fosse mutável:
                            // instSubsequenteOriginal.Argumento1 = novoArg1;
                            // instSubsequenteOriginal.Argumento2 = novoArg2;

                            // Como é imutável (propriedades com private set apenas no construtor via fábrica):
                            // Precisamos recriar a instrução.
                            var opSub = instSubsequenteOriginal.Operacao;
                            var resSub = instSubsequenteOriginal.Resultado;
                            var labelDestSub = instSubsequenteOriginal.LabelDestino;

                            // Tentativa de recriar (pode precisar de mais casos):
                            if (IsArithmetic(opSub) || IsRelational(opSub))
                            {
                                novaInstSubsequente = InstrucaoTresEnderecos.CriarOperacaoBinaria(opSub, resSub, novoArg1, novoArg2);
                            }
                            else if (opSub == TipoOperacao.ASSIGN || opSub == TipoOperacao.COPY || opSub == TipoOperacao.UNARY_MINUS)
                            {
                                novaInstSubsequente = InstrucaoTresEnderecos.CriarAtribuicaoOuUnaria(opSub, resSub, novoArg1);
                            }
                            else if (opSub == TipoOperacao.WRITE || opSub == TipoOperacao.PARAM || opSub == TipoOperacao.RETURN)
                            {
                                // Esses usam apenas Arg1
                                if (opSub == TipoOperacao.WRITE) novaInstSubsequente = InstrucaoTresEnderecos.CriarWrite(novoArg1);
                                else if (opSub == TipoOperacao.PARAM) novaInstSubsequente = InstrucaoTresEnderecos.CriarParam(novoArg1);
                                else novaInstSubsequente = InstrucaoTresEnderecos.CriarReturn(novoArg1);
                            }
                            else if (opSub == TipoOperacao.IF_FALSE_GOTO || opSub == TipoOperacao.IF_TRUE_GOTO)
                            {
                                novaInstSubsequente = InstrucaoTresEnderecos.CriarSaltoCondicional(opSub, novoArg1, labelDestSub);
                            }
                            // Adicionar mais casos conforme necessário ou refatorar InstrucaoTresEnderecos para ter um método `ComArgumentosModificados`
                            else
                            {
                                Console.WriteLine($"AVISO (Propagação): Não foi possível recriar instrução otimizada: {instSubsequenteOriginal}");
                                novaInstSubsequente = instSubsequenteOriginal; // Mantém a original se não souber recriar
                                substituidoAqui = false; // Não considera como substituído
                            }

                            if (substituidoAqui)
                            {
                                _codigoOtimizado[j] = novaInstSubsequente;
                                Console.WriteLine($"Otimização (Propagação Cópia): '{varDestino}' por '{varOrigem}' em '{instSubsequenteOriginal}' => '{novaInstSubsequente}'");
                                mudouGeral = true;
                            }
                        }

                        if (instSubsequenteOriginal.Resultado == varDestino || instSubsequenteOriginal.Resultado == varOrigem)
                        {
                            break;
                        }
                    }
                }
            }
            return mudouGeral;
        }

        // 3. Eliminação de Código Morto
        private bool AplicarEliminacaoCodigoMorto()
        {
            bool mudou = false;
            HashSet<string> varsVivas = new HashSet<string>();
            List<InstrucaoTresEnderecos> novoCodigoIntermediario = new List<InstrucaoTresEnderecos>();
            HashSet<string> labelsAlvo = new HashSet<string>();

            foreach (var inst in _codigoOtimizado)
            {
                if ((inst.Operacao == TipoOperacao.GOTO || inst.Operacao == TipoOperacao.IF_FALSE_GOTO || inst.Operacao == TipoOperacao.IF_TRUE_GOTO)
                    && inst.LabelDestino != null)
                {
                    labelsAlvo.Add(inst.LabelDestino);
                }
                // Se houver chamadas de função, todos os parâmetros e a função em si podem ser considerados "vivos" no ponto da chamada.
                // E variáveis globais ou modificadas por referência também. Para simplificar, não estamos tratando isso profundamente aqui.
            }

            for (int i = _codigoOtimizado.Count - 1; i >= 0; i--)
            {
                var inst = _codigoOtimizado[i];
                bool instrucaoConsideradaViva = false;

                // Efeitos colaterais ou controle de fluxo explícito mantêm a instrução viva
                if (inst.Operacao == TipoOperacao.WRITE || inst.Operacao == TipoOperacao.READ ||
                    inst.Operacao == TipoOperacao.CALL || inst.Operacao == TipoOperacao.RETURN ||
                    inst.Operacao == TipoOperacao.GOTO ||
                    inst.Operacao == TipoOperacao.IF_FALSE_GOTO || inst.Operacao == TipoOperacao.IF_TRUE_GOTO)
                {
                    instrucaoConsideradaViva = true;
                }

                // Um LABEL é vivo se for alvo de algum salto
                if (inst.Operacao == TipoOperacao.LABEL && inst.Resultado != null && labelsAlvo.Contains(inst.Resultado))
                {
                    instrucaoConsideradaViva = true;
                }

                // Se o resultado da instrução é uma variável viva
                if (inst.Resultado != null && varsVivas.Contains(inst.Resultado))
                {
                    instrucaoConsideradaViva = true;
                }

                if (instrucaoConsideradaViva)
                {
                    novoCodigoIntermediario.Add(inst);
                    if (inst.Resultado != null)
                    {
                        varsVivas.Remove(inst.Resultado); // Esta var foi definida, não precisa mais ser "procurada" para cima
                    }
                    if (inst.Argumento1 != null && !IsConstant(inst.Argumento1)) varsVivas.Add(inst.Argumento1);
                    if (inst.Argumento2 != null && !IsConstant(inst.Argumento2)) varsVivas.Add(inst.Argumento2);
                }
                else
                {
                    // Se a instrução não é viva e não é um label essencial (já tratado acima)
                    if (!(inst.Operacao == TipoOperacao.LABEL && inst.Resultado != null && labelsAlvo.Contains(inst.Resultado)))
                    {
                        Console.WriteLine($"Otimização (Código Morto): Removendo '{inst}'");
                        mudou = true;
                    }
                    else
                    {
                        novoCodigoIntermediario.Add(inst); // Mantém label que é alvo mas não foi pego por outras condições
                    }
                }
            }

            if (mudou)
            {
                novoCodigoIntermediario.Reverse();
                _codigoOtimizado = novoCodigoIntermediario;
            }
            return mudou;
        }

        // 4. Redução de Força
        private bool AplicarReducaoDeForca()
        {
            bool mudou = false;
            for (int i = 0; i < _codigoOtimizado.Count; i++)
            {
                var inst = _codigoOtimizado[i];
                InstrucaoTresEnderecos novaInst = null;

                if (inst.Operacao == TipoOperacao.MULT)
                {
                    if (inst.Argumento2 == "2") // t = arg1 * 2
                    {
                        novaInst = InstrucaoTresEnderecos.CriarOperacaoBinaria(TipoOperacao.ADD, inst.Resultado, inst.Argumento1, inst.Argumento1);
                    }
                    else if (inst.Argumento1 == "2") // t = 2 * arg2
                    {
                        novaInst = InstrucaoTresEnderecos.CriarOperacaoBinaria(TipoOperacao.ADD, inst.Resultado, inst.Argumento2, inst.Argumento2);
                    }
                    // Adicionar t = arg * 0 => t = 0 (já coberto por simplificação algébrica se feita antes)
                    // Adicionar t = arg * 1 => t = arg (já coberto por simplificação algébrica se feita antes)
                }
                // Adicionar para DIV por potências de 2 (x / 2 => x >> 1) se aplicável e se SimpSIM suportar shifts

                if (novaInst != null)
                {
                    Console.WriteLine($"Otimização (Redução de Força): '{inst}' => '{novaInst}'");
                    _codigoOtimizado[i] = novaInst;
                    mudou = true;
                }
            }
            return mudou;
        }

        // 5. Simplificação Algébrica
        private bool AplicarSimplificacaoAlgebrica()
        {
            bool mudou = false;
            for (int i = 0; i < _codigoOtimizado.Count; i++)
            {
                var inst = _codigoOtimizado[i];
                string novoArgParaAssign = null; // Se a instrução for simplificada para uma atribuição simples (x = y ou x = const)
                InstrucaoTresEnderecos instrucaoSubstituta = null;


                if (inst.Resultado == null) continue; // Precisa de um resultado para simplificar

                // x = y + 0  => x = y
                if (inst.Operacao == TipoOperacao.ADD)
                {
                    if (inst.Argumento1 == "0") novoArgParaAssign = inst.Argumento2;
                    else if (inst.Argumento2 == "0") novoArgParaAssign = inst.Argumento1;
                }
                // x = y - 0 => x = y
                // x = y - y => x = 0
                else if (inst.Operacao == TipoOperacao.SUB)
                {
                    if (inst.Argumento2 == "0") novoArgParaAssign = inst.Argumento1;
                    else if (inst.Argumento1 == inst.Argumento2 && !IsConstant(inst.Argumento1)) // Evitar transformar "0-0" em "x=0" se x for diferente.
                        novoArgParaAssign = "0"; // x = y - y se torna x = 0
                }
                // x = y * 1 => x = y
                // x = y * 0 => x = 0
                else if (inst.Operacao == TipoOperacao.MULT)
                {
                    if (inst.Argumento1 == "1") novoArgParaAssign = inst.Argumento2;
                    else if (inst.Argumento2 == "1") novoArgParaAssign = inst.Argumento1;
                    else if (inst.Argumento1 == "0" || inst.Argumento2 == "0") novoArgParaAssign = "0";
                }
                // x = y / 1 => x = y
                // x = 0 / y => x = 0 (se y != 0, já tratado no dobramento de constantes se y for const)
                else if (inst.Operacao == TipoOperacao.DIV)
                {
                    if (inst.Argumento2 == "1") novoArgParaAssign = inst.Argumento1;
                    else if (inst.Argumento1 == "0" && inst.Argumento2 != "0") novoArgParaAssign = "0";
                }

                if (novoArgParaAssign != null)
                {
                    instrucaoSubstituta = InstrucaoTresEnderecos.CriarAtribuicaoOuUnaria(TipoOperacao.ASSIGN, inst.Resultado, novoArgParaAssign);
                }


                if (instrucaoSubstituta != null)
                {
                    var originalInstStr = inst.ToString();
                    _codigoOtimizado[i] = instrucaoSubstituta;
                    mudou = true;
                    Console.WriteLine($"Otimização (Simpl. Algébrica): '{originalInstStr}' => '{_codigoOtimizado[i]}'");
                }
            }
            return mudou;
        }

        // 6. Eliminação de Saltos sobre Saltos
        private bool AplicarEliminacaoSaltosSobreSaltos()
        {
            bool mudou = false;
            Dictionary<string, string> mapaRedirecionamentoLabels = new Dictionary<string, string>();

            // Fase 1: Encontrar todos os GOTO L2 que seguem imediatamente um LABEL L1:
            for (int i = 0; i < _codigoOtimizado.Count - 1; i++)
            {
                var instAtual = _codigoOtimizado[i];
                if (instAtual.Operacao == TipoOperacao.LABEL && instAtual.Resultado != null)
                {
                    // Procurar a próxima instrução NÃO-LABEL
                    int idxProximaNaoLabel = i + 1;
                    while (idxProximaNaoLabel < _codigoOtimizado.Count &&
                          _codigoOtimizado[idxProximaNaoLabel].Operacao == TipoOperacao.LABEL)
                    {
                        idxProximaNaoLabel++;
                    }

                    if (idxProximaNaoLabel < _codigoOtimizado.Count)
                    {
                        var proximaInstEfetiva = _codigoOtimizado[idxProximaNaoLabel];
                        if (proximaInstEfetiva.Operacao == TipoOperacao.GOTO && proximaInstEfetiva.LabelDestino != null)
                        {
                            // L1: GOTO L2 (L1 é instAtual.Resultado, L2 é proximaInstEfetiva.LabelDestino)
                            // Todos os saltos para L1 devem ir para L2, a menos que L1 == L2 (loop infinito)
                            if (instAtual.Resultado != proximaInstEfetiva.LabelDestino)
                            {
                                mapaRedirecionamentoLabels[instAtual.Resultado] = proximaInstEfetiva.LabelDestino;
                            }
                        }
                    }
                }
            }

            // Resolver redirecionamentos encadeados (L1->L2, L2->L3 => L1->L3)
            bool alteracaoNoMapa;
            do
            {
                alteracaoNoMapa = false;
                var chaves = new List<string>(mapaRedirecionamentoLabels.Keys);
                foreach (var origem in chaves)
                {
                    string destinoAtual = mapaRedirecionamentoLabels[origem];
                    if (mapaRedirecionamentoLabels.TryGetValue(destinoAtual, out string novoDestino))
                    {
                        if (mapaRedirecionamentoLabels[origem] != novoDestino) // Evitar loop L1->L2, L2->L1 se L1 já aponta para L2
                        {
                            mapaRedirecionamentoLabels[origem] = novoDestino;
                            alteracaoNoMapa = true;
                        }
                    }
                }
            } while (alteracaoNoMapa);


            // Fase 2: Aplicar redirecionamentos
            for (int i = 0; i < _codigoOtimizado.Count; i++)
            {
                var inst = _codigoOtimizado[i];
                string novoLabelDestino = null;

                if (inst.LabelDestino != null && mapaRedirecionamentoLabels.TryGetValue(inst.LabelDestino, out string destinoRedirecionado))
                {
                    if (inst.LabelDestino != destinoRedirecionado) // Só muda se for diferente
                    {
                        novoLabelDestino = destinoRedirecionado;
                    }
                }

                if (novoLabelDestino != null)
                {
                    var originalInstStr = inst.ToString();
                    InstrucaoTresEnderecos novaInstModificada;
                    if (inst.Operacao == TipoOperacao.GOTO)
                    {
                        novaInstModificada = InstrucaoTresEnderecos.CriarGoto(novoLabelDestino);
                    }
                    else if (inst.Operacao == TipoOperacao.IF_FALSE_GOTO || inst.Operacao == TipoOperacao.IF_TRUE_GOTO)
                    {
                        novaInstModificada = InstrucaoTresEnderecos.CriarSaltoCondicional(inst.Operacao, inst.Argumento1, novoLabelDestino);
                    }
                    else
                    {
                        // Não deveria acontecer se LabelDestino só é usado por saltos
                        continue;
                    }
                    _codigoOtimizado[i] = novaInstModificada;
                    mudou = true;
                    Console.WriteLine($"Otimização (Salto sobre Salto): '{originalInstStr}' => '{_codigoOtimizado[i]}'");
                }
            }
            // Após esta otimização, alguns labels L1 (que eram `L1: GOTO L2`) podem se tornar não utilizados.
            // E a instrução `GOTO L2` após `L1:` pode se tornar código morto se L1 não for mais referenciado.
            return mudou;
        }


        // 7. Remoção de Labels Não Utilizados
        private bool AplicarRemocaoLabelsNaoUtilizados()
        {
            bool mudou = false;
            HashSet<string> labelsUtilizados = new HashSet<string>();
            foreach (var inst in _codigoOtimizado)
            {
                if ((inst.Operacao == TipoOperacao.GOTO || inst.Operacao == TipoOperacao.IF_FALSE_GOTO || inst.Operacao == TipoOperacao.IF_TRUE_GOTO)
                    && !string.IsNullOrEmpty(inst.LabelDestino))
                {
                    labelsUtilizados.Add(inst.LabelDestino);
                }
            }

            List<InstrucaoTresEnderecos> novoCodigo = new List<InstrucaoTresEnderecos>();
            foreach (var inst in _codigoOtimizado)
            {
                if (inst.Operacao == TipoOperacao.LABEL)
                {
                    // Labels são definidos com nome no campo Resultado pela fábrica CriarLabel
                    if (inst.Resultado != null && labelsUtilizados.Contains(inst.Resultado))
                    {
                        novoCodigo.Add(inst); // Mantém label utilizado
                    }
                    else
                    {
                        Console.WriteLine($"Otimização (Remoção Label): Label não utilizado '{inst.Resultado}:' removido.");
                        mudou = true; // Label removido
                    }
                }
                else
                {
                    novoCodigo.Add(inst); // Mantém outras instruções
                }
            }

            if (mudou)
            {
                _codigoOtimizado = novoCodigo;
            }
            return mudou;
        }


        // Funções auxiliares
        private bool IsArithmetic(TipoOperacao op)
        {
            return op == TipoOperacao.ADD || op == TipoOperacao.SUB || op == TipoOperacao.MULT || op == TipoOperacao.DIV || op == TipoOperacao.MOD;
        }
        private bool IsRelational(TipoOperacao op)
        {
            return op == TipoOperacao.EQUAL || op == TipoOperacao.NOT_EQUAL ||
                   op == TipoOperacao.LESS_THAN || op == TipoOperacao.LESS_EQUAL ||
                   op == TipoOperacao.GREATER_THAN || op == TipoOperacao.GREATER_EQUAL;
        }


        private bool IsConstant(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return false;
            // Verifica se é um número (inteiro ou real simples)
            if (double.TryParse(arg, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                return true;
            }
            // Verifica se é um literal booleano (true/false)
            if (arg.ToLower() == "true" || arg.ToLower() == "false")
            {
                return true;
            }
            // Adicionar lógica para char ('a') ou string ("abc") se sua linguagem os tratar como constantes diretas aqui
            // Por ora, consideramos apenas numéricos e booleanos como constantes diretas para cálculo/otimização.
            return false;
        }
    }
}