using Portfolio_Financeiro.Models;
using System.Text.Json;

namespace Portfolio_Financeiro.Data
{
    public class DataContext
    {
        public SeedDataRoot Database { get; private set; }

        public DataContext()
        {
            string finalPath = BuscarArquivoMagico();

            if (finalPath != null)
            {
                var jsonString = File.ReadAllText(finalPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Database = JsonSerializer.Deserialize<SeedDataRoot>(jsonString, options) ?? new SeedDataRoot();
            }
            else
            {
                throw new FileNotFoundException("O arquivo SeedData.json não foi encontrado em nenhuma pasta superior!");
            }
        }

        private string BuscarArquivoMagico()
        {
            var diretorioAtual = new DirectoryInfo(AppContext.BaseDirectory);

            while (diretorioAtual != null)
            {
                var caminhoTentativa = Path.Combine(diretorioAtual.FullName, "Data", "SeedData.json");

                if (File.Exists(caminhoTentativa))
                {
                    return caminhoTentativa;
                }

                diretorioAtual = diretorioAtual.Parent; 
            }

            return null;
        }
    }
}