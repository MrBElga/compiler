![Language](https://img.shields.io/badge/Language-C%23-blue.svg)
![Framework](https://img.shields.io/badge/.NET-Framework%204.7.2-blueviolet.svg)
![Build](https://img.shields.io/badge/Build-MSBuild-lightgrey.svg)
![Status](https://img.shields.io/badge/Status-Academic%20Project-orange.svg)
![License](https://img.shields.io/badge/License-MIT-yellow.svg)

# Compilador Didático Completo em C#

## 🚀 Sobre o Projeto

Este repositório abriga um compilador didático desenvolvido como parte da disciplina de **Compiladores**. O projeto implementa todas as fases clássicas de um compilador, desde a análise léxica do código fonte até a geração de código de máquina para um simulador (SimpSIM).

O objetivo é fornecer uma ferramenta educacional que demonstre de forma prática os conceitos fundamentais e os processos envolvidos na construção de compiladores.

## ✨ Funcionalidades Implementadas

* **Análise Léxica:**
    * Reconhecimento e classificação de tokens (palavras-chave, identificadores, operadores, números inteiros e reais, literais de char e string, booleanos).
    * Geração de relatório detalhado da análise léxica em arquivo `.txt`.
* **Análise Sintática:**
    * Implementação de um analisador sintático preditivo descendente LL(1).
    * Validação da estrutura gramatical do código-fonte.
    * Identificação precisa de erros sintáticos, indicando a linha e a natureza do erro.
    * Mecanismos de recuperação de erro (sincronização).
* **Análise Semântica:**
    * Construção e gerenciamento de Tabela de Símbolos para controle de escopo e declarações.
    * Verificação de tipos em atribuições e expressões.
    * Detecção de erros como variáveis não declaradas, dupla declaração, tipos incompatíveis e uso de variáveis não inicializadas.
    * Avisos para variáveis declaradas, mas não utilizadas.
* **Geração de Código Intermediário:**
    * Produção de código de três endereços (ou similar) com uso de variáveis temporárias e rótulos.
    * Tradução de atribuições, expressões aritméticas e estruturas de controle (`If`, `While`).
    * Geração de relatório do código intermediário em arquivo `.txt`.
* **Otimização de Código Intermediário:**
    * Implementação de diversas técnicas de otimização, incluindo:
        * Remoção de Atribuições de Identidade (`x = x`).
        * Remoção de Atribuições Inúteis (Dead Code Elimination).
        * Propagação de Constantes.
        * Cálculo de Constantes (Constant Folding).
        * Eliminação de Subexpressões Comuns (local).
        * Remoção de Saltos Inúteis.
        * Simplificação Aritmética Trivial (`x+0`, `y*1`, etc.).
        * Propagação de Cópias.
    * Geração de relatório do código otimizado em arquivo `.txt`.
* **Geração de Código de Máquina (Simulador SimpSIM):**
    * Tradução do código intermediário otimizado para a linguagem de montagem do SimpSIM.
    * Mapeamento de variáveis para posições de memória.
    * Geração de instruções para operações aritméticas básicas (`+`, `-`, `*`, `/`).
    * Geração de instruções para operações relacionais e saltos condicionais (`ifFalse`).
    * Implementação das estruturas de controle `If/Else` e `While`.
    * Geração de declarações de variáveis e instrução `HALT`.
* **Interface Gráfica (Windows Forms):**
    * Editor de código com numeração de linhas e destaque de sintaxe básico.
    * Painel para exibição de mensagens de erro das diversas fases da compilação.
    * Funcionalidades de manipulação de arquivos (Novo, Abrir, Salvar).
    * Botão para iniciar o processo completo de compilação.
    * Destaque visual das linhas com erros léxicos, sintáticos e semânticos no editor.
    * Navegação para a linha do erro com duplo clique na mensagem de erro.


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
│   ├── Analises/
│   │   ├── Analise_Lexica.cs
│   │   ├── Analise_Sintatica.cs
│   │   ├── Analise_Semantica.cs
│   │   ├── GeradorCodigoIntermediario.cs
│   │   ├── OtimizadorCodigo.cs
│   │   ├── GeradorCodigoSimpSIM.cs
│   │   ├── Automato.cs
│   │   └── Token.cs
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
