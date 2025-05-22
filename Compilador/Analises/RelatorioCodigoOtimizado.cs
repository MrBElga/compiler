using System;
using System.Collections.Generic;
using System.IO;

namespace Compilador.Analises
{
    public static class RelatorioCodigoOtimizado
    {
        public static void Gerar(List<string> linhasOtimizado, string caminhoArquivoFonte)
        {
            try
            {
                string pasta = Path.GetDirectoryName(caminhoArquivoFonte);
                string nomeBase = Path.GetFileNameWithoutExtension(caminhoArquivoFonte);
                string caminhoRelatorio = Path.Combine(pasta, nomeBase + "_relatorioCodigoOtimizado.txt");

                List<string> conteudoRelatorio = new List<string>();

                for (int i = 0; i < linhasOtimizado.Count; i++)
                {
                    conteudoRelatorio.Add($"Linha {i + 1}: {linhasOtimizado[i]}");
                }

                File.WriteAllLines(caminhoRelatorio, conteudoRelatorio);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar relatório otimizado: {ex.Message}");
            }
        }
    }
}