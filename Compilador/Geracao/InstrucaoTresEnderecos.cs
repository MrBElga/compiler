// Dentro de um arquivo como InstrucaoTresEnderecos.cs
// namespace Compilador.Geracao; // Ou Compilador.Sintese

using System; // Necessário para ArgumentException

public enum TipoOperacao
{
    // Atribuição
    ASSIGN,         // resultado = arg1
    COPY,           // resultado = arg1 (para cópias diretas, pode ser otimizado)

    // Aritméticas
    ADD,            // resultado = arg1 + arg2
    SUB,            // resultado = arg1 - arg2
    MULT,           // resultado = arg1 * arg2
    DIV,            // resultado = arg1 / arg2 (divisão inteira)
    MOD,            // resultado = arg1 % arg2 (resto da divisão)
    UNARY_MINUS,    // resultado = -arg1

    // Saltos e Labels
    LABEL,          // Define um label: (Nome do label armazenado em Resultado)
    GOTO,           // Salto incondicional: GOTO LabelDestino
    IF_FALSE_GOTO,  // Salto condicional: IF arg1 == false GOTO LabelDestino
    IF_TRUE_GOTO,   // Salto condicional: IF arg1 == true GOTO LabelDestino

    // Operações Relacionais (resultado será 0 para falso, 1 para verdadeiro)
    // resultado = arg1 op arg2
    EQUAL,
    NOT_EQUAL,
    LESS_THAN,
    LESS_EQUAL,
    GREATER_THAN,
    GREATER_EQUAL,

    // Chamada de Procedimento/Função
    PARAM,          // Define um parâmetro: PARAM arg1
    CALL,           // Chama uma função: [resultado =] CALL arg1 (nome da func), arg2 (num_params)
    RETURN,         // Retorna de uma função: RETURN [arg1 (valor_retorno)]

    // Leitura e Escrita
    READ,           // Lê valor para: resultado
    WRITE           // Escreve valor de: arg1
}

public class InstrucaoTresEnderecos
{
    public TipoOperacao Operacao { get; private set; }
    public string Resultado { get; private set; }
    public string Argumento1 { get; private set; }
    public string Argumento2 { get; private set; }
    public string LabelDestino { get; private set; }

    // Construtor privado para ser chamado pelos métodos fábrica
    private InstrucaoTresEnderecos(TipoOperacao operacao)
    {
        Operacao = operacao;
    }

    // --- Métodos Fábrica Estáticos ---

