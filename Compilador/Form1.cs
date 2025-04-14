// Form1.cs (Modificado)
using Compilador.Analises;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // Keep this if needed for other parts
using System.Windows.Forms;

namespace Compilador
{
    public partial class Form1 : Form
    {
        private string currentFilePath = string.Empty;

        // Existing Form1 constructor and other methods...
        public Form1()
        {
            InitializeComponent();
            openFileDialog1 = new OpenFileDialog
            {
                Title = "Abrir arquivo",
                Filter = "Arquivos de texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*"
            };
            saveFileDialog1 = new SaveFileDialog
            {
                Filter = "Arquivos de texto (*.txt)|*.txt",
                DefaultExt = "txt"
            };
            // Setup RichTextBoxErro - Keep your styling
            richTextBoxErro.ReadOnly = true;
            richTextBoxErro.DoubleClick += RichTextBoxErro_DoubleClick; // Keep if you use this
            this.OnResize(EventArgs.Empty); // Keep if needed
        }

        #region Arquivo Operations
        // --- Keep your File Operations methods (novo, abrir, salvar, ClearEditor, OpenFile, SaveFile) ---
        private void novoToolStripButton_Click(object sender, EventArgs e) { ClearEditor(); }
        private void abrirToolStripButton_Click(object sender, EventArgs e) { if (openFileDialog1.ShowDialog() == DialogResult.OK) { OpenFile(openFileDialog1.FileName); } }
        private void salvarToolStripButton_Click(object sender, EventArgs e) { SaveFile(); }
        private void ClearEditor() { richTextBox1.Clear(); richTextBoxErro.Clear(); currentFilePath = string.Empty; lbNomeProjeto.Text = "Novo Projeto"; richTextBox1.Focus(); }
        private void OpenFile(string filePath) { try { richTextBox1.Text = File.ReadAllText(filePath); currentFilePath = filePath; lbNomeProjeto.Text = Path.GetFileName(filePath); richTextBoxErro.Clear(); } catch (Exception ex) { ShowError("Erro ao abrir arquivo", ex.Message); } }
        private void SaveFile() { try { if (string.IsNullOrEmpty(currentFilePath)) { if (saveFileDialog1.ShowDialog() == DialogResult.OK) { currentFilePath = saveFileDialog1.FileName; } else { return; } } File.WriteAllText(currentFilePath, richTextBox1.Text); lbNomeProjeto.Text = Path.GetFileName(currentFilePath); } catch (Exception ex) { ShowError("Erro ao salvar arquivo", ex.Message); } }
        #endregion

        #region Text Formatting
        // --- Keep your Text Formatting methods (ToggleFontStyle, btnNegrito_Click, etc.) ---
        private void ToggleFontStyle(FontStyle style) { var currentFont = richTextBox1.SelectionFont ?? richTextBox1.Font; var newStyle = currentFont.Style ^ style; richTextBox1.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle); }
        private void btnNegrito_Click(object sender, EventArgs e) { ToggleFontStyle(FontStyle.Bold); }
        private void btnItalico_Click(object sender, EventArgs e) { ToggleFontStyle(FontStyle.Italic); } // Assuming you might have this
        private void btnSublinhar_Click(object sender, EventArgs e) { ToggleFontStyle(FontStyle.Underline); }
        private void btnLimparTudo_Click(object sender, EventArgs e) { ClearEditor(); } // Assuming you might have this
        #endregion

        #region Compiler
        private void btnCompilar_Click(object sender, EventArgs e)
        {
            // Save the file first (same logic as before)
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                SaveFile();
            }
            else
            {
                SaveFile();
                if (string.IsNullOrEmpty(currentFilePath))
                    return; // Exit if the user cancelled saving
            }

            // --- Compilation Process ---
            richTextBoxErro.Clear();
            ResetEditorHighlight(); // Keep this if you implement highlighting

            bool hasLexicalErrors = false;
            bool hasSyntaxErrors = false;
            List<Token> listaTokens = new List<Token>(); // Initialize token list

