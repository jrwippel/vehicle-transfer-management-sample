using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models.Dto
{
    public class AvariasDto
    {
        public int PedidoId { get; set; } // ID do Pedido
        public List<AvariaDto> Avarias { get; set; } // Lista de Avarias
    }

    public class AvariaDto
    {
        public TipoAvaria TipoAvaria { get; set; } // Enum do Tipo de Avaria
        public int X { get; set; } // Coordenada X
        public int Y { get; set; } // Coordenada Y
    }

}
