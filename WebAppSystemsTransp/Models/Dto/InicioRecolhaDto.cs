using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models.Dto
{
    public class InicioRecolhaDto
    {
        public int PedidoId { get; set; }      

        public float KmInicial { get; set; }

        public TipoCombustivel CombustivelInicial { get; set; }
        public DateTime HoraInicio { get; set; }

        public int TrianguloHomologado { get; set; }
        public int ColeteHomologado { get; set; }
        public int DocumentoVeiculo { get; set; }
        public int DocumentoSeguro { get; set; }
        public string AssMotoristaRecolha { get; set; }

        public string AssClienteRecolha { get; set; }

        public string AssMotoristaEntrega { get; set; }

        public string AssClienteEntrega { get; set; }

        public List<IFormFile> FotosRecolha { get; set; }

        public string ObservacaoCarga { get; set; }
        public string TempoEsperaCarga { get; set; }  // Formato "HH:mm"

        public string Avarias { get; set; }  // JSON das avarias
        public string NomePessoaCar { get; set; }
        public string EmailPessoaCar { get; set; }
        public string TelPessoaCar { get; set; }

    }
}
