# Compilador Didático - Análise Léxica e Sintática

## 🚀 Sobre o Projeto

Este repositório abriga um compilador desenvolvido como parte da disciplina de **Compiladores**. O foco principal está na implementação das fases de **análise léxica** e **análise sintática**, utilizando uma abordagem **preditiva descendente (recursiva ou com pilha)**.

O objetivo é fornecer uma ferramenta didática que demonstre os conceitos fundamentais da construção de compiladores.

## ✨ Funcionalidades Implementadas

* **Análise Léxica:**
    * Reconhecimento e classificação de tokens (palavras-chave, identificadores, operadores, literais, etc.).
    * Geração de relatório detalhado da análise léxica em arquivo `.txt`[cite: 5, 6, 9, 13, 19, 20, 21, 22, 29, 33, 35, 41, 46, 47].
* **Análise Sintática:**
    * Implementação de um analisador sintático preditivo descendente (LL(1))[cite: 3].
    * Validação da estrutura gramatical do código-fonte.
    * Identificação precisa de erros sintáticos, indicando a linha e a natureza do erro[cite: 3].
    * Fornecimento de mensagens de erro claras para auxiliar na correção[cite: 3].
* **Interface Gráfica (Windows Forms):**
    * Editor de código com numeração de linhas.
    * Painel para exibição de erros léxicos e sintáticos.
    * Funcionalidades básicas de manipulação de arquivos (Novo, Abrir, Salvar).
    * Botão para iniciar o processo de compilação (análise).
    * Destaque visual das linhas com erros no editor.

---

## 📜 Gramática e Linguagem

A linguagem de entrada suportada por este compilador foi definida com base nos requisitos da disciplina e inclui construções como:

* Declarações de variáveis (`Integer`, `Float`, `Char`, `String`, `Boolean`).
* Comandos de atribuição (`=`).
* Estruturas de controle de fluxo (`If`/`Else`, `While`).
* Expressões matemáticas e relacionais (`+`, `-`, `*`, `/`, `==`, `!=`, `<`, `>`, `<=`, `>=`).

A gramática formal utilizada segue as regras para a construção de analisadores LL(1), garantindo uma análise sintática eficiente e sem ambiguidades.

### Definição Formal da Gramática em BNF

```bnf
// Regra Inicial
<programa> ::= 'Program' t_id <bloco>

// Bloco de Código
<bloco> ::= '{' [<declaracao_variavel_lista>] [<comando_lista>] '}'

// Declarações
<declaracao_variavel_lista> ::= <declaracao_variavel> { <declaracao_variavel> }
<declaracao_variavel> ::= <tipo> <identificador_inicializador_lista> ';'
<tipo> ::= 'Integer' | 'Float' | 'Char' | 'String' | 'Boolean'
<identificador_inicializador_lista> ::= <identificador_inicializador> { ',' <identificador_inicializador> }
<identificador_inicializador> ::= t_id [ '=' <expr> ]

// Comandos
<comando_lista> ::= <comando> { <comando> }
<comando> ::= <comando_if> | <comando_while> | <comando_atribuicao> | <bloco>
<comando_atribuicao> ::= t_id '=' <expr> ';'
<comando_if> ::= 'If' '(' <expr_relacional> ')' <bloco> [ 'Else' <bloco> ]
<comando_while> ::= 'While' '(' <expr_relacional> ')' <bloco>

// Expressões
<expr_relacional> ::= <expr> <operador_relacional> <expr>
<expr> ::= <termo> { <operador_aditivo> <termo> }
<termo> ::= <fator> { <operador_multiplicativo> <fator> }
<fator> ::= t_id | t_numero_int | t_numero_real | t_bool | t_char_literal | t_string_literal | '(' <expr> ')'

// Operadores (Tokens)
<operador_relacional> ::= '==' | '!=' | '<' | '>' | '<=' | '>='
<operador_aditivo> ::= '+' | '-'
<operador_multiplicativo> ::= '*' | '/'

// Tokens (Terminais - definidos pelo Analisador Léxico)
// t_id, t_numero_int, t_numero_real, t_bool, t_char_literal, t_string_literal
// Palavras-chave: 'Program', 'Integer', 'Float', 'Char', 'String', 'Boolean', 'If', 'Else', 'While', 'true', 'false'
// Símbolos: '{', '}', '(', ')', '=', ',', '.', ';', '+', '-', '*', '/', '==', '!=', '<', '>', '<=', '>='´
```
### Conjuntos First e Follow (Resumo)

* **First(t_programa):** `{ 'Program' }`
* **First(t_bloco):** `{ '{' }`
* **First(declaracao_variavel):** `{ 'Integer', 'Float', 'Char', 'String', 'Boolean' }`
* **First(comando):** `{ 'If', 'While', t_id, '{' }`
* **First(expr):** `{ t_id, t_numero_int, t_numero_real, t_bool, t_char_literal, t_string_literal, '(' }`
* **First(expr_relacional):** `{ t_id, t_numero_int, t_numero_real, t_bool, t_char_literal, t_string_literal, '(' }`

