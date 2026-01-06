namespace WebAppSystemsTransp.Models
{
    public class Marca
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public string CodigoFipe { get; set; }

        // Uma marca pode ter vários modelos
        public ICollection<Modelo> Modelos { get; set; } = new List<Modelo>();
    }
}
