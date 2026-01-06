using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebAppSystemsTransp.Models;

public class FipeService
{
    private readonly HttpClient _httpClient;

    public FipeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Marca>> ObterMarcasAsync()
    {
        var response = await _httpClient.GetStringAsync("https://parallelum.com.br/fipe/api/v1/carros/marcas");

        // Verifique a resposta da API
        if (string.IsNullOrWhiteSpace(response))
        {
            throw new Exception("A resposta da API FIPE está vazia.");
        }

        try
        {
            // Deserializa utilizando o Newtonsoft.Json
            var marcasFipe = JsonConvert.DeserializeObject<List<FipeMarca>>(response);

            // Se a deserialização falhar ou a lista for vazia
            if (marcasFipe == null || !marcasFipe.Any())
            {
                throw new Exception("Nenhuma marca foi retornada pela API.");
            }

            var listaMarcas = new List<Marca>();
            foreach (var fipeMarca in marcasFipe)
            {
                // Convertendo para o modelo de marca da sua aplicação
                listaMarcas.Add(new Marca
                {                    
                    Nome = fipeMarca.Nome,
                    CodigoFipe = fipeMarca.Codigo
                });
            }

            return listaMarcas;
        }
        catch (JsonException ex)
        {
            // Caso ocorra algum erro na deserialização
            throw new Exception("Erro ao deserializar a resposta da API.", ex);
        }
    }

    public async Task<List<Modelo>> ObterModelosPorMarcaAsync(int marcaId)
    {
        var response = await _httpClient.GetStringAsync($"https://parallelum.com.br/fipe/api/v1/carros/marcas/{marcaId}/modelos");

        // Substituindo a deserialização do System.Text.Json para Newtonsoft.Json
        var fipeModelos = JsonConvert.DeserializeObject<FipeModeloResponse>(response);

        var listaModelos = new List<Modelo>();
        foreach (var modelo in fipeModelos.Modelos)
        {
            listaModelos.Add(new Modelo
            {
                //Id = modelo.Codigo,  // Certifique-se de que o tipo esteja correto
                Nome = modelo.Nome,
                MarcaId = marcaId
            });
        }
        return listaModelos;
    }

    public class FipeMarca
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
    }

    public class FipeModeloResponse
    {
        public List<FipeModelo> Modelos { get; set; }
    }

    public class FipeModelo
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
    }
}
