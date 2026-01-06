using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models
{
    public class FotoPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }
        public TipoFotoPedido TipoPedido { get; set; }
        public TipoFotoVeiculo TipoFotoVeiculo { get; set; }

        // 🔹 Agora armazenamos apenas a URL da imagem no Blob Storage
        public string UrlFoto { get; set; }

        // 🔹 O nome do arquivo pode ser usado para referência no Azure
        public string NomeArquivo { get; set; } = $"{Guid.NewGuid()}.jpg";

        // 🔹 Data do upload da imagem
        public DateTime DataUpload { get; set; } = DateTime.UtcNow;
    }
}
