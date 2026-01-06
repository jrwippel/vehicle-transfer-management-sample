namespace WebAppSystemsTransp.Models.Dto
{
    public class ChecklistCargaDto
    {
        public int PedidoId { get; set; }
        public long HoraInicio { get; set; } // Timestamp (em milissegundos)
        public float KmInicial { get; set; }
        public int CombustivelInicial { get; set; }
        public int TrianguloHomologado { get; set; }
        public int ColeteHomologado { get; set; }
        public int DocumentoSeguro { get; set; }
        public int DocumentoVeiculo { get; set; }

        public bool IsRegistroExistente { get; set; }

    }



}
