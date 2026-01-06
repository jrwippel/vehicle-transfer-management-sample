namespace WebAppSystemsTransp.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Local { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string? Documento { get; set; }
        // Relacionamento com os pedidos
        public ICollection<Pedido> PedidosCarga { get; set; } = new List<Pedido>();
        public ICollection<Pedido> PedidosDescarga { get; set; } = new List<Pedido>();


    }
}