            try
            {
                // === Lexical Analysis ===
                using (var fileStream = new FileStream(currentFilePath, FileMode.Open, FileAccess.Read))
                {
                    var analiseLexica = new Analise_Lexica(fileStream); // Use your existing Lexical Analyzer
                    var relatorioLexico = analiseLexica.AnalisadorLexico();

                    // Generate Lexical Report File (optional)
                    string reportPath = Path.Combine(
                        Path.GetDirectoryName(currentFilePath),
                        $"{Path.GetFileNameWithoutExtension(currentFilePath)}_relatorioAnaliseLexica.txt");
                    File.WriteAllLines(reportPath, relatorioLexico.Select(r => $"Linha {r.LineNumber}: {r.TokenDescription}")); // Include line number in report

                    // Process Lexical Errors and Build Token List
                    richTextBoxErro.SelectionColor = Color.Red;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    bool firstLexError = true;

                    foreach (var lex in relatorioLexico)
                    {
                        string description = lex.TokenDescription.Trim();
                        string lexeme = "[ERRO]"; // Default
                        string tokenType = "ERRO_LEXICO"; // Default

                        if (description.StartsWith("ERRO"))
                        {
                            // Tentativa de extrair o lexema da mensagem de erro para exibição
                            var match = Regex.Match(description, @"\'([^\']+)\'");
                            lexeme = match.Success ? match.Groups[1].Value : "[?] ";
                            // tokenType permanece "ERRO_LEXICO"
                        }
                        else
                        {
                            int indexEh = description.IndexOf(" eh ");
                            if (indexEh > 0)
                            {
                                // Formato esperado "lexeme eh tipo"
                                lexeme = description.Substring(0, indexEh).Trim();
                                tokenType = description.Substring(indexEh + 4).Trim();
                            }
                            else
                            {
                                // Se o formato não veio como esperado do Lexer (apesar da correção acima),
                                // tratamos como um erro de formato do lexer ou token desconhecido.
                                lexeme = description; // Usa a descrição inteira como lexema?
                                tokenType = "DESCONHECIDO";
                                // Seria bom logar um aviso aqui indicando que o formato do lexer falhou
                                System.Diagnostics.Debug.WriteLine($"AVISO: Formato inesperado do lexer na linha {lex.LineNumber}: '{description}'");
                            }
                        }

                        // Lógica de tratamento de erro/adição à lista (igual à anterior)
                        if (tokenType.Contains("ERRO") || tokenType == "ERRO_LEXICO" || tokenType == "DESCONHECIDO")
                        {
                            if (firstLexError && !tokenType.Contains("ERRO"))
                            {
                                richTextBoxErro.AppendText("❌ Erros Léxicos Encontrados:\n");
                                firstLexError = false;
                            }
                            richTextBoxErro.AppendText($"  Linha {lex.LineNumber}: {description}\n"); // Mostra a descrição original do lexer
                            hasLexicalErrors = true;
                        }
                        else if (tokenType != "VAZIO") // Ignora tokens vazios se ocorrerem
                        {
                            listaTokens.Add(new Token(lexeme, tokenType, lex.LineNumber)); // Adiciona token válido
                        }
                    }

                    // Adiciona EOF (sem alterações)
                    int lastLine = listaTokens.Any() ? listaTokens.Last().LineNumber : 1;
                    listaTokens.Add(new Token("$", "EOF", lastLine));


                    if (!hasLexicalErrors)
                    {
                        richTextBoxErro.SelectionColor = Color.Green;
                        richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                        richTextBoxErro.AppendText("✓ Análise Léxica concluída com sucesso!\n");
                    }
                    richTextBoxErro.AppendText($"* Relatório de Análise Léxica Gerado em: {reportPath}\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");
                } // End of using fileStream


                // === Syntactic Analysis ===
                if (!hasLexicalErrors)
                {
                    // Add the mandatory EOF token [cite: 75]
                    int lastLine = listaTokens.Any() ? listaTokens.Last().LineNumber : 1;
                    listaTokens.Add(new Token("$", "EOF", lastLine)); // Use $ or similar for EOF lexeme

                    var analiseSintatica = new AnaliseSintatica(listaTokens);
                    analiseSintatica.Parse(); // Start parsing from the top rule

                    if (analiseSintatica.Erros.Any())
                    {
                        hasSyntaxErrors = true;
                        richTextBoxErro.SelectionColor = Color.Orange; // Use a different color for syntax errors
                        richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                        richTextBoxErro.AppendText("❌ Erros Sintáticos Encontrados:\n");
                        foreach (var erroSintatico in analiseSintatica.Erros)
                        {
                            richTextBoxErro.AppendText($"  {erroSintatico}\n");
                            // You could try to parse the line number from erroSintatico for highlighting
                            // Match match = Regex.Match(erroSintatico, @"Linha (\d+):");
                            // if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNo))
                            // {
                            //     HighlightErrorLine(lineNo - 1); // Adjust to 0-based index
                            // }
                        }
                        richTextBoxErro.AppendText("------------------------------------------------------\n");
                    }
                    else
                    {
                        richTextBoxErro.SelectionColor = Color.Green;
                        richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                        richTextBoxErro.AppendText("✓ Análise Sintática concluída com sucesso!\n");
                        richTextBoxErro.AppendText("------------------------------------------------------\n");
                    }
                }
                else
                {
                    richTextBoxErro.SelectionColor = Color.Red;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold | FontStyle.Italic);
                    richTextBoxErro.AppendText("ℹ️ Análise Sintática não executada devido a erros léxicos.\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");
                }


