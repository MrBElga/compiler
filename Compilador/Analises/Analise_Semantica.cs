using System.Collections.Generic;

namespace Compilador.Analises
{
    public class Simbolo
    {
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public bool Inicializado { get; set; }
        public bool Usado { get; set; }
        public int LinhaDeclaracao { get; set; }
    }

    public class TabelaSimbolos
    {
        private Dictionary<string, Simbolo> simbolos = new Dictionary<string, Simbolo>();

        public void Adicionar(string nome, string tipo, int linha)
        {
            if (simbolos.ContainsKey(nome))
            {
            }
            else
            {
                simbolos[nome] = new Simbolo
                {
                    Nome = nome,
                    Tipo = tipo,
                    Inicializado = false,
                    Usado = false,
                    LinhaDeclaracao = linha
                };
            }
        }

        public Simbolo Buscar(string nome)
        {
            simbolos.TryGetValue(nome, out var simbolo);
            return simbolo;
        }

        public IEnumerable<Simbolo> Todos() => simbolos.Values;
    }

    public class Analise_Semantica
    {
        private readonly List<Token> tokens;
        private readonly TabelaSimbolos tabelaSimbolos = new TabelaSimbolos();
        private int currentTokenIndex = 0;

        public List<string> Erros { get; }

        public Analise_Semantica(List<Token> tokens)
        {
            this.tokens = tokens;
            this.Erros = new List<string>();
        }

        private Token CurrentToken => tokens[currentTokenIndex];
        private Token LookaheadToken => (currentTokenIndex + 1 < tokens.Count) ? tokens[currentTokenIndex + 1] : CurrentToken;

        private void Advance()
        { if (currentTokenIndex < tokens.Count - 1) currentTokenIndex++; }

        private bool Match(string expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                Advance();
                return true;
            }
            return false;
        }

        public void Analisar()
        {
            if (CurrentToken.Type == "t_programa")
            {
                Advance();
                if (CurrentToken.Type == "t_id")
                {
                    Advance();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado identificador após 'Program'.");
                    Advance();
                }
            }
            else
            {
            }

            if (CurrentToken.Type == "t_abreBloco")
            {
                Advance();
            }
            else
            {
            }

            while (CurrentToken.Type != "t_fechaBloco" && CurrentToken.Type != "EOF")
            {
                AnalisarComandoOuDeclaracao();
            }

            if (CurrentToken.Type == "t_fechaBloco")
            {
                Advance();
            }

            if (CurrentToken.Type != "EOF")
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: Tokens inesperados após o fim do programa.");
                while (CurrentToken.Type != "EOF")
                {
                    Advance();
                }
            }

