namespace WebAppSystemsTransp.Models.Dto
{
    public class StartStopRequest
    {
        public int Id { get; set; }
        public int KmInicial { get; set; }
        public int KmFinal { get; set; }
        public string CombustivelInicial { get; set; }  // Combustível na hora de início
        public string CombustivelFinal { get; set; }    // Combustível na hora de finalização

    }
}
