using System.Drawing;
using System.Windows.Forms;

namespace Compilador
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.lineNumbersForRichText1 = new LineNumbersControlForRichTextBox.LineNumbersForRichText();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.novoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.abrirToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.salvarToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.btnCompilar = new System.Windows.Forms.ToolStripButton();
            this.btnLimparTudo = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.richTextBoxErro = new System.Windows.Forms.RichTextBox();
            this.lbNomeProjeto = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(54)))));
            this.richTextBox1.Font = new System.Drawing.Font("Consolas", 12F);
            this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
            this.richTextBox1.Location = new System.Drawing.Point(40, 28);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(480, 521);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // lineNumbersForRichText1
            // 
            this.lineNumbersForRichText1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lineNumbersForRichText1.AutoSizing = true;
            this.lineNumbersForRichText1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(54)))));
            this.lineNumbersForRichText1.BackgroundGradientAlphaColor = System.Drawing.Color.Transparent;
            this.lineNumbersForRichText1.BackgroundGradientBetaColor = System.Drawing.Color.LightSteelBlue;
            this.lineNumbersForRichText1.BackgroundGradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            this.lineNumbersForRichText1.BorderLinesColor = System.Drawing.Color.SlateGray;
            this.lineNumbersForRichText1.BorderLinesStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            this.lineNumbersForRichText1.BorderLinesThickness = 1F;
            this.lineNumbersForRichText1.DockSide = LineNumbersControlForRichTextBox.LineNumbersForRichText.LineNumberDockSide.Left;
            this.lineNumbersForRichText1.ForeColor = System.Drawing.Color.White;
            this.lineNumbersForRichText1.GridLinesColor = System.Drawing.Color.SlateGray;
            this.lineNumbersForRichText1.GridLinesStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            this.lineNumbersForRichText1.GridLinesThickness = 1F;
            this.lineNumbersForRichText1.LineNumbersAlignment = System.Drawing.ContentAlignment.TopRight;
            this.lineNumbersForRichText1.LineNumbersAntiAlias = true;
            this.lineNumbersForRichText1.LineNumbersAsHexadecimal = false;
            this.lineNumbersForRichText1.LineNumbersClippedByItemRectangle = true;
            this.lineNumbersForRichText1.LineNumbersLeadingZeroes = true;
            this.lineNumbersForRichText1.LineNumbersOffset = new System.Drawing.Size(0, 0);
            this.lineNumbersForRichText1.Location = new System.Drawing.Point(19, 28);
            this.lineNumbersForRichText1.Margin = new System.Windows.Forms.Padding(0);
            this.lineNumbersForRichText1.MarginLinesColor = System.Drawing.Color.SlateGray;
            this.lineNumbersForRichText1.MarginLinesSide = LineNumbersControlForRichTextBox.LineNumbersForRichText.LineNumberDockSide.Right;
            this.lineNumbersForRichText1.MarginLinesStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            this.lineNumbersForRichText1.MarginLinesThickness = 1F;
            this.lineNumbersForRichText1.Name = "lineNumbersForRichText1";
            this.lineNumbersForRichText1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.lineNumbersForRichText1.ParentRichTextBox = this.richTextBox1;
            this.lineNumbersForRichText1.SeeThroughMode = false;
            this.lineNumbersForRichText1.ShowBackgroundGradient = true;
            this.lineNumbersForRichText1.ShowBorderLines = true;
            this.lineNumbersForRichText1.ShowGridLines = true;
            this.lineNumbersForRichText1.ShowLineNumbers = true;
            this.lineNumbersForRichText1.ShowMarginLines = true;
            this.lineNumbersForRichText1.Size = new System.Drawing.Size(20, 521);
            this.lineNumbersForRichText1.TabIndex = 3;
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(71)))), ((int)(((byte)(90)))));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Right;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.novoToolStripButton,
            this.abrirToolStripButton,
            this.salvarToolStripButton,
            this.toolStripSeparator,
            this.btnCompilar,
            this.btnLimparTudo});
            this.toolStrip1.Location = new System.Drawing.Point(776, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(24, 561);
            this.toolStrip1.TabIndex = 2;
            // 
            // novoToolStripButton
            // 
            this.novoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.novoToolStripButton.Image = global::Compilador.Properties.Resources.file_new;
            this.novoToolStripButton.Name = "novoToolStripButton";
            this.novoToolStripButton.Size = new System.Drawing.Size(21, 20);
            this.novoToolStripButton.Click += new System.EventHandler(this.novoToolStripButton_Click);
            // 
            // abrirToolStripButton
            // 
            this.abrirToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.abrirToolStripButton.Image = global::Compilador.Properties.Resources.open_folder;
            this.abrirToolStripButton.Name = "abrirToolStripButton";
            this.abrirToolStripButton.Size = new System.Drawing.Size(21, 20);
            this.abrirToolStripButton.Click += new System.EventHandler(this.abrirToolStripButton_Click);
            // 
            // salvarToolStripButton
            // 
            this.salvarToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.salvarToolStripButton.Image = global::Compilador.Properties.Resources.disk_save;
            this.salvarToolStripButton.Name = "salvarToolStripButton";
            this.salvarToolStripButton.Size = new System.Drawing.Size(21, 20);
            this.salvarToolStripButton.Click += new System.EventHandler(this.salvarToolStripButton_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(21, 6);
            // 
            // btnCompilar
            // 
            this.btnCompilar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCompilar.Image = global::Compilador.Properties.Resources.play;
            this.btnCompilar.Name = "btnCompilar";
            this.btnCompilar.Size = new System.Drawing.Size(21, 20);
            this.btnCompilar.Click += new System.EventHandler(this.btnCompilar_Click);
            // 
            // btnLimparTudo
            // 
            this.btnLimparTudo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLimparTudo.Image = global::Compilador.Properties.Resources.trash;
            this.btnLimparTudo.Name = "btnLimparTudo";
            this.btnLimparTudo.Size = new System.Drawing.Size(21, 20);
            this.btnLimparTudo.Click += new System.EventHandler(this.btnLimparTudo_Click);
            // 
            // richTextBoxErro
            // 
            this.richTextBoxErro.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(54)))));
            this.richTextBoxErro.Dock = System.Windows.Forms.DockStyle.Right;
            this.richTextBoxErro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.richTextBoxErro.Location = new System.Drawing.Point(526, 0);
            this.richTextBoxErro.Name = "richTextBoxErro";
            this.richTextBoxErro.Size = new System.Drawing.Size(250, 561);
            this.richTextBoxErro.TabIndex = 1;
            this.richTextBoxErro.Text = "";
            // 
            // lbNomeProjeto
            // 
            this.lbNomeProjeto.AutoSize = true;
            this.lbNomeProjeto.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Bold);
            this.lbNomeProjeto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(184)))), ((int)(((byte)(108)))));
            this.lbNomeProjeto.Location = new System.Drawing.Point(10, 5);
            this.lbNomeProjeto.Name = "lbNomeProjeto";
            this.lbNomeProjeto.Size = new System.Drawing.Size(160, 22);
            this.lbNomeProjeto.TabIndex = 0;
            this.lbNomeProjeto.Text = "Nome do Projeto";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(54)))));
            this.ClientSize = new System.Drawing.Size(800, 561);
            this.Controls.Add(this.lbNomeProjeto);
            this.Controls.Add(this.richTextBoxErro);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lineNumbersForRichText1);
            this.Controls.Add(this.richTextBox1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(242)))));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Form1";
            this.Text = "Compilador LIB";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private LineNumbersControlForRichTextBox.LineNumbersForRichText lineNumbersForRichText1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton novoToolStripButton;
        private System.Windows.Forms.ToolStripButton abrirToolStripButton;
        private System.Windows.Forms.ToolStripButton salvarToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton btnCompilar;
        private System.Windows.Forms.ToolStripButton btnLimparTudo;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.RichTextBox richTextBoxErro;
        private System.Windows.Forms.Label lbNomeProjeto;
        private System.Windows.Forms.Label label1;
    }
}
