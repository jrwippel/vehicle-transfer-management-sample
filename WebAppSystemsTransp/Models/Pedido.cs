using System.ComponentModel.DataAnnotations.Schema;
using WebAppSystems.Models;
using WebAppSystemsTransp.Models.Enums;

namespace WebAppSystemsTransp.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime DataPedido { get; set; }
        public TimeSpan HoraPedido { get; set; }
        public DateTime DataPretendida { get; set; }  
        
        public string? AdicionalCarga { get; set; }       
        public string? AdicionalDescarga { get; set; }

        // Chave estrangeira para o veículo
        public int VeiculoId { get; set; }
        // Propriedade de navegação para o veículo        
        public Veiculo Veiculo { get; set; }

        // Cliente relacionado à carga
        public int ClienteCargaId { get; set; }
        public Cliente ClienteCarga { get; set; }

        // Cliente relacionado à descarga
        public int ClienteDescargaId { get; set; }
        public Cliente ClienteDescarga { get; set; }

        public int? MotoristaId { get; set; } // Foreign key para Motorista
        public Attorney Motorista { get; set; } // Relacionamento

        public TipoPedidoEnum TipoPedido { get; set; }

        // Recolha
        public DateTime? DataExecucao { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFinal { get; set; }
        public float KmInicial { get; set; }
        public float KmFinal { get; set; }


        public int CombustivelInicial { get; set; }  
        public int CombustivelFinal { get; set; }
        public int TrianguloHomologado { get; set; }
        public int ColeteHomologado { get; set; }

        public int DocumentoVeiculo { get; set; }

        public int DocumentoSeguro { get; set; }  

        public ICollection<FotoPedido> FotoPedidos { get; set; }

        public TimeSpan? TempoEsperaCarga { get; set; }
        public TimeSpan? TempoEsperaDescarga { get; set; }
        public string? ObservacaoCarga { get; set; }
        public string? ObservacaoDescarga { get; set; } 

        public string? NomePessoaCar { get; set; }
        public string? EmailPessoaCar { get; set; }
        public string? TelPessoaCar { get; set; }

        public string? NomePessoaDes { get; set; }
        public string? EmailPessoaDes { get; set; }
        public string? TelPessoaDes { get; set; }

        public ICollection<Avaria> Avarias { get; set; } = new List<Avaria>(); // Relacionamento 1-N


    }
}
