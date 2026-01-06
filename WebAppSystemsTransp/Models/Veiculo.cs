using WebAppSystemsTransp.Models;

namespace WebAppSystemsTransp.Models
{
    public class Veiculo
    {

        public int Id { get; set; }
        public int ModeloId { get; set; }
        public Modelo Modelo { get; set; }  // Relacionamento com Modelo

        public string Matricula { get; set; }    
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();       

    }
}
