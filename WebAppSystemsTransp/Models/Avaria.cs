using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models
{
    public class Avaria
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }  // Relacionamento com Pedido
        public Pedido Pedido { get; set; } // Propriedade de navegação

        public TipoAvaria TipoAvaria { get; set; } // Agora usando o enum

        public int X { get; set; } // Coordenada X no diagrama do veículo
        public int Y { get; set; } // Coordenada Y no diagrama do veículo
    }
}