    public static InstrucaoTresEnderecos CriarOperacaoBinaria(TipoOperacao op, string resultado, string arg1, string arg2)
    {
        if (op < TipoOperacao.ADD || op > TipoOperacao.GREATER_EQUAL) // Validação básica do tipo de operação
            Console.WriteLine($"Alerta: CriarOperacaoBinaria chamada com op não binária: {op}");

        var inst = new InstrucaoTresEnderecos(op);
        inst.Resultado = resultado;
        inst.Argumento1 = arg1;
        inst.Argumento2 = arg2;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarAtribuicaoOuUnaria(TipoOperacao op, string resultado, string arg1)
    {
        if (op != TipoOperacao.ASSIGN && op != TipoOperacao.COPY && op != TipoOperacao.UNARY_MINUS)
            Console.WriteLine($"Alerta: CriarAtribuicaoOuUnaria chamada com op inadequada: {op}");

        var inst = new InstrucaoTresEnderecos(op);
        inst.Resultado = resultado;
        inst.Argumento1 = arg1;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarRead(string resultado)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.READ);
        inst.Resultado = resultado;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarWrite(string arg1)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.WRITE);
        inst.Argumento1 = arg1;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarLabel(string nomeLabel)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.LABEL);
        inst.Resultado = nomeLabel; // Convenção: nome do label armazenado em Resultado
        return inst;
    }

    public static InstrucaoTresEnderecos CriarGoto(string labelDestino)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.GOTO);
        inst.LabelDestino = labelDestino;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarSaltoCondicional(TipoOperacao opSalto, string condicao, string labelDestino)
    {
        if (opSalto != TipoOperacao.IF_FALSE_GOTO && opSalto != TipoOperacao.IF_TRUE_GOTO)
            throw new ArgumentException("Operação deve ser IF_FALSE_GOTO ou IF_TRUE_GOTO.", nameof(opSalto));

        var inst = new InstrucaoTresEnderecos(opSalto);
        inst.Argumento1 = condicao; // Condição (geralmente um temporário booleano)
        inst.LabelDestino = labelDestino;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarParam(string nomeParametro)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.PARAM);
        inst.Argumento1 = nomeParametro;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarCall(string nomeFuncao, string numArgumentos, string variavelRetorno)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.CALL);
        inst.Resultado = variavelRetorno; // Pode ser null se for um procedimento (void)
        inst.Argumento1 = nomeFuncao;
        inst.Argumento2 = numArgumentos;
        return inst;
    }

    public static InstrucaoTresEnderecos CriarReturn(string valorDeRetorno)
    {
        var inst = new InstrucaoTresEnderecos(TipoOperacao.RETURN);
        inst.Argumento1 = valorDeRetorno; // Pode ser null se for um retorno de void
        return inst;
    }

    // --- Representação em String ---
    public override string ToString()
    {
        switch (Operacao)
        {
            case TipoOperacao.ASSIGN:
            case TipoOperacao.COPY:
                return $"{Resultado} = {Argumento1}";
            case TipoOperacao.UNARY_MINUS:
                return $"{Resultado} = -{Argumento1}"; // Mais intuitivo que "OPERACAO Argumento1"
            case TipoOperacao.READ:
                return $"{Resultado} = READ";

            case TipoOperacao.ADD:
                return $"{Resultado} = {Argumento1} + {Argumento2}";
            case TipoOperacao.SUB:
                return $"{Resultado} = {Argumento1} - {Argumento2}";
            case TipoOperacao.MULT:
                return $"{Resultado} = {Argumento1} * {Argumento2}";
            case TipoOperacao.DIV:
                return $"{Resultado} = {Argumento1} / {Argumento2}";
            case TipoOperacao.MOD:
                return $"{Resultado} = {Argumento1} % {Argumento2}";

            case TipoOperacao.EQUAL:
                return $"{Resultado} = {Argumento1} == {Argumento2}";
            case TipoOperacao.NOT_EQUAL:
                return $"{Resultado} = {Argumento1} != {Argumento2}";
            case TipoOperacao.LESS_THAN:
                return $"{Resultado} = {Argumento1} < {Argumento2}";
            case TipoOperacao.LESS_EQUAL:
                return $"{Resultado} = {Argumento1} <= {Argumento2}";
            case TipoOperacao.GREATER_THAN:
                return $"{Resultado} = {Argumento1} > {Argumento2}";
            case TipoOperacao.GREATER_EQUAL:
                return $"{Resultado} = {Argumento1} >= {Argumento2}";

            case TipoOperacao.LABEL:
                return $"{Resultado}:"; // Nome do label está em Resultado

            case TipoOperacao.GOTO:
                return $"GOTO {LabelDestino}";

            case TipoOperacao.IF_FALSE_GOTO:
                return $"IF_FALSE {Argumento1} GOTO {LabelDestino}";
            case TipoOperacao.IF_TRUE_GOTO:
                return $"IF_TRUE {Argumento1} GOTO {LabelDestino}";

            case TipoOperacao.WRITE:
                return $"WRITE {Argumento1}";

            case TipoOperacao.PARAM:
                return $"PARAM {Argumento1}";
            case TipoOperacao.CALL:
                return (Resultado != null ? $"{Resultado} = " : "") + $"CALL {Argumento1}, {Argumento2}";
            case TipoOperacao.RETURN:
                return Argumento1 != null ? $"RETURN {Argumento1}" : "RETURN";

            default:
                return $"{Operacao} (ToString não implementado completamente: Res={Resultado}, Arg1={Argumento1}, Arg2={Argumento2}, Label={LabelDestino})";
        }
    }
}