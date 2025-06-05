using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilador.Geracao
{
    public class GeradorCodigoSimpSIM
    {
        private List<string> _codigoAssembly;
        private Dictionary<string, int> _mapaVariaveisMemoria;
        private int _proximoEnderecoMemoriaDisponivel;


        public GeradorCodigoSimpSIM()
        {
            _codigoAssembly = new List<string>();
            _mapaVariaveisMemoria = new Dictionary<string, int>();
            _proximoEnderecoMemoriaDisponivel = 0;
        }

        private int ObterOuAlocarEndereco(string varNome)
        {
            if (char.IsDigit(varNome[0]) || varNome[0] == '-')
            {
                throw new InvalidOperationException($"Tentando alocar endereço para constante: {varNome}");
            }

            if (!_mapaVariaveisMemoria.ContainsKey(varNome))
            {
                _mapaVariaveisMemoria[varNome] = _proximoEnderecoMemoriaDisponivel;
                _codigoAssembly.Insert(0, $"; VAR {varNome} AT {_proximoEnderecoMemoriaDisponivel}");
                _proximoEnderecoMemoriaDisponivel++;
            }
            return _mapaVariaveisMemoria[varNome];
        }


        public List<string> GerarCodigo(List<InstrucaoTresEnderecos> codigoIntermediarioOtimizado)
        {
            _codigoAssembly.Clear();
            _mapaVariaveisMemoria.Clear();
            _proximoEnderecoMemoriaDisponivel = 100;

            _codigoAssembly.Add("; --- Início do Código Gerado para SimpSIM ---");
            _codigoAssembly.Add("JMP INICIO_PROGRAMA");
            _codigoAssembly.Add("; --- Área de Declaração de Variáveis (preenchida dinamicamente) ---");
            int linhaDeclaracaoVariaveis = _codigoAssembly.Count;


            _codigoAssembly.Add("INICIO_PROGRAMA:");


            foreach (var instrucao in codigoIntermediarioOtimizado)
            {
                _codigoAssembly.Add($"; {instrucao}");

                switch (instrucao.Operacao)
                {
                    case TipoOperacao.ASSIGN:
                        GerarAtribuicao(instrucao.Resultado, instrucao.Argumento1);
                        break;

                    case TipoOperacao.ADD:
                    case TipoOperacao.SUB:
                    case TipoOperacao.MULT:
                    case TipoOperacao.DIV:
                    case TipoOperacao.MOD:
                        GerarAritmetica(instrucao.Operacao, instrucao.Resultado, instrucao.Argumento1, instrucao.Argumento2);
                        break;

                    case TipoOperacao.LABEL:
                        _codigoAssembly.Add($"{instrucao.Resultado}:");
                        break;

                    case TipoOperacao.GOTO:
                        _codigoAssembly.Add($"JMP {instrucao.LabelDestino}");
                        break;

                    case TipoOperacao.IF_FALSE_GOTO:
                        CarregarValorEmAX(instrucao.Argumento1);
                        _codigoAssembly.Add($"JZ {instrucao.LabelDestino}");
                        break;


                    case TipoOperacao.EQUAL:
                    case TipoOperacao.NOT_EQUAL:
                    case TipoOperacao.LESS_THAN:
                    case TipoOperacao.GREATER_THAN:
                    case TipoOperacao.LESS_EQUAL:
                    case TipoOperacao.GREATER_EQUAL:
                        GerarRelacional(instrucao.Operacao, instrucao.Resultado, instrucao.Argumento1, instrucao.Argumento2);
                        break;

                    case TipoOperacao.WRITE:
                        CarregarValorEmAX(instrucao.Argumento1);
                        _codigoAssembly.Add("OUT");
                        break;

                    case TipoOperacao.READ:
                        int endRead = ObterOuAlocarEndereco(instrucao.Resultado);
                        _codigoAssembly.Add("IN");
                        _codigoAssembly.Add($"STORE {endRead}");
                        break;


                    default:
                        _codigoAssembly.Add($"; AVISO: Geração SimpSIM não implementada para {instrucao.Operacao}");
                        break;
                }
            }

            _codigoAssembly.Add("HALT");
            _codigoAssembly.Add("; --- Fim do Código Gerado ---");


            List<string> declaracoesParaSimpSim = new List<string>();
            declaracoesParaSimpSim.Add("; --- Área de Dados (Variáveis e Temporários) ---");
            foreach (var kvp in _mapaVariaveisMemoria.OrderBy(x => x.Value))
            {
                declaracoesParaSimpSim.Add($"; {kvp.Key} @ {kvp.Value}");
            }
            _codigoAssembly.InsertRange(1, declaracoesParaSimpSim);


            return _codigoAssembly;
        }

        private void CarregarValorEmAX(string valorOuVariavel)
        {
            if (int.TryParse(valorOuVariavel, out int constante))
            {
                _codigoAssembly.Add($"LOADI {constante}");
            }
            else
            {
                int endVar = ObterOuAlocarEndereco(valorOuVariavel);
                _codigoAssembly.Add($"LOAD {endVar}");
            }
        }
        private void CarregarValorEmBX(string valorOuVariavel)
        {


            if (int.TryParse(valorOuVariavel, out int constante))
            {
                int tempConstAddr = ObterOuAlocarEndereco($"__const_{constante}");
                _codigoAssembly.Add($"; Preparando constante {constante} para operação");
                _codigoAssembly.Add($"LOADI {constante}");
                _codigoAssembly.Add($"STORE {tempConstAddr}");
            }
        }


        private void GerarAtribuicao(string resultado, string arg1)
        {
            int endResultado = ObterOuAlocarEndereco(resultado);
            CarregarValorEmAX(arg1);
            _codigoAssembly.Add($"STORE {endResultado}");
        }

        private void GerarAritmetica(TipoOperacao op, string resultado, string arg1, string arg2)
        {

            int endResultado = ObterOuAlocarEndereco(resultado);
            int endArg2;

            CarregarValorEmAX(arg1);

            if (int.TryParse(arg2, out int constanteArg2))
            {
                string instrucaoImediata = "";
                switch (op)
                {
                    case TipoOperacao.ADD: instrucaoImediata = "ADDI"; break;
                    case TipoOperacao.SUB: instrucaoImediata = "SUBI"; break;
                }

                if (!string.IsNullOrEmpty(instrucaoImediata) && (op == TipoOperacao.ADD || op == TipoOperacao.SUB))
                {
                    _codigoAssembly.Add($"{instrucaoImediata} {constanteArg2}");
                }
                else
                {
                    int tempAddrArg1 = ObterOuAlocarEndereco("__temp_arg1_arith");
                    _codigoAssembly.Add($"STORE {tempAddrArg1}");
                    _codigoAssembly.Add($"LOADI {constanteArg2}");
                    int tempAddrArg2 = ObterOuAlocarEndereco("__temp_arg2_arith");
                    _codigoAssembly.Add($"STORE {tempAddrArg2}");

                    _codigoAssembly.Add($"LOAD {tempAddrArg1}");

                    string instrucaoMemoria = "";
                    switch (op)
                    {
                        case TipoOperacao.ADD: instrucaoMemoria = "ADD"; break;
                        case TipoOperacao.SUB: instrucaoMemoria = "SUB"; break;
                        case TipoOperacao.MULT:
                            _codigoAssembly.Add($"; Simulação de MULT: {resultado} = {arg1} * {arg2}");
                            SimularMultiplicacao(endResultado, tempAddrArg1, tempAddrArg2);
                            return;
                        case TipoOperacao.DIV:
                        case TipoOperacao.MOD:
                            _codigoAssembly.Add($"; Simulação de DIV/MOD: {resultado} = {arg1} / ou % {arg2}");
                            SimularDivisaoModulo(op, endResultado, tempAddrArg1, tempAddrArg2);
                            return;
                    }
                    if (!string.IsNullOrEmpty(instrucaoMemoria))
                        _codigoAssembly.Add($"{instrucaoMemoria} {tempAddrArg2}");
                }
            }
            else
            {
                endArg2 = ObterOuAlocarEndereco(arg2);
                string instrucao = "";
                switch (op)
                {
                    case TipoOperacao.ADD: instrucao = "ADD"; break;
                    case TipoOperacao.SUB: instrucao = "SUB"; break;
                    case TipoOperacao.MULT:
                        _codigoAssembly.Add($"; Simulação de MULT: {resultado} = {arg1} * {arg2}");
                        int endArg1 = ObterOuAlocarEndereco(arg1);
                        SimularMultiplicacao(endResultado, endArg1, endArg2);
                        return;
                    case TipoOperacao.DIV:
                    case TipoOperacao.MOD:
                        _codigoAssembly.Add($"; Simulação de DIV/MOD: {resultado} = {arg1} / ou % {arg2}");
                        int endArg1_div = ObterOuAlocarEndereco(arg1);
                        SimularDivisaoModulo(op, endResultado, endArg1_div, endArg2);
                        return;
                }
                if (!string.IsNullOrEmpty(instrucao))
                    _codigoAssembly.Add($"{instrucao} {endArg2}");
            }

            _codigoAssembly.Add($"STORE {endResultado}");
        }

        private void SimularMultiplicacao(int endResultado, int endArg1, int endArg2)
        {

            string lacoMult = $"L_MULT_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string fimMult = $"L_FIM_MULT_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string arg2Negativo = $"L_ARG2_NEG_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string fimSinal = $"L_FIM_SINAL_{Guid.NewGuid().ToString("N").Substring(0, 6)}";


            int tempSinal = ObterOuAlocarEndereco("__temp_sinal_mult");
            int tempArg1Abs = ObterOuAlocarEndereco("__temp_arg1_abs_mult");
            int tempArg2Abs = ObterOuAlocarEndereco("__temp_arg2_abs_mult");
            int contador = ObterOuAlocarEndereco("__temp_contador_mult");


            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"STORE {tempSinal}");

            _codigoAssembly.Add($"LOAD {endArg1}");
            _codigoAssembly.Add($"JGE {arg2Negativo}_arg1_ok");
            _codigoAssembly.Add($"LOAD {tempSinal}");
            _codigoAssembly.Add($"MULI -1");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {endArg1}");
            _codigoAssembly.Add($"STORE {tempArg1Abs}");
            _codigoAssembly.Add($"JMP {arg2Negativo}_arg1_fim");
            _codigoAssembly.Add($"{arg2Negativo}_arg1_ok:");
            _codigoAssembly.Add($"LOAD {endArg1}");
            _codigoAssembly.Add($"STORE {tempArg1Abs}");
            _codigoAssembly.Add($"{arg2Negativo}_arg1_fim:");

            _codigoAssembly.Add($"LOAD {endArg2}");
            _codigoAssembly.Add($"JGE {arg2Negativo}_arg2_ok");
            _codigoAssembly.Add($"LOAD {tempSinal}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {tempSinal}");
            _codigoAssembly.Add($"STORE {tempSinal}");

            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {endArg2}");
            _codigoAssembly.Add($"STORE {tempArg2Abs}");
            _codigoAssembly.Add($"JMP {arg2Negativo}_arg2_fim");
            _codigoAssembly.Add($"{arg2Negativo}_arg2_ok:");
            _codigoAssembly.Add($"LOAD {endArg2}");
            _codigoAssembly.Add($"STORE {tempArg2Abs}");
            _codigoAssembly.Add($"{arg2Negativo}_arg2_fim:");


            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"STORE {endResultado}");
            _codigoAssembly.Add($"LOAD {tempArg2Abs}");
            _codigoAssembly.Add($"STORE {contador}");

            _codigoAssembly.Add($"{lacoMult}:");
            _codigoAssembly.Add($"LOAD {contador}");
            _codigoAssembly.Add($"JZ {fimSinal}");

            _codigoAssembly.Add($"LOAD {endResultado}");
            _codigoAssembly.Add($"ADD {tempArg1Abs}");
            _codigoAssembly.Add($"STORE {endResultado}");

            _codigoAssembly.Add($"LOAD {contador}");
            _codigoAssembly.Add($"SUBI 1");
            _codigoAssembly.Add($"STORE {contador}");
            _codigoAssembly.Add($"JMP {lacoMult}");

            _codigoAssembly.Add($"{fimSinal}:");
            _codigoAssembly.Add($"LOAD {tempSinal}");
            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"SUB {tempSinal}");
            _codigoAssembly.Add($"LOAD {tempSinal}");
            _codigoAssembly.Add($"JGE {fimMult}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {endResultado}");
            _codigoAssembly.Add($"STORE {endResultado}");

            _codigoAssembly.Add($"{fimMult}:");
        }

        private void SimularDivisaoModulo(TipoOperacao op, int endResultado, int endArg1, int endArg2)
        {

            string lacoDiv = $"L_DIV_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string fimDiv = $"L_FIM_DIV_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string arg1Neg = $"L_DIV_ARG1NEG_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string arg1Pos = $"L_DIV_ARG1POS_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string arg2Neg = $"L_DIV_ARG2NEG_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string arg2Pos = $"L_DIV_ARG2POS_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string fimSinaisDiv = $"L_FIM_SINAIS_DIV_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string aplicarSinalResultado = $"L_APLICAR_SINAL_RES_{Guid.NewGuid().ToString("N").Substring(0, 6)}";


            int quociente = ObterOuAlocarEndereco("__temp_quociente_div");
            int dividendoAbs = ObterOuAlocarEndereco("__temp_dividendo_abs_div");
            int divisorAbs = ObterOuAlocarEndereco("__temp_divisor_abs_div");
            int sinalResultado = ObterOuAlocarEndereco("__temp_sinal_res_div");
            int sinalResto = ObterOuAlocarEndereco("__temp_sinal_resto_div");


            _codigoAssembly.Add($"LOAD {endArg2}");
            _codigoAssembly.Add($"JNZ L_DIV_NAO_ZERO_{Guid.NewGuid().ToString("N").Substring(0, 4)}");
            _codigoAssembly.Add($"; ERRO: Divisão por zero detectada em tempo de execução");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"STORE {endResultado}");
            _codigoAssembly.Add("HALT");
            _codigoAssembly.Add($"L_DIV_NAO_ZERO_{Guid.NewGuid().ToString("N").Substring(0, 4)}:");


            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"STORE {sinalResultado}");

            _codigoAssembly.Add($"LOAD {endArg1}");
            _codigoAssembly.Add($"JGE {arg1Pos}");
            _codigoAssembly.Add($"LOADI -1");
            _codigoAssembly.Add($"STORE {sinalResto}");
            _codigoAssembly.Add($"LOAD {sinalResultado}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {sinalResultado}");
            _codigoAssembly.Add($"STORE {sinalResultado}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {endArg1}");
            _codigoAssembly.Add($"STORE {dividendoAbs}");
            _codigoAssembly.Add($"JMP {arg2Neg}");
            _codigoAssembly.Add($"{arg1Pos}:");
            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"STORE {sinalResto}");
            _codigoAssembly.Add($"LOAD {endArg1}");
            _codigoAssembly.Add($"STORE {dividendoAbs}");

            _codigoAssembly.Add($"{arg2Neg}:");
            _codigoAssembly.Add($"LOAD {endArg2}");
            _codigoAssembly.Add($"JGE {arg2Pos}");
            _codigoAssembly.Add($"LOAD {sinalResultado}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {sinalResultado}");
            _codigoAssembly.Add($"STORE {sinalResultado}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {endArg2}");
            _codigoAssembly.Add($"STORE {divisorAbs}");
            _codigoAssembly.Add($"JMP {fimSinaisDiv}");
            _codigoAssembly.Add($"{arg2Pos}:");
            _codigoAssembly.Add($"LOAD {endArg2}");
            _codigoAssembly.Add($"STORE {divisorAbs}");

            _codigoAssembly.Add($"{fimSinaisDiv}:");

            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"STORE {quociente}");

            _codigoAssembly.Add($"{lacoDiv}:");
            _codigoAssembly.Add($"LOAD {dividendoAbs}");
            _codigoAssembly.Add($"SUB {divisorAbs}");
            _codigoAssembly.Add($"JLT {aplicarSinalResultado}");

            _codigoAssembly.Add($"STORE {dividendoAbs}");
            _codigoAssembly.Add($"LOAD {quociente}");
            _codigoAssembly.Add($"ADDI 1");
            _codigoAssembly.Add($"STORE {quociente}");
            _codigoAssembly.Add($"JMP {lacoDiv}");

            _codigoAssembly.Add($"{aplicarSinalResultado}:");

            _codigoAssembly.Add($"LOAD {sinalResultado}");
            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"SUB {sinalResultado}");
            _codigoAssembly.Add($"JZ L_QUOC_POS_{Guid.NewGuid().ToString("N").Substring(0, 4)}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {quociente}");
            _codigoAssembly.Add($"STORE {quociente}");
            _codigoAssembly.Add($"L_QUOC_POS_{Guid.NewGuid().ToString("N").Substring(0, 4)}:");

            _codigoAssembly.Add($"LOAD {sinalResto}");
            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"SUB {sinalResto}");
            _codigoAssembly.Add($"JZ L_RESTO_POS_{Guid.NewGuid().ToString("N").Substring(0, 4)}");
            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"SUB {dividendoAbs}");
            _codigoAssembly.Add($"STORE {dividendoAbs}");
            _codigoAssembly.Add($"L_RESTO_POS_{Guid.NewGuid().ToString("N").Substring(0, 4)}:");


            if (op == TipoOperacao.DIV)
            {
                _codigoAssembly.Add($"LOAD {quociente}");
                _codigoAssembly.Add($"STORE {endResultado}");
            }
            else
            {
                _codigoAssembly.Add($"LOAD {dividendoAbs}");
                _codigoAssembly.Add($"STORE {endResultado}");
            }
            _codigoAssembly.Add($"{fimDiv}:");
        }


        private void GerarRelacional(TipoOperacao op, string resultado, string arg1, string arg2)
        {

            int endResultado = ObterOuAlocarEndereco(resultado);
            string labelTrue = $"L_TRUE_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            string labelEndRel = $"L_END_REL_{Guid.NewGuid().ToString("N").Substring(0, 6)}";

            CarregarValorEmAX(arg1);

            if (int.TryParse(arg2, out int constanteArg2))
            {
                _codigoAssembly.Add($"SUBI {constanteArg2}");
            }
            else
            {
                int endArg2 = ObterOuAlocarEndereco(arg2);
                _codigoAssembly.Add($"SUB {endArg2}");
            }

            switch (op)
            {
                case TipoOperacao.EQUAL:
                    _codigoAssembly.Add($"JZ {labelTrue}");
                    break;
                case TipoOperacao.NOT_EQUAL:
                    _codigoAssembly.Add($"JNZ {labelTrue}");
                    break;
                case TipoOperacao.LESS_THAN:
                    _codigoAssembly.Add($"JLT {labelTrue}");
                    break;
                case TipoOperacao.LESS_EQUAL:
                    _codigoAssembly.Add($"JLE {labelTrue}");
                    break;
                case TipoOperacao.GREATER_THAN:
                    _codigoAssembly.Add($"JGT {labelTrue}");
                    break;
                case TipoOperacao.GREATER_EQUAL:
                    _codigoAssembly.Add($"JGE {labelTrue}");
                    break;
            }

            _codigoAssembly.Add($"LOADI 0");
            _codigoAssembly.Add($"STORE {endResultado}");
            _codigoAssembly.Add($"JMP {labelEndRel}");

            _codigoAssembly.Add($"{labelTrue}:");
            _codigoAssembly.Add($"LOADI 1");
            _codigoAssembly.Add($"STORE {endResultado}");

            _codigoAssembly.Add($"{labelEndRel}:");
        }
    }
}