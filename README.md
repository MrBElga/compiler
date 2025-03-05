# Compilador - Análise Léxica e Sintática

## 📌 Descrição do Projeto

Este repositório contém a implementação de um compilador com análise léxica e sintática, utilizando uma abordagem **preditiva com pilha ou recursiva**. O projeto foi desenvolvido para a disciplina de **Compiladores** e será apresentado conforme o cronograma estabelecido pelo professor.

O compilador realiza:

- **Análise Léxica** → Reconhecimento de tokens e detalhamento no reconhecimento.
- **Análise Sintática** → Implementação da análise preditiva, identificação e tratamento de erros.
- **Interface** → Destacando linha de erro, limpando tela e garantindo um código limpo e sem "gambiarras".

---

## 📜 Estrutura da Gramática

A gramática utilizada segue as regras formais para construção de um analisador sintático LL(1), garantindo uma abordagem eficiente e previsível na análise do código-fonte.

### **Definição da Linguagem e Comandos** 

- A linguagem suporta **declarações de variáveis, comandos de controle de fluxo (**`**, **`**, ****\`\`****) e expressões matemáticas**.
- A estrutura da linguagem é inspirada em sintaxes tradicionais, como C e Java.

### **Conjuntos First e Follow**

- **First** define os tokens que podem iniciar uma determinada construção gramatical.
- **Follow** define os tokens que podem aparecer imediatamente após uma construção gramatical.

Detalhes sobre esses conjuntos estão documentados no código e nos arquivos de referência.

---

## 🛠️ Implementação Técnica

### **1️⃣ Análise Léxica**

A análise léxica realiza:

- **Reconhecimento dos tokens** → Identificação e classificação das palavras-chave, identificadores, operadores e delimitadores.
- **Detalhamento no reconhecimento** → Extração de informações relevantes para a próxima etapa do compilador.

### **2️⃣ Análise Sintática** 

- **Método preditivo com pilha ou recursivo** → Implementação da análise descendente sem retrocesso.
- **Realização completa da etapa sintática** → Construção da árvore sintática e validação da estrutura do código.
- **Identificação e indicação dos erros** → etecção de erros sintáticos, com exibição da linha e do tipo de erro.
- **Tratamento de erros** → Fornecimento de mensagens detalhadas para correção do código.

---






