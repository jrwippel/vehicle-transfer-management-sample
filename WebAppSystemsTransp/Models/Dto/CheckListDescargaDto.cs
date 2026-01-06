namespace WebAppSystemsTransp.Models.Dto
{
    public class ChecklistDescargaDto
    {
        public int PedidoId { get; set; }
        public long HoraFinal { get; set; } // Agora é timestamp (em milissegundos)
        public float KmFinal { get; set; }
        public int CombustivelFinal { get; set; }
    }


}
