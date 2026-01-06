using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models.Dto
{
    public class FinalRecolhaDto
    {
        public int PedidoId { get; set; }
        public float KmFinal { get; set; }
        public TipoCombustivel CombustivelFinal { get; set; }
        public DateTime HoraFinal { get; set; }

        public string AssMotoristaEntrega { get; set; }

        public string AssClienteEntrega { get; set; }

        public List<IFormFile> FotosEntrega { get; set; }
        public string ObservacaoDescarga { get; set; }
        public string TempoEsperaDescarga { get; set; }  // Formato "HH:mm"

        public string NomePessoaDes { get; set; }
        public string EmailPessoaDes { get; set; }
        public string TelPessoaDes { get; set; }
    }
}
