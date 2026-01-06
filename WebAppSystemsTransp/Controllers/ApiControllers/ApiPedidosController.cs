using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WebAppSystemsTransp.Models.Dto;
using WebAppSystems.Services;
using WebAppSystemsTransp.Services; // Serviço de banco de dados (ex.: DbContext)
using WebAppSystemsTransp.Models.Enums;
using WebAppSystemsTransp.Models;
using System;

namespace WebAppSystems.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPedidosController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        private readonly AvariaService _avariaService;

        private readonly BlobStorageService _blobStorageService;

        private readonly PedidoPdfService _pedidoPdfService;
        public ApiPedidosController(PedidoService pedidoService, BlobStorageService blobStorageService, AvariaService avariaService, PedidoPdfService pedidoPdfService)
        {
            _pedidoService = pedidoService;
            _blobStorageService = blobStorageService;
            _avariaService = avariaService;
            _pedidoPdfService = pedidoPdfService;   
        }

        [HttpGet("generate-pdf/{orderId}")]
        public async Task<IActionResult> GeneratePdf(int orderId)
        {
            try
            {
                var fileStreamResult = await _pedidoPdfService.GeneratePdfAsync(orderId);

                if (fileStreamResult == null)
                {
                    return BadRequest("Erro ao gerar o PDF.");
                }

                return fileStreamResult;
            }
            catch (Exception ex)
            {
                // Log de erro, se necessário
                Console.WriteLine($"Erro ao gerar PDF: {ex.Message}");
                return StatusCode(500, "Erro interno ao processar o PDF.");
            }
        }




        [HttpGet("ValidateChecklistPrerequisitesDescarga/{pedidoId}")]
        public IActionResult ValidateChecklistPrerequisitesDescarga(int pedidoId)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Verifica se NomePessoaCar e EmailPessoaCar estão preenchidos
                bool camposPreenchidos = !string.IsNullOrWhiteSpace(pedido.NomePessoaDes) &&
                                         !string.IsNullOrWhiteSpace(pedido.EmailPessoaDes);

                // Verifica as fotos necessárias
                var fotosNecessarias = new List<int>
        {
            (int)TipoFotoVeiculo.LocalEstacionamento
        };

                // Converte fotos no banco para inteiros
                var fotosPedidoInt = pedido.FotoPedidos
                    .Select(f => (int)f.TipoFotoVeiculo)
                    .ToList();

                // Verifica se todas as fotos necessárias estão presentes
                bool fotosCompletas = fotosNecessarias.All(tipo => fotosPedidoInt.Contains(tipo));

                // Retorna o estado geral
                return Ok(new
                {
                    CamposPreenchidos = camposPreenchidos,
                    FotosCompletas = fotosCompletas,
                    HabilitarSalvar = camposPreenchidos && fotosCompletas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao validar checklist.", Erro = ex.Message });
            }
        }


        [HttpGet("ValidateChecklistPrerequisites/{pedidoId}")]
        public IActionResult ValidateChecklistPrerequisites(int pedidoId)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Verifica se NomePessoaCar e EmailPessoaCar estão preenchidos
                bool camposPreenchidos = !string.IsNullOrWhiteSpace(pedido.NomePessoaCar) &&
                                         !string.IsNullOrWhiteSpace(pedido.EmailPessoaCar);

                // Verifica as fotos necessárias
                var fotosNecessarias = new List<int>
        {
            (int)TipoFotoVeiculo.RodaDianteiraDir,
            (int)TipoFotoVeiculo.RodaDianteiraEsq,
            (int)TipoFotoVeiculo.RodaTraseiraDir,
            (int)TipoFotoVeiculo.RodaTraseiraEsq,
            (int)TipoFotoVeiculo.CapoDianteiro,
            (int)TipoFotoVeiculo.CapoTraseiroFechado,
            (int)TipoFotoVeiculo.AssMotorista,
            (int)TipoFotoVeiculo.AssCliente
        };

                // Converte fotos no banco para inteiros
                var fotosPedidoInt = pedido.FotoPedidos
                    .Select(f => (int)f.TipoFotoVeiculo)
                    .ToList();


                // Verifica se todas as fotos necessárias estão presentes
                bool fotosCompletas = fotosNecessarias.All(tipo => fotosPedidoInt.Contains(tipo));

                // Retorna o estado geral
                return Ok(new
                {
                    CamposPreenchidos = camposPreenchidos,
                    FotosCompletas = fotosCompletas,
                    HabilitarSalvar = camposPreenchidos && fotosCompletas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao validar checklist.", Erro = ex.Message });
            }
        }




        [HttpPost("SaveAvarias")]
        public IActionResult SaveAvarias([FromBody] AvariasDto avariasDto)
        {
            try
            {
                // Verifica se o pedido existe no banco de dados
                var pedido = _pedidoService.GetPedidoById(avariasDto.PedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Itera pelas avarias enviadas no DTO
                foreach (var avaria in avariasDto.Avarias)
                {
                    // Cria uma nova entidade de Avaria e preenche os dados
                    var novaAvaria = new Avaria
                    {
                        PedidoId = avariasDto.PedidoId,
                        TipoAvaria = avaria.TipoAvaria,
                        X = avaria.X,
                        Y = avaria.Y
                    };

                    // Salva no banco
                    _avariaService.AddAvaria(novaAvaria);
                }

                return Ok(new { Message = "Avarias salvas com sucesso!" });
            }
            catch (Exception ex)
            {
                // Captura exceções e retorna erro com mensagem detalhada
                return StatusCode(500, new { Message = "Erro ao salvar avarias.", Erro = ex.Message });
            }
        }

        

        [HttpGet("GetChecklistDescarga/{pedidoId}")]
        public IActionResult GetChecklistDescarga(int pedidoId)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Converte a hora para Unix Timestamp
                var horaFinalTimestamp = pedido.HoraFinal.HasValue
                        ? ((DateTimeOffset)DateTime.Today.Add(pedido.HoraFinal.Value)).ToUnixTimeMilliseconds()
                        : 0; // Define um valor padrão para quando HoraInicio for nulo


                var checklistDto = new ChecklistDescargaDto
                {
                    PedidoId = pedido.Id,
                    HoraFinal = horaFinalTimestamp,
                    KmFinal = pedido.KmFinal,
                    CombustivelFinal = pedido.CombustivelFinal,
                };

                return Ok(checklistDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao obter o checklist.", Erro = ex.Message });
            }
        } 

        [HttpPost("ChecklistDescarga")]
        public IActionResult ChecklistDescarga([FromBody] ChecklistDescargaDto checklistDto)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(checklistDto.PedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Atualiza os campos do pedido
                pedido.DataExecucao = DateTime.UtcNow.Date;
                pedido.HoraFinal = DateTimeOffset.FromUnixTimeMilliseconds(checklistDto.HoraFinal).TimeOfDay;
                pedido.KmFinal = checklistDto.KmFinal;
                pedido.CombustivelFinal = checklistDto.CombustivelFinal;
                _pedidoService.UpdatePedido(pedido); // Salva as alterações no banco

                return Ok(new { Message = "Checklist salvo com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao salvar o checklist descarga.", Erro = ex.Message });
            }
        }

        [HttpGet("GetObservacaoCarga/{pedidoId}")]
        public IActionResult GetObservacaoCarga(int pedidoId)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Converte TempoEsperaCarga (TimeSpan) para uma string formatada
                var tempoEsperaCargaFormatado = pedido.TempoEsperaCarga?.ToString(@"hh\:mm\:ss");

                var observacaoDto = new ObservacoesDto
                {
                    PedidoId = pedido.Id,
                    TempoEspera = tempoEsperaCargaFormatado, // Agora como string no formato HH:mm:ss
                    Observacao = pedido.ObservacaoCarga,
                    NomePessoa = pedido.NomePessoaCar,
                    EmailPessoa = pedido.EmailPessoaCar,
                    TelPessoa = pedido.TelPessoaCar
                };

                return Ok(observacaoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao buscar observação carga.", Erro = ex.Message });
            }
        }

        [HttpPost("SaveObservacaoCarga")]
        public IActionResult SaveObservacaoCarga([FromBody] ObservacoesDto checklistDto)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(checklistDto.PedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Converte TempoEsperaCarga se estiver no formato correto
                pedido.TempoEsperaCarga = TimeSpan.TryParse(checklistDto.TempoEspera, out var tempoEspera)
                    ? tempoEspera
                    : null;

                // Atualiza outros campos
                pedido.ObservacaoCarga = checklistDto.Observacao;
                pedido.NomePessoaCar = checklistDto.NomePessoa;
                pedido.EmailPessoaCar = checklistDto.EmailPessoa;
                pedido.TelPessoaCar = checklistDto.TelPessoa;

                _pedidoService.UpdatePedido(pedido); // Salva no banco

                return Ok(new { Message = "Observação salva com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao salvar observação carga.", Erro = ex.Message });
            }
        }


        [HttpGet("GetObservacaoDescarga/{pedidoId}")]
        public IActionResult GetObservacaoDescarga(int pedidoId)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Converte TempoEsperaCarga (TimeSpan) para uma string formatada
                var tempoEsperaDescargaFormatado = pedido.TempoEsperaDescarga?.ToString(@"hh\:mm\:ss");

                var observacaoDto = new ObservacoesDto
                {
                    PedidoId = pedido.Id,
                    TempoEspera = tempoEsperaDescargaFormatado, // Agora como string no formato HH:mm:ss
                    Observacao = pedido.ObservacaoDescarga,
                    NomePessoa = pedido.NomePessoaDes,
                    EmailPessoa = pedido.EmailPessoaDes,
                    TelPessoa = pedido.TelPessoaDes
                };

                return Ok(observacaoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao buscar observação carga.", Erro = ex.Message });
            }
        }

        [HttpPost("SaveObservacaoDescarga")]
        public IActionResult SaveObservacaoDescarga([FromBody] ObservacoesDto checklistDto)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(checklistDto.PedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Converte TempoEsperaCarga se estiver no formato correto
                pedido.TempoEsperaDescarga = TimeSpan.TryParse(checklistDto.TempoEspera, out var tempoEspera)
                    ? tempoEspera
                    : null;

                // Atualiza outros campos
                pedido.ObservacaoDescarga = checklistDto.Observacao;
                pedido.NomePessoaDes = checklistDto.NomePessoa;
                pedido.EmailPessoaDes = checklistDto.EmailPessoa;
                pedido.TelPessoaDes = checklistDto.TelPessoa;

                _pedidoService.UpdatePedido(pedido); // Salva no banco

                return Ok(new { Message = "Observação salva com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao salvar observação carga.", Erro = ex.Message });
            }
        }

        [HttpGet("GetChecklistCarga/{pedidoId}")]
        public IActionResult GetChecklistCarga(int pedidoId)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Converte a hora para Unix Timestamp
                var horaInicioTimestamp = pedido.HoraInicio.HasValue
                        ? ((DateTimeOffset)DateTime.Today.Add(pedido.HoraInicio.Value)).ToUnixTimeMilliseconds()
                        : 0;

                var checklistDto = new ChecklistCargaDto
                {
                    PedidoId = pedido.Id,
                    HoraInicio = horaInicioTimestamp,
                    KmInicial = pedido.KmInicial,
                    CombustivelInicial = pedido.CombustivelInicial,
                    TrianguloHomologado = pedido.TrianguloHomologado,
                    ColeteHomologado = pedido.ColeteHomologado,
                    DocumentoSeguro = pedido.DocumentoSeguro,
                    DocumentoVeiculo = pedido.DocumentoVeiculo,

                };

                return Ok(checklistDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao obter o checklist.", Erro = ex.Message });
            }
        }

        [HttpPost("ChecklistCarga")]
        public IActionResult ChecklistCarga([FromBody] ChecklistCargaDto checklistDto)
        {
            try
            {
                var pedido = _pedidoService.GetPedidoById(checklistDto.PedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = "Pedido não encontrado!" });
                }

                // Atualiza os campos do pedido
                pedido.DataExecucao = DateTime.UtcNow.Date;
                pedido.HoraInicio = DateTimeOffset.FromUnixTimeMilliseconds(checklistDto.HoraInicio).TimeOfDay;
                pedido.KmInicial = checklistDto.KmInicial;
                pedido.CombustivelInicial = checklistDto.CombustivelInicial;
                pedido.TrianguloHomologado = checklistDto.TrianguloHomologado;
                pedido.ColeteHomologado = checklistDto.ColeteHomologado;
                pedido.DocumentoSeguro = checklistDto.DocumentoSeguro;
                pedido.DocumentoVeiculo = checklistDto.DocumentoVeiculo;

                _pedidoService.UpdatePedido(pedido); // Salva as alterações no banco

                return Ok(new { Message = "Checklist salvo com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao salvar o checklist.", Erro = ex.Message });
            }
        }





        [HttpDelete("DeleteFotoPedido/{fileName}")]
        public async Task<IActionResult> DeleteFotoPedido(string fileName)
        {
            try
            {
                // Buscar a foto no banco de dados pelo NomeArquivo
                var foto = _pedidoService.GetFotoPedidoByFileName(fileName);
                if (foto == null)
                {
                    return NotFound(new { Message = $"Foto com o nome {fileName} não encontrada." });
                }

                // Remover a foto do Blob Storage
                await _blobStorageService.DeleteFileAsync(fileName);

                // Remover a foto do banco de dados
                _pedidoService.DeleteFotoPedido(foto);

                return Ok(new { Message = "Foto removida com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao remover a foto.", Erro = ex.Message });
            }
        }


        [HttpPost("UploadFotoPedido")]
        public async Task<IActionResult> ApiUploadPedidoPhotos()
        {
            try
            {
                // Verificar se há arquivos na requisição
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "Nenhum arquivo foi enviado." });
                }

                // Obter campos adicionais
                var tipoFoto = formCollection["TipoFoto"];
                var nomeFoto = formCollection["NomeFoto"];
                if (!int.TryParse(formCollection["PedidoId"], out var pedidoId))
                {
                    return BadRequest(new { Message = "PedidoId está ausente ou inválido." });
                }

                if (!int.TryParse(formCollection["TipoFotoPedido"], out var tipoFotoPedido))
                {
                    return BadRequest(new { Message = "TipoFotoPedido está ausente ou inválido." });
                }

                if (string.IsNullOrWhiteSpace(tipoFoto) || string.IsNullOrWhiteSpace(nomeFoto))
                {
                    return BadRequest(new { Message = "Campos TipoFoto ou NomeFoto estão ausentes ou inválidos." });
                }

                // Validar se o pedido existe
                var pedido = _pedidoService.GetPedidoById(pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { Message = $"Pedido com ID {pedidoId} não encontrado." });
                }

                // Fazer upload para Blob Storage
                var fileName = $"{Guid.NewGuid()}-{nomeFoto}";
                using (var stream = file.OpenReadStream())
                {
                    var urlFoto = await _blobStorageService.UploadFileAsync(fileName, stream);

                    // Salvar foto no banco de dados
                    var tipoFotoEnum = (TipoFotoVeiculo)Enum.Parse(typeof(TipoFotoVeiculo), tipoFoto);
                    var tipoFotoPedidoEnum = (TipoFotoPedido)tipoFotoPedido;

                    var fotoSalva = _pedidoService.SaveOrUpdateFotoPedido(
                        pedidoId,
                        tipoFotoEnum,
                        fileName,
                        urlFoto,
                        tipoFotoPedidoEnum // Novo campo
                    );

                    return Ok(new
                    {
                        Message = "Foto enviada com sucesso!",
                        Foto = new
                        {
                            UrlFoto = fotoSalva.UrlFoto,
                            NomeArquivo = fotoSalva.NomeArquivo,
                            TipoFoto = fotoSalva.TipoFotoVeiculo,
                            TipoFotoPedido = fotoSalva.TipoPedido // Adiciona o TipoFotoPedido à resposta
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao enviar a foto.", Erro = ex.Message });
            }
        }

        [HttpGet("{id}/photosassdescarga")]
        public IActionResult GetPhotosAssDescarga(int id)
        {
            var pedido = _pedidoService.GetPhotoPedidoById(id);
            if (pedido == null || pedido.FotoPedidos == null)
            {
                return NotFound(new { Message = $"Nenhuma foto encontrada para o pedido com ID {id}." });
            }

            var fotosDto = pedido.FotoPedidos
                .Where(f => (f.TipoFotoVeiculo == TipoFotoVeiculo.AssMotorista || f.TipoFotoVeiculo == TipoFotoVeiculo.AssCliente) && f.TipoPedido == TipoFotoPedido.Descarga)
                .Select(f => new FotoDto
                {
                    UrlFoto = f.UrlFoto,
                    TipoFoto = f.TipoFotoVeiculo,
                    NomeArquivo = f.NomeArquivo
                }).ToList();

            return Ok(fotosDto);
        }



        [HttpGet("{id}/photos")]
        public IActionResult GetPedidoPhotos(int id)
        {
            var pedido = _pedidoService.GetPhotoPedidoById(id);
            if (pedido == null || pedido.FotoPedidos == null)
            {
                return NotFound(new { Message = $"Nenhuma foto encontrada para o pedido com ID {id}." });
            }

            var fotosDto = pedido.FotoPedidos
                .Where(f => (f.TipoFotoVeiculo == TipoFotoVeiculo.AssMotorista || f.TipoFotoVeiculo == TipoFotoVeiculo.AssCliente) && f.TipoPedido == TipoFotoPedido.Carga)
                .Select(f => new FotoDto
                {
                    UrlFoto = f.UrlFoto,
                    TipoFoto = f.TipoFotoVeiculo,
                    NomeArquivo = f.NomeArquivo
                }).ToList();

            return Ok(fotosDto);
        }

        [HttpGet("{id}/photoscarga")]
        public IActionResult GetPedidoPhotosCarga(int id)
        {
            var pedido = _pedidoService.GetPhotoPedidoById(id);
            if (pedido == null || pedido.FotoPedidos == null)
            {
                return NotFound(new { Message = $"Nenhuma foto encontrada para o pedido com ID {id}." });
            }

            var fotosDto = pedido.FotoPedidos
                .Where(f => f.TipoPedido == TipoFotoPedido.Carga &&
                            f.TipoFotoVeiculo != TipoFotoVeiculo.AssMotorista &&
                            f.TipoFotoVeiculo != TipoFotoVeiculo.AssCliente)
                .Select(f => new FotoDto
                {
                    UrlFoto = f.UrlFoto,
                    TipoFoto = f.TipoFotoVeiculo,
                    NomeArquivo = f.NomeArquivo
                }).ToList();

            return Ok(fotosDto);
        }

        [HttpGet("{id}/photosdescarga")]
        public IActionResult GetPedidoPhotosDescarga(int id)
        {
            var pedido = _pedidoService.GetPhotoPedidoById(id);
            if (pedido == null || pedido.FotoPedidos == null)
            {
                return NotFound(new { Message = $"Nenhuma foto encontrada para o pedido com ID {id}." });
            }

            var fotosDto = pedido.FotoPedidos
                .Where(f => f.TipoPedido == TipoFotoPedido.Descarga &&
                            f.TipoFotoVeiculo != TipoFotoVeiculo.AssMotorista &&
                            f.TipoFotoVeiculo != TipoFotoVeiculo.AssCliente)
                .Select(f => new FotoDto
                {
                    UrlFoto = f.UrlFoto,
                    TipoFoto = f.TipoFotoVeiculo,
                    NomeArquivo = f.NomeArquivo
                }).ToList();

            return Ok(fotosDto);
        }




        // Endpoint para baixar uma imagem específica
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadImage(string fileName)
        {
            var stream = await _blobStorageService.DownloadFileAsync(fileName);

            // Retorna o stream da imagem como FileStreamResult
            return new FileStreamResult(stream, "image/jpeg");
        }

        // Endpoint para listar todos os pedidos
        [HttpGet]
        public IActionResult GetAllPedidos()
        {
            var pedidos = _pedidoService.GetAllPedidos();
            if (pedidos == null || !pedidos.Any())
            {
                return NotFound(new { Message = "Nenhum pedido encontrado." });
            }

            var pedidosDto = pedidos.Select(p => new
            {
                p.Id,
                p.DataPedido,
                p.DataPretendida,
                ClienteCargaNome = p.ClienteCarga?.Nome ?? string.Empty,
                ClienteDescargaNome = p.ClienteDescarga?.Nome ?? string.Empty,
                VeiculoMatricula = p.Veiculo?.Matricula ?? string.Empty
            }).ToList();

            return Ok(pedidosDto);
        }

        [HttpGet("details/{id}")]
        public IActionResult GetPedidoDetails(int id)
        {
            var pedido = _pedidoService.GetPedidoById(id);
            if (pedido == null)
            {
                return NotFound(new { Message = $"Pedido com ID {id} não encontrado." });
            }

            var pedidoDto = new
            {
                pedido.Id,
                pedido.DataPedido,
                pedido.DataPretendida,
                VeiculoDescricao = pedido.Veiculo?.Matricula ?? string.Empty,
                MarcaVeiculo = pedido.Veiculo?.Modelo?.Marca?.Nome ?? string.Empty,
                MotoristaNome = pedido.Motorista?.Name ?? string.Empty,
                ClienteCargaNome = pedido.ClienteCarga?.Nome ?? string.Empty,
                ClienteDescargaNome = pedido.ClienteDescarga?.Nome ?? string.Empty,
                ClienteDescargaLocal = pedido.ClienteDescarga?.Local ?? string.Empty,
                ClienteCargaLocal = pedido.ClienteCarga?.Local ?? string.Empty,
                HoraInicio = pedido.HoraInicio,
                HoraFinal = pedido.HoraFinal,
                Fotos = pedido.FotoPedidos?.Select(f => new FotoDto
                {
                    UrlFoto = f.UrlFoto,                    
                    TipoFoto = f.TipoFotoVeiculo,
                    NomeArquivo = f.NomeArquivo
                }).ToList() ?? new List<FotoDto>()
            };

            return Ok(pedidoDto);
        }





        // Endpoint para buscar um pedido específico pelo Id
        [HttpGet("{id}")]
        public IActionResult GetPedido(int id)
        {
            var pedido = _pedidoService.GetPedidoById(id); // Busca o pedido pelo ID
            if (pedido == null)
            {
                return NotFound(new { Message = $"Pedido com ID {id} não encontrado." });
            }

            var pedidoDto = new
            {
                pedido.Id,
                pedido.DataPedido,
                pedido.HoraPedido,
                pedido.DataPretendida,
                VeiculoDescricao = pedido.Veiculo?.Matricula ?? string.Empty, // Descrição do veículo (ou Matricula, se preferir)
                MarcaVeiculo = pedido.Veiculo?.Modelo?.Marca?.Nome ?? string.Empty, // Marca do veículo
                MotoristaNome = pedido.Motorista?.Name ?? string.Empty, // Nome do motorista
                ClienteCargaNome = pedido.ClienteCarga?.Nome ?? string.Empty, // Nome do cliente de carga
                ClienteDescargaNome = pedido.ClienteDescarga?.Nome ?? string.Empty, // Nome do cliente de descarga
                Fotos = pedido.FotoPedidos?.Select(f => new FotoDto
                {
                    UrlFoto = f.UrlFoto,
                    TipoFoto = f.TipoFotoVeiculo,
                    NomeArquivo = f.NomeArquivo
                }).ToList() ?? new List<FotoDto>() // Ajuste o tipo aqui
            };

            return Ok(pedidoDto);
        }
    }
}
