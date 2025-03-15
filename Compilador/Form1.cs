using Compilador.Analises;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compilador
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
        }

        private void novoToolStripButton_Click(object sender, EventArgs e)
        {
            //Limpar Texto dentro da Caixa de Diálogo
            richTextBox1.Clear();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            lbNomeProjeto.Text = "Nome do Projeto";
            //Posicionar o cursor
            richTextBox1.Focus();
        }

        private void abrirToolStripButton_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Title = "Abrir arquivo"; //Título da janela
            openFileDialog1.InitialDirectory = ""; //Diretório inicial - Desktop
            openFileDialog1.FileName = ""; //Limpa o nome da busca pelo arquivo
            openFileDialog1.Filter = "Arquivo de texto (*.txt)|*.txt|Todos os arquivos(*.*)|*.*"; //Opção de filtro que irá abrir

            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    lbNomeProjeto.Text = openFileDialog1.FileName;
                    FileStream arquivo = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);
                    StreamReader streamReader = new StreamReader(arquivo); //Criação do leitor
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    this.richTextBox1.Text = "";

                    string linha = streamReader.ReadLine(); //Ler uma linha e armazenar na variável

                    //Enquanto houver linha ele vai ler e armazenar na variavel linha
                    while (linha != null)
                    {
                        this.richTextBox1.Text += linha + "\n";
                        linha = streamReader.ReadLine(); //Leitura da nova linha
                    }
                    streamReader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro de leitura: " + ex.Message, "Erro ao ler arquivo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Salvar()
        {
            try
            {
                if (saveFileDialog1.FileName == "" && openFileDialog1.FileName == "" )
                {
                    if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        //Criação do Arquivo
                        lbNomeProjeto.Text = saveFileDialog1.FileName;
                        FileStream arquivo = new FileStream(saveFileDialog1.FileName + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter streamReader = new StreamWriter(arquivo); //Criação do escritor
                        streamReader.Flush(); //Responsável por fazer a transição com o buffer
                        streamReader.BaseStream.Seek(0, SeekOrigin.Begin); //A partir de onde começará a escrever
                        streamReader.Write(this.richTextBox1.Text); //Conteúdo que será gravado
                        streamReader.Flush();
                        streamReader.Close(); //Fechar o escritor
                    }
                }
                else
                {
                    string nome;
                    if (saveFileDialog1.FileName != "")
                    {
                         nome = saveFileDialog1.FileName +".txt";
                    }
                    else
                    {
                        nome = openFileDialog1.FileName;
                    }
                    //Criação do Arquivo
                    lbNomeProjeto.Text = nome;
                    FileStream arquivo = new FileStream(nome, FileMode.Create, FileAccess.Write);
                    StreamWriter streamReader = new StreamWriter(arquivo); //Criação do escritor
                    streamReader.Flush(); //Responsável por fazer a transição com o buffer
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin); //A partir de onde começará a escrever
                    streamReader.Write(this.richTextBox1.Text); //Conteúdo que será gravado
                    streamReader.Flush();
                    streamReader.Close(); //Fechar o escritor
                }
            }
            catch (Exception ex) //Caso aconteça algum erro
            {
                MessageBox.Show("Erro na gravação: " + ex.Message, "Erro ao gravar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void salvarToolStripButton_Click(object sender, EventArgs e)
        {
            Salvar();
        }

        private void btnNegrito_Click(object sender, EventArgs e)
        {
            string nome_da_fonte = null;
            float tamanho_da_fonte = 0;
            bool negrito, italico, sublinhado = false;

            nome_da_fonte = richTextBox1.Font.Name;
            tamanho_da_fonte = richTextBox1.Font.Size;
            negrito = richTextBox1.SelectionFont.Bold;
            italico = richTextBox1.SelectionFont.Italic;
            sublinhado = richTextBox1.SelectionFont.Underline;

            richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Regular); //Colocar o texto de forma comum (sem negrito)

            //Verificação se o campo selecionado está em negrito
            if (negrito == false)
            {
                if (italico == true & sublinhado == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline); //Colocar o texto em negrito, itálico e sublinhado
                }
                else if (italico == false & sublinhado == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold | FontStyle.Underline); //Colocar o texto em negrito e sublinhado
                }
                else if (italico == true & sublinhado == false)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold | FontStyle.Italic); //Colocar o texto em negrito e itálico
                }
                else if (italico == false & sublinhado == false)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold); //Colocar o texto em negrito
                }
            }
            else
            {
                if (italico == true & sublinhado == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Italic | FontStyle.Underline); //Colocar o texto itálico e sublinhado
                }
                else if (italico == false & sublinhado == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Underline); //Colocar o texto em sublinhado
                }
                else if (italico == true & sublinhado == false)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Italic); //Colocar o texto em itálico
                }
            }
        }

        private void btnSublinhar_Click(object sender, EventArgs e)
        {
            string nome_da_fonte = null;
            float tamanho_da_fonte = 0;
            bool negrito, italico, sublinhado = false;

            nome_da_fonte = richTextBox1.Font.Name;
            tamanho_da_fonte = richTextBox1.Font.Size;
            negrito = richTextBox1.SelectionFont.Bold;
            italico = richTextBox1.SelectionFont.Italic;
            sublinhado = richTextBox1.SelectionFont.Underline;

            richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Regular); //Colocar o texto de forma comum

            //Verificação se o campo selecionado está sublinhado
            if (sublinhado == false)
            {
                if (negrito == true & italico == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline); //Colocar o texto em negrito, itálico e sublinhado
                }
                else if (negrito == false & italico == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Italic | FontStyle.Underline); //Colocar o texto em itálico e sublinhado
                }
                else if (negrito == true & italico == false)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold | FontStyle.Underline); //Colocar o texto em negrito e sublinhado
                }
                else if (negrito == false & italico == false)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Underline); //Colocar o texto sublinhado
                }
            }
            else
            {
                if (negrito == true & italico == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold | FontStyle.Italic); //Colocar o texto negrito e italico
                }
                else if (negrito == false & italico == true)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Italic); //Colocar o texto em itálico
                }
                else if (negrito == true & italico == false)
                {
                    richTextBox1.SelectionFont = new Font(nome_da_fonte, tamanho_da_fonte, FontStyle.Bold); //Colocar o texto em negrito
                }
            }
        }

        private void btnLimparTudo_Click(object sender, EventArgs e)
        {
            //Limpar Texto dentro da Caixa de Diálogo
            richTextBox1.Clear();
            //Posicionar o cursor
            richTextBox1.Focus();
        }

        private void btnCompilar_Click(object sender, EventArgs e)
        {
            this.richTextBoxErro.Text = "";
            Salvar();
            string nome;
            if (saveFileDialog1.FileName != "")
            {
                nome = saveFileDialog1.FileName + ".txt";
            }
            else
            {
                nome = openFileDialog1.FileName;
            }

           // FileStream arquivo = new FileStream(nome, FileMode.Open, FileAccess.Read);
            /* StreamReader streamReader = new StreamReader(arquivo); //Criação do leitor
             streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
             this.richTextBoxErro.Text = "";
             this.richTextBoxErro.Text = "Nome Arquivo : "+ nome;

             string linha = streamReader.ReadLine(); //Ler uma linha e armazenar na variável

             //Enquanto houver linha ele vai ler e armazenar na variavel linha
             while (linha != null)
             {
                 //linha.
                 richTextBoxErro.Text += linha + "\n";
                 linha = streamReader.ReadLine(); //Leitura da nova linha
             }
             streamReader.Close();*/
            // richTextBoxErro.Text = "ERRO : adicona o erro  => LINHA : 12 \n";

            FileStream arquivo = new FileStream(nome, FileMode.Open, FileAccess.Read);
            Analise_Lexica analise_Lexica = new Analise_Lexica(arquivo);
            string relatorioLexico = analise_Lexica.AnalisadorLexico();

            //Criação do Arquivo
            nome = nome.Replace(".txt", "");
            FileStream arquivoRelatorio = new FileStream(nome+"_relatorioAnaliseLexica.txt", FileMode.Create, FileAccess.Write);
            StreamWriter streamReader = new StreamWriter(arquivoRelatorio); //Criação do escritor
            streamReader.Flush(); //Responsável por fazer a transição com o buffer
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin); //A partir de onde começará a escrever
            streamReader.Write(relatorioLexico); //Conteúdo que será gravado
            streamReader.Flush();
            streamReader.Close(); //Fechar o escritor

           
            string[] erroRelatorio = relatorioLexico.Split('\n');
            string erroLinha = "";
            for (int i = 0; i < erroRelatorio.Length; i++)
            {
                if (erroRelatorio[i].Contains("ERRO"))
                {
                    erroLinha += "  @"+erroRelatorio[i] + "\n";
                }
               
             }

            if (erroLinha == "")
            {
                richTextBoxErro.Text = "Compilado com Sucesso!!!\n " + erroLinha + "* Relátorio de Análise Lexica Gerado !!";
            }
            else
            {
                richTextBoxErro.Text = "ERRO ao Compilar!!!\n" + erroLinha + "\n* Relátorio de Análise Lexica Gerado !!";
            }
           

        }
    }
}
