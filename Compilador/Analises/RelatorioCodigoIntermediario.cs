using System;
using System.Collections.Generic;
using System.IO;

namespace Compilador.Analises
{
    public static class RelatorioCodigoIntermediario
    {
        public static void Gerar(List<string> linhasCodigo, string caminhoArquivoFonte)
        {
            try
            {
                string pasta = Path.GetDirectoryName(caminhoArquivoFonte);
                string nomeBase = Path.GetFileNameWithoutExtension(caminhoArquivoFonte);
                string caminhoRelatorio = Path.Combine(pasta, nomeBase + "_relatorioCodigoIntermediario.txt");

                List<string> conteudoRelatorio = new List<string>();

                for (int i = 0; i < linhasCodigo.Count; i++)
                {
                    conteudoRelatorio.Add($"Linha {i + 1}: {linhasCodigo[i]}");
                }

                File.WriteAllLines(caminhoRelatorio, conteudoRelatorio);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar relatório de código intermediário: {ex.Message}");
            }
        }
    }
}