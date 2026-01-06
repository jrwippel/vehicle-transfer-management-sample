namespace WebAppSystemsTransp.Models.ViewModels
{
    public class PedidoViewModel
    {
        public int Id { get; set; }
        public DateTime DataPretendida { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFinal { get; set; }
        public bool PossuiTodasFotosCarga { get; set; }
        public bool PossuiTodasFotosDescarga { get; set; }
        public List<string> FotosCapturadas { get; set; } // Lista de fotos capturadas por tipo

    }
}
