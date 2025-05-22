using Compilador.Analises;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Compilador
{
    public partial class Form1 : Form
    {
        private string currentFilePath = string.Empty;

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
            richTextBoxErro.ReadOnly = true;
            richTextBoxErro.DoubleClick += RichTextBoxErro_DoubleClick;
            this.OnResize(EventArgs.Empty);
        }

        #region Arquivo Operations
        private void novoToolStripButton_Click(object sender, EventArgs e) { ClearEditor(); }
        private void abrirToolStripButton_Click(object sender, EventArgs e) { if (openFileDialog1.ShowDialog() == DialogResult.OK) { OpenFile(openFileDialog1.FileName); } }
        private void salvarToolStripButton_Click(object sender, EventArgs e) { SaveFile(); }
        private void ClearEditor() { richTextBox1.Clear(); richTextBoxErro.Clear(); currentFilePath = string.Empty; lbNomeProjeto.Text = "Novo Projeto"; richTextBox1.Focus(); }
        private void OpenFile(string filePath) { try { richTextBox1.Text = File.ReadAllText(filePath); currentFilePath = filePath; lbNomeProjeto.Text = Path.GetFileName(filePath); richTextBoxErro.Clear(); ResetEditorHighlight(); } catch (Exception ex) { ShowError("Erro ao abrir arquivo", ex.Message); } }
        private void SaveFile()
        {
            try
            {
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        currentFilePath = saveFileDialog1.FileName;
                    }
                    else
                    {
                        currentFilePath = string.Empty;
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    File.WriteAllText(currentFilePath, richTextBox1.Text);
                    lbNomeProjeto.Text = Path.GetFileName(currentFilePath);
                }
            }
            catch (Exception ex)
            {
                ShowError("Erro ao salvar arquivo", ex.Message);
                currentFilePath = string.Empty;
            }
        }
        #endregion

        #region Text Formatting
        private void ToggleFontStyle(FontStyle style) { var currentFont = richTextBox1.SelectionFont ?? richTextBox1.Font; var newStyle = currentFont.Style ^ style; richTextBox1.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle); }
        private void btnNegrito_Click(object sender, EventArgs e) { ToggleFontStyle(FontStyle.Bold); }
        private void btnItalico_Click(object sender, EventArgs e) { ToggleFontStyle(FontStyle.Italic); }
        private void btnSublinhar_Click(object sender, EventArgs e) { ToggleFontStyle(FontStyle.Underline); }
        private void btnLimparTudo_Click(object sender, EventArgs e) { ClearEditor(); ResetEditorHighlight(); }
        #endregion

        #region Compiler
        private void btnCompilar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                SaveFile();
            }
            else
            {
                SaveFile();
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    MessageBox.Show("A compilação requer que o arquivo seja salvo primeiro.", "Arquivo não salvo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            richTextBoxErro.Clear();
            ResetEditorHighlight();

            bool hasLexicalErrors = false;
            bool hasSyntaxErrors = false;
            bool hasSemanticaxErrors = false;
            List<Token> listaTokens = new List<Token>();

            string reportPath = Path.Combine(
                Path.GetDirectoryName(currentFilePath),
                $"{Path.GetFileNameWithoutExtension(currentFilePath)}_relatorioAnaliseLexica.txt");

            try
            {
                using (var fileStream = new FileStream(currentFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var analiseLexica = new Analise_Lexica(fileStream);
                    var relatorioLexico = analiseLexica.AnalisadorLexico();

                    try
                    {
                        File.WriteAllLines(reportPath, relatorioLexico.Select(r => $"Linha {r.LineNumber}: {r.TokenDescription}"));
                    }
                    catch (Exception ex)
                    {
                        ShowError("Erro ao Gerar Relatório", $"Não foi possível gerar o arquivo de relatório léxico: {ex.Message}");
                    }

                    richTextBoxErro.SelectionColor = Color.Red;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    bool firstLexError = true;

                    foreach (var lex in relatorioLexico)
                    {
                        string description = lex.TokenDescription.Trim();
                        string lexeme = "[ERRO]";
                        string tokenType = "ERRO_LEXICO";

                        if (description.StartsWith("ERRO"))
                        {
                            var match = Regex.Match(description, @"\'([^\']+)\'");
                            lexeme = match.Success ? match.Groups[1].Value : "[?] ";
                        }
                        else
                        {
                            int indexEh = description.IndexOf(" eh ");
                            if (indexEh > 0)
                            {
                                lexeme = description.Substring(0, indexEh).Trim();
                                tokenType = description.Substring(indexEh + 4).Trim();
                            }
                            else
                            {
                                lexeme = description;
                                tokenType = "DESCONHECIDO";
                                System.Diagnostics.Debug.WriteLine($"AVISO: Formato inesperado do lexer na linha {lex.LineNumber}: '{description}'");
                            }
                        }

                        if (tokenType.Contains("ERRO") || tokenType == "ERRO_LEXICO" || tokenType == "DESCONHECIDO")
                        {
                            if (firstLexError)
                            {
                                richTextBoxErro.AppendText("❌ Erros Léxicos Encontrados:\n");
                                firstLexError = false;
                            }
                            richTextBoxErro.AppendText($"  Linha {lex.LineNumber}: {description}\n");
                            hasLexicalErrors = true;
                            HighlightErrorLine(lex.LineNumber, Color.FromArgb(90, 255, 85, 85));
                        }
                        else if (tokenType != "VAZIO")
                        {
                            listaTokens.Add(new Token(lexeme, tokenType, lex.LineNumber));
                        }
                    }

                    int lastLineEof = listaTokens.Any() ? listaTokens.Last().LineNumber : (richTextBox1.Lines.Length > 0 ? richTextBox1.Lines.Length : 1);
                    listaTokens.Add(new Token("$", "EOF", lastLineEof));

                    if (!hasLexicalErrors)
                    {
                        richTextBoxErro.SelectionColor = Color.Green;
                        richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                        richTextBoxErro.AppendText("✓ Análise Léxica concluída com sucesso!\n");
                    }
                    if (File.Exists(reportPath))
                    {
                        richTextBoxErro.AppendText($"* Relatório de Análise Léxica Gerado em: {reportPath}\n");
                    }
                    richTextBoxErro.AppendText("------------------------------------------------------\n");

                } 
                var analiseSintatica = new AnaliseSintatica(listaTokens);
                analiseSintatica.Parse();

                if (analiseSintatica.Erros.Any())
                {
                    hasSyntaxErrors = true;
                    richTextBoxErro.SelectionColor = Color.Orange;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    richTextBoxErro.AppendText("❌ Erros Sintáticos Encontrados:\n");

                    foreach (var erroSintatico in analiseSintatica.Erros)
                    {
                        richTextBoxErro.AppendText($"  {erroSintatico}\n");
                        Match match = Regex.Match(erroSintatico, @"(?:Linha|na Linha)\s+(\d+):");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNo))
                        {
                            HighlightErrorLine(lineNo, Color.FromArgb(90, 255, 165, 0));
                        }
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

                var analiseSemantica = new Analise_Semantica(listaTokens);
                analiseSemantica.Analisar();

                if (analiseSemantica.Erros.Any())
                {
                    hasSemanticaxErrors = true; // Mantido
                    richTextBoxErro.SelectionColor = Color.Purple; // Mantido
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold); // Mantido
                    richTextBoxErro.AppendText("❌ Erros Semânticos Encontrados:\n"); // Mantido
                    foreach (var erroSemantico in analiseSemantica.Erros)
                    {
                        richTextBoxErro.AppendText($"  {erroSemantico}\n"); // Mantido

                        // --- Adicionar esta parte ---
                        // Extrai o número da linha da mensagem de erro semântico
                        Match match = Regex.Match(erroSemantico, @"(?:linha|Linha)\s+(\d+):");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNo))
                        {
                            // Chama a função para destacar a linha no editor
                            HighlightErrorLine(lineNo, Color.FromArgb(90, 128, 0, 128)); // Cor Roxo Semi-Transparente
                        }
                        // --- Fim da adição ---
                    }
                    richTextBoxErro.AppendText("------------------------------------------------------\n"); // Mantido
                }
                else
                {
                    richTextBoxErro.SelectionColor = Color.Blue;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    richTextBoxErro.AppendText("✓ Análise Semântica concluída com sucesso!\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");
                }

                if (!hasLexicalErrors && !hasSyntaxErrors && !hasSemanticaxErrors)
                {
                    richTextBoxErro.SelectionColor = Color.DodgerBlue;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Italic);
                    richTextBoxErro.AppendText("ℹ️ Compilação bem-sucedida.\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");
                    // --- Geração de Código Intermediário ---
                    var gerador = new GeradorCodigoIntermediario(listaTokens);
                    List<string> codigoIntermediario = gerador.Gerar();

                    // Gera e salva o relatório de código intermediário
                    RelatorioCodigoIntermediario.Gerar(codigoIntermediario, currentFilePath);

                    // Exibe mensagem na tela
                    richTextBoxErro.SelectionColor = Color.DarkCyan;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    richTextBoxErro.AppendText("✓ Código Intermediário gerado com sucesso!\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");
                    // --- Otimização de Código ---
                    List<string> codigoOtimizado = OtimizadorCodigo.Otimizar(codigoIntermediario);

                    // Gera relatório otimizado
                    RelatorioCodigoOtimizado.Gerar(codigoOtimizado, currentFilePath);

                    // Mensagem no painel de saída
                    richTextBoxErro.SelectionColor = Color.Teal;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    richTextBoxErro.AppendText("✓ Código Otimizado gerado com sucesso!\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");

                    var geradorSimpSIM = new GeradorCodigoSimpSIM(codigoOtimizado); // 'codigoOtimizado' é a List<string> da otimização
                    List<string> codigoFinalSimpSIM = geradorSimpSIM.Gerar();

                    // Salvar ou exibir o código SimpSIM
                    string caminhoArquivoSimpSIM = Path.Combine(
                        Path.GetDirectoryName(currentFilePath),
                        Path.GetFileNameWithoutExtension(currentFilePath) + "_codigoSimpSIM.txt" // ou .txt
                    );
                    File.WriteAllLines(caminhoArquivoSimpSIM, codigoFinalSimpSIM);

                    richTextBoxErro.SelectionColor = Color.DarkGreen; // Uma nova cor para o sucesso da geração final
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    richTextBoxErro.AppendText("✓ Código de Máquina SimpSIM gerado com sucesso!\n");
                    richTextBoxErro.AppendText($"* Salvo em: {caminhoArquivoSimpSIM}\n");
                    richTextBoxErro.AppendText("------------------------------------------------------\n");
                }
                else
                {

                    richTextBoxErro.SelectionColor = Color.DarkRed;
                    richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                    richTextBoxErro.AppendText("⚠️ Compilação concluída com erros.\n");
                }

            }
            catch (IOException ioEx)
            {
                ShowError("Erro de Arquivo", $"Não foi possível ler/escrever o arquivo '{currentFilePath}'. Verifique as permissões e se o arquivo não está aberto em outro programa.\nDetalhes: {ioEx.Message}");
                richTextBoxErro.SelectionColor = Color.Magenta;
                richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                richTextBoxErro.AppendText($"💥 Erro de Arquivo: {ioEx.Message}\n");
                hasLexicalErrors = true; 
            }
            catch (Exception ex)
            {
                ShowError("Erro Inesperado na Compilação", ex.Message + (ex.InnerException != null ? "\nInner Exception: " + ex.InnerException.Message : "") + "\n\nStackTrace:\n" + ex.StackTrace);
                richTextBoxErro.SelectionColor = Color.Magenta;
                richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                richTextBoxErro.AppendText($"💥 Erro Fatal Inesperado: {ex.Message}\n");
                
                hasLexicalErrors = true;
                hasSyntaxErrors = true;
            }
            finally
            {
                richTextBoxErro.ScrollToCaret();
                richTextBox1.Focus();
                richTextBox1.DeselectAll();
            }
        }

        private string ExtractErrorLexeme(string errorMessage)
        {
            var match = Regex.Match(errorMessage, @"\'([^\']+)\'");
            return match.Success ? match.Groups[1].Value : "[?] ";
        }

        private void HighlightErrorLine(int lineNumber, Color highlightColor)
        {
            int zeroBasedLineNumber = lineNumber - 1;
            if (zeroBasedLineNumber >= 0 && zeroBasedLineNumber < richTextBox1.Lines.Length)
            {
                int startIndex = richTextBox1.GetFirstCharIndexFromLine(zeroBasedLineNumber);
                int lineLength = richTextBox1.Lines[zeroBasedLineNumber].Length;
                if (startIndex >= 0)
                {
                    richTextBox1.Select(startIndex, lineLength);
                    richTextBox1.SelectionBackColor = highlightColor;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Tentativa de realçar linha inválida: {lineNumber}");
            }
        }

        private void RichTextBoxErro_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                int charIndex = richTextBoxErro.SelectionStart;
                if (charIndex < 0 || charIndex >= richTextBoxErro.TextLength) return;
                int lineIndex = richTextBoxErro.GetLineFromCharIndex(charIndex);
                if (lineIndex < 0 || lineIndex >= richTextBoxErro.Lines.Length) return;
                string lineText = richTextBoxErro.Lines[lineIndex];
                var match = Regex.Match(lineText, @"(?:Linha|na Linha)\s+(\d+):");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNumber))
                {
                    GoToEditorLine(lineNumber - 1);
                }
            }
            catch (Exception ex)
            {
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

        private void ResetEditorHighlight()
        {
            int selectionStart = richTextBox1.SelectionStart;
            int selectionLength = richTextBox1.SelectionLength;
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = richTextBox1.BackColor;
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            richTextBox1.DeselectAll();
            richTextBox1.Select(selectionStart, selectionLength);
        }
        #endregion

        #region Helpers
        private void ShowError(string title, string message)
        {
            MessageBox.Show($"Erro: {message}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
}