namespace WebAppSystemsTransp.Models.Dto
{
    public class ObservacoesDto
    {
        public int PedidoId { get; set; }
        public string? TempoEspera { get; set; } // Milissegundos desde a época Unix
        public string? Observacao { get; set; }
        public string? NomePessoa { get; set; }
        public string? EmailPessoa { get; set; }
        public string? TelPessoa { get; set; }
    }

}