* **Follow(t_programa):** `{ $ }` (EOF)
* **Follow(t_bloco):** Depende do contexto (ex: após `If`/`Else`/`While` ou fim do programa) - geralmente `{ 'Else', '}', $ }`
* **Follow(declaracao_variavel):** `{ 'Integer', 'Float', 'Char', 'String', 'Boolean', 'If', 'While', t_id, '{', '}' }`
* **Follow(comando):** `{ 'Integer', 'Float', 'Char', 'String', 'Boolean', 'If', 'While', t_id, '{', '}' }`
* **Follow(expr):** `{ ';', ')', '==', '!=', '<', '>', '<=', '>=', '}' }`
* **Follow(expr_relacional):** `{ ')' }`

*(Nota: Os conjuntos Follow podem ser mais complexos dependendo da derivação exata na implementação.)*

---

## 🛠️ Implementação Técnica

* **Linguagem:** C# (.NET Framework 4.7.2).
* **Interface:** Windows Forms.
* **Análise Léxica:** Implementada manualmente com um sistema de reconhecimento de tokens e palavras reservadas, utilizando uma classe `Automato` auxiliar.
* **Análise Sintática:** Implementada usando um método preditivo descendente recursivo.

---

## 🧠 Análise Semântica (Próximos Passos)

Após a validação da estrutura sintática, a próxima etapa natural no desenvolvimento deste compilador é a **análise semântica**. Esta fase é crucial para verificar o significado do código-fonte e garantir que ele faça sentido lógico, indo além da simples correção gramatical.

As principais tarefas planejadas para a análise semântica incluem:

* **Tabela de Símbolos:** Implementação e gerenciamento de uma tabela de símbolos para armazenar informações sobre identificadores (variáveis), como tipo e escopo.
* **Verificação de Tipos:**
    * Garantir que os tipos de dados sejam usados corretamente em atribuições e expressões (ex: não atribuir uma `String` a um `Integer`).
    * Verificar a compatibilidade de tipos em operações.
* **Controle de Escopo:**
    * Verificar se as variáveis foram declaradas antes de serem usadas.
    * Identificar declarações duplicadas de variáveis no mesmo escopo.

A implementação da análise semântica adicionará uma camada mais profunda de validação ao compilador.

---

## 📁 Estrutura de Pastas do Projeto
```bash
compiler/
│
├── Compilador/                  # Projeto principal do Compilador (Windows Forms App)
│   ├── Analises/               # Classes responsáveis pelas análises
│   │   ├── Analise_Lexica.cs   #
│   │   ├── Analise_Sintatica.cs#
│   │   ├── Automato.cs         #
│   │   └── Token.cs            #
│   │
│   ├── Properties/             # Arquivos de configuração do projeto
│   │   ├── AssemblyInfo.cs     #
│   │   ├── Resources.Designer.cs #
│   │   ├── Resources.resx
│   │   └── Settings.Designer.cs  #
│   │
│   ├── bin/                    # Arquivos compilados (executáveis, DLLs)
│   │   └── Debug/              # Versão de depuração
│   │       ├── Compilador.exe
│   │       ├── *.txt           # Arquivos de teste e relatórios gerados
│   │       └── ... (outras DLLs e arquivos)
│   │
│   ├── obj/                    # Arquivos temporários de compilação
│   │
│   ├── App.config              # Configuração da aplicação .NET
│   ├── Compilador.csproj       # Arquivo de projeto C#
│   ├── Form1.cs                # Código da interface gráfica principal
│   ├── Form1.Designer.cs       # Código gerado pelo designer do formulário
│   ├── Form1.resx
│   └── Program.cs              # Ponto de entrada da aplicação
│
├── LineNumbersControlForRichTextBox/ # Projeto da biblioteca para numeração de linhas
│   ├── LineNumbersForRichText.cs     #
│   ├── Properties/
│   │   └── AssemblyInfo.cs     #
│   └── LineNumbersControlForRichTextBox.csproj
│
├── Compilador.sln              # Arquivo da solução do Visual Studio
└── README.md                   # Este arquivo
```

*(Nota: A estrutura exata pode variar ligeiramente dependendo da configuração específica do Visual Studio e arquivos não incluídos no upload).*

---

## ⚙️ Como Usar

1.  **Pré-requisitos:**
    * .NET Framework 4.7.2 ou superior.
    * Ambiente de desenvolvimento C# (Visual Studio 2017 ou posterior recomendado).
2.  **Compilação:**
    * Clone o repositório.
    * Abra o arquivo `Compilador.sln` no Visual Studio.
    * Compile a solução (Menu Build > Build Solution ou F6).
3.  **Execução:**
    * Execute o arquivo `Compilador.exe` localizado na pasta `Compilador/bin/Debug/`.
    * Utilize a interface para:
        * Criar um novo arquivo (`Novo`).
        * Abrir um arquivo de código existente (`Abrir`).
        * Escrever ou colar o código-fonte no editor.
        * Salvar o código (`Salvar`).
        * Clicar no botão `Compilar` (ícone de play) para executar as análises.
        * Observar os resultados e erros no painel à direita. O relatório léxico também será salvo em um arquivo `.txt` na mesma pasta do arquivo de código-fonte.

---
