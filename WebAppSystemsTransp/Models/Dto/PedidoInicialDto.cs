using WebAppSystems.Models;
using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models.Dto
{
    public class PedidoInicialDto
    {

        public int Id { get; set; }
        public DateTime DataPedido { get; set; }
        public TimeSpan HoraPedido { get; set; }
        public DateTime DataPretendida { get; set; }

        public string AdicionalCarga { get; set; }
        public string AdicionalDescarga { get; set; }

        // Chave estrangeira para o veículo
        public int VeiculoId { get; set; }
        // Propriedade de navegação para o veículo        
        public Veiculo Veiculo { get; set; }

        // Cliente relacionado à carga
        public int ClienteCargaId { get; set; }
        public Cliente ClienteCarga { get; set; }

        // Cliente relacionado à descarga
        public int ClienteDescargaId { get; set; }
        public Cliente ClienteDescarga { get; set; }

        public int MotoristaId { get; set; } // Foreign key para Motorista
        public Attorney Motorista { get; set; } // Relacionamento

        public TipoPedidoEnum TipoPedido { get; set; }
    }
}