                // === Semantic Analysis & Code Generation (Future Steps) ===
                if (!hasLexicalErrors && !hasSyntaxErrors)
                {
                    // Placeholder for future steps
                    richTextBoxErro.SelectionColor = Color.Blue;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Italic);
                    richTextBoxErro.AppendText("ℹ️ Compilação (Análise Léxica e Sintática) bem-sucedida. Próximas etapas: Análise Semântica e Geração de Código.\n");
                }


            }
            catch (IOException ioEx) // Catch file access errors specifically
            {
                ShowError("Erro de Arquivo", $"Não foi possível ler o arquivo '{currentFilePath}'. Verifique as permissões e se o arquivo não está aberto em outro programa.\nDetalhes: {ioEx.Message}");
                // Indicate error state
                hasLexicalErrors = true; // Prevent further analysis stages
            }
            catch (Exception ex)
            {
                // Catch unexpected errors during compilation
                ShowError("Erro Inesperado na Compilação", ex.Message + (ex.InnerException != null ? "\nInner Exception: " + ex.InnerException.Message : ""));
                richTextBoxErro.SelectionColor = Color.Magenta;
                richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                richTextBoxErro.AppendText($"💥 Erro Fatal: {ex.Message}\n");
                // Indicate error state
                hasLexicalErrors = true; // Prevent further analysis stages
            }
            finally
            {
                // Ensure caret is visible and editor is focused
                richTextBoxErro.ScrollToCaret();
                richTextBox1.Focus();
            }
        }

        // Helper to extract the lexeme from an error message if possible
        private string ExtractErrorLexeme(string errorMessage)
        {
            // Simple extraction based on typical error format like "ERRO : 'lexeme' ..."
            var match = Regex.Match(errorMessage, @"\'([^\']+)\'");
            return match.Success ? match.Groups[1].Value : "[?] ";
        }
        private void RichTextBoxErro_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                int charIndex = richTextBoxErro.SelectionStart;
                if (charIndex < 0 || charIndex >= richTextBoxErro.TextLength) return; // Basic bounds check

                int lineIndex = richTextBoxErro.GetLineFromCharIndex(charIndex);
                if (lineIndex < 0 || lineIndex >= richTextBoxErro.Lines.Length) return; // Basic bounds check

                string lineText = richTextBoxErro.Lines[lineIndex];

                // Match format "Linha X:" (Lexical) or "Erro Sintático na Linha X:" (Syntax)
                var match = Regex.Match(lineText, @"(?:Linha|na Linha)\s+(\d+):");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNumber))
                {
                    GoToEditorLine(lineNumber - 1); // Adjust to 0-based index for RichTextBox
                }
            }
            catch (Exception ex)
            {
                // Log or ignore minor errors in double-click handling
                System.Diagnostics.Debug.WriteLine($"Error in RichTextBoxErro_DoubleClick: {ex.Message}");
            }
        }

        private void GoToEditorLine(int lineNumber)
        {
            if (lineNumber >= 0 && lineNumber < richTextBox1.Lines.Length)
            {
                richTextBox1.Focus();
                int start = richTextBox1.GetFirstCharIndexFromLine(lineNumber);
                if (start >= 0) 
                {
                    richTextBox1.Select(start, 0);
                    richTextBox1.ScrollToCaret();
                }
            }
        }
        private void ResetEditorHighlight() { richTextBox1.SelectAll(); richTextBox1.SelectionBackColor = richTextBox1.BackColor; richTextBox1.SelectionColor = richTextBox1.ForeColor; richTextBox1.DeselectAll(); richTextBox1.Focus(); } // Added focus back


        #endregion

        #region Helpers

        private void ShowError(string title, string message)
        {
            MessageBox.Show($"Erro: {message}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion



    }
}