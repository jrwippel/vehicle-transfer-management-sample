namespace WebAppSystemsTransp.Models
{
    public class Modelo
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        // Relação com a marca
        public int MarcaId { get; set; }
        public Marca Marca { get; set; }
    }
}
