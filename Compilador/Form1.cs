using Compilador.Analises;
using System;
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
            richTextBoxErro.ForeColor = Color.White;
            richTextBoxErro.ReadOnly = true;
            richTextBoxErro.DoubleClick += RichTextBoxErro_DoubleClick;
            this.OnResize(EventArgs.Empty);
        }

        #region Arquivo Operations
        private void novoToolStripButton_Click(object sender, EventArgs e)
        {
            ClearEditor();
        }

        private void abrirToolStripButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openFileDialog1.FileName);
            }
        }

        private void salvarToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void ClearEditor()
        {
            richTextBox1.Clear();
            richTextBoxErro.Clear();
            currentFilePath = string.Empty;
            lbNomeProjeto.Text = "Novo Projeto";
            richTextBox1.Focus();
        }

        private void OpenFile(string filePath)
        {
            try
            {
                richTextBox1.Text = File.ReadAllText(filePath);
                currentFilePath = filePath;
                lbNomeProjeto.Text = Path.GetFileName(filePath);
                richTextBoxErro.Clear();
            }
            catch (Exception ex)
            {
                ShowError("Erro ao abrir arquivo", ex.Message);
            }
        }

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
                        return;
                    }
                }

                File.WriteAllText(currentFilePath, richTextBox1.Text);
                lbNomeProjeto.Text = Path.GetFileName(currentFilePath);
            }
            catch (Exception ex)
            {
                ShowError("Erro ao salvar arquivo", ex.Message);
            }
        }
        #endregion

        #region Text Formatting
        private void ToggleFontStyle(FontStyle style)
        {
            var currentFont = richTextBox1.SelectionFont ?? richTextBox1.Font;
            var newStyle = currentFont.Style ^ style;
            richTextBox1.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
        }

        private void btnNegrito_Click(object sender, EventArgs e)
        {
            ToggleFontStyle(FontStyle.Bold);
        }

        private void btnSublinhar_Click(object sender, EventArgs e)
        {
            ToggleFontStyle(FontStyle.Underline);
        }

        private void btnLimparTudo_Click(object sender, EventArgs e)
        {
            ClearEditor();
        }
        #endregion

        #region Compiler
        private void btnCompilar_Click(object sender, EventArgs e)
        {
            // Salva o arquivo se já estiver associado a um caminho
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                SaveFile();
            }
            // Se não houver caminho (novo arquivo), solicita salvar
            else if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFile();
                if (string.IsNullOrEmpty(currentFilePath)) return; // Sai se o usuário cancelar a salvar
            }

            try
            {
                richTextBoxErro.Clear();
                ResetEditorHighlight();

                using (var fileStream = new FileStream(currentFilePath, FileMode.Open, FileAccess.Read))
                {
                    var analiseLexica = new Analise_Lexica(fileStream);
                    var relatorioLexico = analiseLexica.AnalisadorLexico();

                    string reportPath = Path.Combine(
                        Path.GetDirectoryName(currentFilePath),
                        $"{Path.GetFileNameWithoutExtension(currentFilePath)}_relatorioAnaliseLexica.txt");

                    // Salva o relatório como texto puro
                    File.WriteAllLines(reportPath, relatorioLexico.Select(r => r.TokenDescription));

                    var errors = relatorioLexico
                        .Where(r => r.TokenDescription.Contains("ERRO"))
                        .ToList();

                    if (errors.Any())
                    {
                        richTextBoxErro.SelectionColor = Color.Red;
                        richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                        richTextBoxErro.AppendText("❌ ERRO ao Compilar!!!\n");
                        richTextBoxErro.SelectionFont = richTextBoxErro.Font;

                        foreach (var error in errors)
                        {
                            int errorLine = error.LineNumber - 1;
                            string errorMessage = error.TokenDescription;
                            string lineContent = errorLine >= 0 && errorLine < richTextBox1.Lines.Length
                                ? richTextBox1.Lines[errorLine].Trim()
                                : "Não disponível";

                            richTextBoxErro.SelectionColor = Color.Red;
                            richTextBoxErro.AppendText($"  Linha {errorLine + 1}: ");
                            richTextBoxErro.SelectionColor = Color.White;
                            richTextBoxErro.AppendText($"{errorMessage}\n");
                            richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Italic);
                            richTextBoxErro.AppendText($"    Trecho: \"{lineContent}\"\n");
                            richTextBoxErro.SelectionFont = richTextBoxErro.Font;
                            richTextBoxErro.AppendText("  -------------------\n");

                            if (errorLine >= 0 && errorLine < richTextBox1.Lines.Length)
                            {
                                HighlightErrorLine(errorLine);
                            }
                        }

                        richTextBoxErro.SelectionColor = Color.Cyan;
                        richTextBoxErro.AppendText($"* Relatório de Análise Léxica Gerado em: {reportPath}\n");
                    }
                    else
                    {
                        richTextBoxErro.SelectionColor = Color.Green;
                        richTextBoxErro.SelectionFont = new Font(richTextBoxErro.Font, FontStyle.Bold);
                        richTextBoxErro.AppendText("✓ Compilado com Sucesso!!!\n");
                        richTextBoxErro.SelectionFont = richTextBoxErro.Font;
                        richTextBoxErro.SelectionColor = Color.Cyan;
                        richTextBoxErro.AppendText($"* Relatório de Análise Léxica Gerado em: {reportPath}\n");

                        // Exibe os tokens reconhecidos
                        foreach (var token in relatorioLexico)
                        {
                            richTextBoxErro.AppendText($"{token.TokenDescription}\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erro ao compilar", ex.Message);
            }
        }

        private void HighlightErrorLine(int lineNumber)
        {
            int start = richTextBox1.GetFirstCharIndexFromLine(lineNumber);
            int length = richTextBox1.Lines[lineNumber].Length;

            richTextBox1.Select(start, length);
            richTextBox1.SelectionBackColor = Color.LightPink;

            string errorToken = ExtractErrorToken(richTextBox1.Lines[lineNumber]);
            if (!string.IsNullOrEmpty(errorToken))
            {
                int tokenStart = richTextBox1.Lines[lineNumber].IndexOf(errorToken);
                if (tokenStart >= 0)
                {
                    richTextBox1.Select(start + tokenStart, errorToken.Length);
                    richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Underline);
                    richTextBox1.SelectionColor = Color.Red;
                }
            }

            richTextBox1.DeselectAll();
        }

        private string ExtractErrorToken(string line)
        {
            var match = Regex.Match(line, @"'([^']*)'");
            return match.Success ? match.Groups[1].Value : null;
        }

        private void ResetEditorHighlight()
        {
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = richTextBox1.BackColor;
            richTextBox1.SelectionFont = richTextBox1.Font;
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            richTextBox1.DeselectAll();
        }

        private void RichTextBoxErro_DoubleClick(object sender, EventArgs e)
        {
            int charIndex = richTextBoxErro.SelectionStart;
            int lineIndex = richTextBoxErro.GetLineFromCharIndex(charIndex);

            string lineText = richTextBoxErro.Lines[lineIndex];
            var match = Regex.Match(lineText, @"Linha (\d+):");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNumber))
            {
                GoToEditorLine(lineNumber - 1);
            }
        }

        private void GoToEditorLine(int lineNumber)
        {
            if (lineNumber >= 0 && lineNumber < richTextBox1.Lines.Length)
            {
                int start = richTextBox1.GetFirstCharIndexFromLine(lineNumber);
                richTextBox1.Select(start, 0);
                richTextBox1.ScrollToCaret();
                richTextBox1.Focus();
            }
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