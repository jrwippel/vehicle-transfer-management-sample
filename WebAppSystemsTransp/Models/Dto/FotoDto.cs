using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models.Dto
{
    public class FotoDto
    {        
        public int Id { get; set; } // Adiciona o identificador único
        public string UrlFoto { get; set; }
        public TipoFotoVeiculo TipoFoto { get; set; }
        public string NomeArquivo { get; set; }
    }

}