            VerificarVariaveisNaoUsadas();
        }

        private void AnalisarComandoOuDeclaracao()
        {
            if (IsTipo(CurrentToken.Type))
            {
                AnalisarDeclaracao();
            }
            else if (CurrentToken.Type == "t_id")
            {
                if (LookaheadToken.Type == "t_atribuicao")
                {
                    AnalisarAtribuicao();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: identificador '{CurrentToken.Lexeme}' não inicia uma atribuição válida.");
                    Advance();
                }
            }
            else if (CurrentToken.Type == "t_if")
            {
                AnalisarComandoIf();
            }
            else if (CurrentToken.Type == "t_while")
            {
                AnalisarComandoWhile();
            }
            else if (CurrentToken.Type == "t_abreBloco")
            {
                AnalisarBloco();
            }
            else if (CurrentToken.Type == "t_else")
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: 'else' inesperado sem um 'if' correspondente.");
                Advance();
            }
            else if (CurrentToken.Type != "t_fechaBloco" && CurrentToken.Type != "EOF")
            {
                Advance();
            }
        }

        private void AnalisarComandoIf()
        {
            Advance();
            if (CurrentToken.Type == "t_abreParen")
            {
                Advance();
                AnalisarExpressaoRelacional();
                if (CurrentToken.Type == "t_fechaParen")
                {
                    Advance();
                    AnalisarBloco();

                    if (CurrentToken.Type == "t_else")
                    {
                        Advance();
                        AnalisarBloco();
                    }
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ')' após condição do if.");
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado '(' após 'if'.");
            }
        }

        private void AnalisarExpressaoRelacional()
        {
            string tipoPrimeiroOperando = null;
            string tipoSegundoOperando = null;
            string operadorRelacional = null;
            int linhaInicioExpressao = CurrentToken.LineNumber;

            tipoPrimeiroOperando = AnalisarExpressao(null);

            if (IsOperadorRelacional(CurrentToken.Type))
            {
                operadorRelacional = CurrentToken.Type; // Captura o tipo do operador
                Advance();

                // analisa o segundo operando/expressão.
                tipoSegundoOperando = AnalisarExpressao(null); // passei null pelo mesmo motivo

                // --- Verificação Crucial de Compatibilidade de Tipos para Operadores Relacionais ---
                // só verifica se ambos os operandos foram analisados sem terem sido marcados como ERRO léxico/sintático
                if (tipoPrimeiroOperando != "ERRO" && tipoSegundoOperando != "ERRO")
                {
                    bool compativel = false;
                    string mensagemDetalheErro = "";
                    string lexemaOperador = CurrentToken.Lexeme;

                    // Capturar o lexema do operador para a mensagem de erro
                    string operadorLexemaParaErro = "";
                    switch (operadorRelacional)
                    {
                        case "t_igualdade": operadorLexemaParaErro = "=="; break;
                        case "t_diferenca": operadorLexemaParaErro = "!="; break;
                        case "t_menor": operadorLexemaParaErro = "<"; break;
                        case "t_maior": operadorLexemaParaErro = ">"; break;
                        case "t_menor_igual": operadorLexemaParaErro = "<="; break;
                        case "t_maior_igual": operadorLexemaParaErro = ">="; break;
                        default: operadorLexemaParaErro = operadorRelacional; break;
                    }

                    if (operadorRelacional == "t_igualdade" || operadorRelacional == "t_diferenca")
                    {
                        // para == e !=, tipos devem ser os mesmos ou compatíveis (numérico com numérico, booleano com booleano, string com string, char com char)
                        if (tipoPrimeiroOperando == tipoSegundoOperando)
                        {
                            compativel = true;
                        }
                        else if (SaoTiposNumericos(tipoPrimeiroOperando) && SaoTiposNumericos(tipoSegundoOperando))
                        {
                            compativel = true; // Integer == Float ou Float == Integer é permitido
                        }
                        //aq daria par por tipo char se nossa gramatica aceitar e outras coisas
                        else if (tipoPrimeiroOperando == "t_string" && tipoSegundoOperando == "t_string") compativel = true;
                        else if (tipoPrimeiroOperando == "t_char" && tipoSegundoOperando == "t_char") compativel = true;

                        if (!compativel)
                        {
                            mensagemDetalheErro = $"Tipos incompatíveis '{tipoPrimeiroOperando}' e '{tipoSegundoOperando}' para operador de igualdade/diferença ('{operadorLexemaParaErro}').";
                        }
                    }
                    else
                    {
                        if (SaoTiposNumericos(tipoPrimeiroOperando) && SaoTiposNumericos(tipoSegundoOperando))
                        {
                            compativel = true;
                        }
                        mensagemDetalheErro = $"Tipos não numéricos '{tipoPrimeiroOperando}' e '{tipoSegundoOperando}' para operador relacional ('{operadorLexemaParaErro}'). Ambos devem ser Integer ou Float.";
                    }

                    if (!compativel)
                    {
                        Erros.Add($"Erro Semântico na linha {linhaInicioExpressao}: {mensagemDetalheErro}");
                    }
                }
            }
            else
            {
                if (tipoPrimeiroOperando != "t_boolean" && tipoPrimeiroOperando != "ERRO")
                {
                    Erros.Add($"Erro Semântico na linha {linhaInicioExpressao}: Expressão condicional deve resultar em um valor booleano (encontrado '{tipoPrimeiroOperando}').");
                }
            }
        }

        private bool IsOperadorRelacional(string tipo)
        {
            return tipo == "t_igualdade" || tipo == "t_diferenca" ||
                   tipo == "t_menor" || tipo == "t_maior" ||
                   tipo == "t_menor_igual" || tipo == "t_maior_igual";
        }

        private bool SaoTiposNumericos(string tipo)
        {
            return tipo == "t_integer" || tipo == "t_float";
        }

        private bool IsTipo(string tipo) =>
            tipo == "t_integer" || tipo == "t_float" || tipo == "t_char" || tipo == "t_string" || tipo == "t_boolean";

        private void AnalisarComandoWhile()
        {
            Advance();
            if (CurrentToken.Type == "t_abreParen")
            {
                Advance();
                AnalisarExpressaoRelacional();
                if (CurrentToken.Type == "t_fechaParen")
                {
                    Advance();
                    AnalisarBloco();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ')' após condição do while.");
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado '(' após 'while'.");
            }
        }

        private void AnalisarBloco()
        {
            if (CurrentToken.Type == "t_abreBloco")
            {
                Advance();

                while (CurrentToken.Type != "t_fechaBloco" && CurrentToken.Type != "EOF")
                {
                    AnalisarComandoOuDeclaracao();
                }

                if (CurrentToken.Type == "t_fechaBloco")
                {
                    Advance();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado '}}' para fechar bloco.");
                }
            }
            else
            {
                AnalisarComandoOuDeclaracao();
            }
        }

        private void AnalisarDeclaracao()
        {
            string tipoDeclarado = CurrentToken.Type;
            Advance(); // Consome o tipo (Integer, Float, etc.)
            while (CurrentToken.Type == "t_id")
            {
                string nomeVar = CurrentToken.Lexeme;
                int linhaDeclaracao = CurrentToken.LineNumber;

                // --- Verificação semântica: variável já declarada no escopo atual ---
                // Busca a variável na tabela de símbolos. Como a tabela é global,
                // encontrar um símbolo significa que já foi declarado no escopo global.
                if (tabelaSimbolos.Buscar(nomeVar) != null)
                {
                    Erros.Add($"Erro Semântico na linha {linhaDeclaracao}: variável '{nomeVar}' já declarada neste escopo.");
                }
                else
                {
                    tabelaSimbolos.Adicionar(nomeVar, tipoDeclarado, linhaDeclaracao);
                }
                // --- Fim da adição da verificação ---

                Advance();
                if (CurrentToken.Type == "t_atribuicao")
                {
                    Advance();
                    // Analisa a expressão de inicialização e verifica compatibilidade de tipo
                    string tipoAtribuido = AnalisarExpressao(tipoDeclarado);

                    // Se a expressão de inicialização não teve erros internos e pudemos determinar seu tipo
                    if (tipoAtribuido != "ERRO")
                    {
                        // Verifica se o tipo atribuído é compatível com o tipo declarado
                        if (!TiposCompativeis(tipoDeclarado, tipoAtribuido))
                        {
                            Erros.Add($"Erro Semântico na linha {linhaDeclaracao}: tipo incompatível na inicialização da variável '{nomeVar}' (esperado '{tipoDeclarado}', encontrado '{tipoAtribuido}').");
                            var simbolo = tabelaSimbolos.Buscar(nomeVar); // Busca novamente, caso não tenha sido adicionada por ser duplicada
                            if (simbolo != null) simbolo.Inicializado = false;
                        }
                        else
                        {
                            var simbolo = tabelaSimbolos.Buscar(nomeVar);
                            if (simbolo != null) simbolo.Inicializado = true;
                        }
                    }
                    else
                    {
                        // A expressão de inicialização continha erros, a variável não é considerada inicializada com sucesso por esta atribuição.
                        var simbolo = tabelaSimbolos.Buscar(nomeVar);
                        if (simbolo != null) simbolo.Inicializado = false;
                    }
                }
                else
                {
                    // Variável declarada sem inicialização explícita (Inicializado permanece false por padrão na TabelaSimbolos)
                }

                // Verifica se há mais IDs na mesma declaração (separados por vírgula)
                if (CurrentToken.Type == "t_virgula")
                {
                    Advance();
                    // Continua o loop para o próximo ID
                }
                else
                {
                    break; // Sai do loop se não houver vírgula
                }
            }

            if (CurrentToken.Type == "t_ponto_virgula")
            {
                Advance();
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ';' após declaração de variável.");
            }
        }

        private void AnalisarAtribuicao()
        {
            string nomeVar = CurrentToken.Lexeme;
            int linhaAtribuicao = CurrentToken.LineNumber;
            var simbolo = tabelaSimbolos.Buscar(nomeVar);

            if (simbolo == null)
            {
                Erros.Add($"Erro Semântico na linha {linhaAtribuicao}: variável '{nomeVar}' não declarada.");
                Advance();
                if (CurrentToken.Type == "t_atribuicao")
                {
                    Advance();
                    AnalisarExpressao(null);
                }
                if (CurrentToken.Type == "t_ponto_virgula") Advance();
                return;
            }

            simbolo.Usado = true;

            Advance();

            if (CurrentToken.Type == "t_atribuicao")
            {
                Advance();

                string tipoAtribuido = AnalisarExpressao(simbolo.Tipo);

                if (tipoAtribuido != "ERRO")
                {
                    if (!TiposCompativeis(simbolo.Tipo, tipoAtribuido))
                    {
                        Erros.Add($"Erro Semântico na linha {linhaAtribuicao}: tipo incompatível na atribuição à variável '{nomeVar}' (esperado '{simbolo.Tipo}', encontrado '{tipoAtribuido}').");
                    }
                    else
                    {
                        simbolo.Inicializado = true;
                    }
                }
                else
                {
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {linhaAtribuicao}: esperado '=' após identificador '{nomeVar}' em uma atribuição.");
            }

            if (CurrentToken.Type == "t_ponto_virgula")
            {
                Advance();
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ';' após comando de atribuição.");
            }
        }

        private string AnalisarExpressao(string tipoEsperado)
        {
            string tipoExpressao = AnalisarTermo(tipoEsperado);

            while (CurrentToken.Type == "t_soma" || CurrentToken.Type == "t_subtracao")
            {
                string operador = CurrentToken.Lexeme;
                Advance();
                string tipoProximoTermo = AnalisarTermo(tipoEsperado);

                if (tipoExpressao != "ERRO" && tipoProximoTermo != "ERRO")
                {
                    if (!SaoTiposNumericos(tipoExpressao) || !SaoTiposNumericos(tipoProximoTermo))
                    {
                        Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: operandos de tipo incompatível para operador '{operador}'. Ambos devem ser Integer ou Float.");
                        tipoExpressao = "ERRO";
                    }
                    else
                    {
                        if (tipoExpressao == "t_integer" && tipoProximoTermo == "t_integer")
                        {
                        }
                        else
                        {
                            tipoExpressao = "t_float";
                        }
                    }
                }
                else
                {
                    tipoExpressao = "ERRO";
                }
            }

            return tipoExpressao;
        }

        private string AnalisarTermo(string tipoEsperado)
        {
            string tipoTermo = AnalisarFator(tipoEsperado);

            while (CurrentToken.Type == "t_multiplicacao" || CurrentToken.Type == "t_divisao")
            {
                string operador = CurrentToken.Lexeme;
                Advance();
                string tipoProximoFator = AnalisarFator(tipoEsperado);

                if (tipoTermo != "ERRO" && tipoProximoFator != "ERRO")
                {
                    if (!SaoTiposNumericos(tipoTermo) || !SaoTiposNumericos(tipoProximoFator))
                    {
                        Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: operandos de tipo incompatível para operador '{operador}'. Ambos devem ser Integer ou Float.");
                        tipoTermo = "ERRO";
                    }
                    else
                    {
                        if (tipoTermo == "t_integer" && tipoProximoFator == "t_integer")
                        {
                        }
                        else
                        {
                            tipoTermo = "t_float";
                        }
                    }
                }
                else
                {
                    tipoTermo = "ERRO";
                }
            }

            return tipoTermo;
        }

        private string AnalisarFator(string tipoEsperado)
        {
            string tipoFator = null;

            if (CurrentToken.Type == "t_id")
            {
                string nomeVar = CurrentToken.Lexeme;
                int linhaUso = CurrentToken.LineNumber;
                var simbolo = tabelaSimbolos.Buscar(nomeVar);

                if (simbolo == null)
                {
                    Erros.Add($"Erro Semântico na linha {linhaUso}: variável '{nomeVar}' não declarada.");
                    tipoFator = "ERRO";
                }
                else
                {
                    if (!simbolo.Inicializado)
                    {
                        Erros.Add($"Erro Semântico na linha {linhaUso}: variável '{nomeVar}' usada sem inicialização.");
                    }

                    simbolo.Usado = true;
                    tipoFator = simbolo.Tipo;

                    if (tipoEsperado != null && !TiposCompativeis(tipoEsperado, tipoFator))
                    {
                        Erros.Add($"Erro Semântico na linha {linhaUso}: tipo incompatível em expressão (esperado '{tipoEsperado}', encontrado '{tipoFator}').");
                    }
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_numero_int")
            {
                tipoFator = "t_integer";
                if (tipoEsperado != null && !TiposCompativeis(tipoEsperado, tipoFator))
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado '{tipoEsperado}', encontrado '{tipoFator}').");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_numero_real")
            {
                tipoFator = "t_float";
                if (tipoEsperado != null && !TiposCompativeis(tipoEsperado, tipoFator))
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado '{tipoEsperado}', encontrado '{tipoFator}').");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_char_literal")
            {
                tipoFator = "t_char";
                if (tipoEsperado != null && tipoEsperado != "t_char")
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado '{tipoEsperado}', encontrado '{tipoFator}').");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_string_literal")
            {
                tipoFator = "t_string";
                if (tipoEsperado != null && tipoEsperado != "t_string")
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado '{tipoEsperado}', encontrado '{tipoFator}').");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_bool")
            {
                tipoFator = "t_boolean";
                if (tipoEsperado != null && tipoEsperado != "t_boolean")
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: tipo incompatível (esperado '{tipoEsperado}', encontrado '{tipoFator}').");
                }
                Advance();
            }
            else if (CurrentToken.Type == "t_abreParen")
            {
                Advance();
                tipoFator = AnalisarExpressao(null);
                if (CurrentToken.Type == "t_fechaParen")
                {
                    Advance();
                }
                else
                {
                    Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: esperado ')' para fechar expressão entre parênteses.");
                    tipoFator = "ERRO";
                }
            }
            else
            {
                Erros.Add($"Erro Semântico na linha {CurrentToken.LineNumber}: Token inesperado '{CurrentToken.Lexeme}' no início de um fator.");
                Advance();
                tipoFator = "ERRO";
            }

            return tipoFator;
        }

        private bool TiposCompativeis(string esperado, string encontrado)
        {
            if (esperado == "ERRO" || encontrado == "ERRO") return false;
            if (esperado == encontrado) return true;

            if (esperado == "t_float" && encontrado == "t_integer") return true;

            return false;
        }

        private void VerificarVariaveisNaoUsadas()
        {
            foreach (var simbolo in tabelaSimbolos.Todos())
            {
                if (!simbolo.Usado)
                {
                    Erros.Add($"Aviso Semântico na linha {simbolo.LinhaDeclaracao}: variável '{simbolo.Nome}' declarada mas nunca usada.");
                }
            }
        }
    }
}