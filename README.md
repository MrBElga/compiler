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

### **Definição da Linguagem e Comandos** *(6,0 pontos)*

- A linguagem suporta **declarações de variáveis, comandos de controle de fluxo (**`**, **`**, ****\`\`****) e expressões matemáticas**.
- A estrutura da linguagem é inspirada em sintaxes tradicionais, como C e Java.

### **Conjuntos First e Follow** *(2,0 pontos cada)*

- **First** define os tokens que podem iniciar uma determinada construção gramatical.
- **Follow** define os tokens que podem aparecer imediatamente após uma construção gramatical.

Detalhes sobre esses conjuntos estão documentados no código e nos arquivos de referência.

---

## 🛠️ Implementação Técnica

### **1️⃣ Análise Léxica** *(6,0 pontos)*

A análise léxica realiza:

- **Reconhecimento dos tokens** → Identificação e classificação das palavras-chave, identificadores, operadores e delimitadores.
- **Detalhamento no reconhecimento** → Extração de informações relevantes para a próxima etapa do compilador.

### **2️⃣ Análise Sintática** *(5,0 pontos)*

- **Método preditivo com pilha ou recursivo** → Implementação da análise descendente sem retrocesso.
- **Realização completa da etapa sintática** *(2,0 pontos)* → Construção da árvore sintática e validação da estrutura do código.
- **Identificação e indicação dos erros** *(3,0 pontos)* → Detecção de erros sintáticos, com exibição da linha e do tipo de erro.
- **Tratamento de erros** → Fornecimento de mensagens detalhadas para correção do código.

---

## 🎯 Critérios de Avaliação

### **Entrega e Apresentação**

- 📅 **Análise Sintática** → Apresentação a partir do **10/03**, conforme cronograma.
- 📅 **Análise Léxica** → Apresentação a partir do **01/04**, conforme cronograma.
- ⚠ **O aluno que não apresentar ficará com ZERO.**
- 🚨 **Tópicos que o aluno não souber explicar serão desconsiderados.**

### **Pontuação Total**

- **Interface** *(4,0 pontos)* → Organização da tela, destaque de erros, código sem "gambiarras".
- **Análise Léxica** *(6,0 pontos)* → Reconhecimento e detalhamento de tokens.
- **Análise Sintática** *(5,0 pontos)* → Implementação preditiva, identificação e tratamento de erros.

---

## 📂 Estrutura do Repositório

```
📁 compilador/
├── 📜 README.md  # Documentação do projeto
├── 📜 lexer.py  # Implementação da análise léxica
├── 📜 parser.py  # Implementação da análise sintática
├── 📜 tokens.txt  # Lista de tokens reconhecidos
├── 📜 exemplos/  # Códigos de exemplo para testes
└── 📜 docs/  # Documentação detalhada
```

---

## 🚀 Como Executar

### **1️⃣ Clone o repositório**

```bash
git clone https://github.com/seu-usuario/seu-repositorio.git
cd compilador
```

### **2️⃣ Execute o analisador léxico e sintático**

```bash
python lexer.py arquivo_de_entrada.txt
python parser.py arquivo_de_entrada.txt
```

### **3️⃣ Veja os resultados**

- Erros identificados serão exibidos com **linha e descrição**.
- O código será analisado e validado conforme a gramática definida.

---

## 📌 Contribuição

Sugestões e melhorias são bem-vindas! Para contribuir:

1. **Fork** este repositório.
2. Crie um **branch** com sua funcionalidade: `git checkout -b minha-mudanca`
3. Faça **commit** das alterações: `git commit -m 'Minha mudança'`
4. Faça **push** para o branch: `git push origin minha-mudanca`
5. Abra um **Pull Request** 🚀

---

## 📜 Licença

Este projeto é distribuído sob a licença **MIT**.

---

🚀 **Vamos compilar e analisar!** Se tiver dúvidas, entre em contato! 😊

