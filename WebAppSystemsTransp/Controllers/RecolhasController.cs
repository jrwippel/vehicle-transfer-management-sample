using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Newtonsoft.Json;
using WebAppSystems.Data;
using WebAppSystems.Helper;
using WebAppSystems.Models;
using WebAppSystems.Models.Enums;
using WebAppSystemsTransp.Models;
using WebAppSystemsTransp.Models.Dto;
using WebAppSystemsTransp.Models.Enums;
using WebAppSystemsTransp.Models.ViewModels;
using WebAppSystemsTransp.Services;

namespace WebAppSystemsTransp.Controllers
{
    public class RecolhasController : Controller
    {
        private readonly WebAppSystemsContext _context;
        private readonly ISessao _isessao;
        private readonly IEmail _email;
        private readonly PedidoPdfService _pedidoPdfService;
        private readonly BlobStorageService _blobStorageService;


        public RecolhasController(WebAppSystemsContext context, ISessao isessao, IEmail email, PedidoPdfService pedidoPdfService, BlobStorageService blobStorageService)
        {
            _context = context;
            _isessao = isessao;
            _email = email;
            _pedidoPdfService = pedidoPdfService;
            _blobStorageService = blobStorageService;
        }

        public IActionResult Index()
        {
            Attorney usuario = _isessao.BuscarSessaoDoUsuario();

            IQueryable<Pedido> query = _context.Pedido
                .Where(p => !p.HoraInicio.HasValue || !p.HoraFinal.HasValue); // Exclui pedidos concluídos

            if (usuario.Perfil != ProfileEnum.Admin)
            {
                // Filtra apenas os pedidos do motorista logado
                query = query.Where(p => p.MotoristaId == usuario.Id);
            }

            // Carregar dados de pedidos e contar as fotos de carga e descarga
            var pedidos = query
                .ToList() // Move para execução no lado do cliente
                .Select(p => new PedidoViewModel
                {
                    Id = p.Id,
                    DataPretendida = p.DataPretendida,
                    HoraInicio = p.HoraInicio,
                    HoraFinal = p.HoraFinal,
                    // Conta as fotos de carga e descarga
                    PossuiTodasFotosCarga = _context.FotoPedido.Count(f => f.PedidoId == p.Id && f.TipoPedido == TipoFotoPedido.Carga) >= 6,
                    PossuiTodasFotosDescarga = _context.FotoPedido.Count(f => f.PedidoId == p.Id && f.TipoPedido == TipoFotoPedido.Descarga) >= 1

                })
                .ToList();

            return View(pedidos);
        }

        [HttpGet]
        public JsonResult ObterDetalhes(int id)
        {
            var pedido = _context.Pedido
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    id = p.Id,
                    placaVeiculo = p.Veiculo.Matricula,
                    modelo = p.Veiculo.Modelo.Nome,
                    clienteCarga = p.ClienteCarga.Nome,
                    enderecoCarga = p.ClienteCarga.Local,
                    clienteDescarga = p.ClienteDescarga.Nome,
                    enderecoDescarga = p.ClienteDescarga.Local,
                    TipoPedido = p.TipoPedido.ToString(), // Converte enum para string
                    adicionalCarga = p.AdicionalCarga,
                    adicionalDescarga = p.AdicionalDescarga,
                    kmInicial = p.KmInicial
                })
                .FirstOrDefault();

            if (pedido == null)
                return Json(new { error = "Pedido não encontrado" });

            return Json(pedido);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var pedido = await _context.Pedido.FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Garanta que as horas sejam válidas
            pedido.HoraInicio = pedido.HoraInicio == default ? TimeSpan.Zero : pedido.HoraInicio;
            pedido.HoraFinal = pedido.HoraFinal == default ? TimeSpan.Zero : pedido.HoraFinal;

            return View(pedido);
        }

        public async Task<IActionResult> DownloadFotoPedido(int id)
        {
            var foto = _context.FotoPedido.Find(id); // Busca a foto no banco
            if (foto == null || string.IsNullOrEmpty(foto.UrlFoto))
            {
                return NotFound();
            }

            try
            {
                // 🔹 Baixa o arquivo do Blob Storage
                var stream = await _blobStorageService.DownloadFileAsync(foto.UrlFoto); // Aqui você pode passar a URL ou o nome do arquivo
                var fileName = Path.GetFileName(foto.UrlFoto); // Obtém o nome do arquivo da URL

                // 🔹 Retorna o arquivo para download
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                // Caso haja erro no download
                return BadRequest($"Erro ao tentar baixar o arquivo: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return NotFound();
            }

            // Carregar o pedido existente do banco de dados
            var pedidoExistente = await _context.Pedido.FindAsync(id);
            if (pedidoExistente == null)
            {
                return NotFound();
            }

            // Atualizar apenas os campos desejados
            pedidoExistente.DataExecucao = pedido.DataExecucao;
            pedidoExistente.HoraInicio = pedido.HoraInicio;
            pedidoExistente.HoraFinal = pedido.HoraFinal;

            try
            {
                // Marcar os campos atualizados para serem salvos no banco
                _context.Update(pedidoExistente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(pedido.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedido.Any(e => e.Id == id);
        }


        [HttpPost]
        public async Task<IActionResult> Start([FromForm] InicioRecolhaDto inicioDto)
        {
            if (inicioDto == null || inicioDto.PedidoId <= 0)
            {
                return BadRequest("Dados de entrada inválidos.");
            }

            var pedido = await _context.Pedido
                .Include(p => p.ClienteCarga)
                .Include(p => p.ClienteDescarga)
                .FirstOrDefaultAsync(p => p.Id == inicioDto.PedidoId);

            if (pedido == null)
            {
                return NotFound("Pedido não encontrado.");
            }

            try
            {
                pedido.DataExecucao = DateTime.Now.Date;
                pedido.HoraInicio = DateTime.Now.TimeOfDay;
                pedido.KmInicial = inicioDto.KmInicial;
                pedido.CombustivelInicial = (int)inicioDto.CombustivelInicial;
                pedido.TrianguloHomologado = inicioDto.TrianguloHomologado;
                pedido.ColeteHomologado = inicioDto.ColeteHomologado;
                pedido.DocumentoSeguro = inicioDto.DocumentoSeguro;
                pedido.DocumentoVeiculo = inicioDto.DocumentoVeiculo;

                pedido.NomePessoaCar = inicioDto.NomePessoaCar;
                pedido.EmailPessoaCar = inicioDto.EmailPessoaCar;
                pedido.TelPessoaCar = inicioDto.TelPessoaCar;

                // Função para limpar o prefixo da base64
                string RemoveBase64Prefix(string base64String)
                {
                    if (string.IsNullOrEmpty(base64String))
                        return base64String;

                    var prefix = "data:image/png;base64,";
                    if (base64String.StartsWith(prefix))
                    {
                        return base64String.Substring(prefix.Length); // Remove o prefixo
                    }

                    return base64String;
                }

                // Salvar as assinaturas como FotoPedido
                if (!string.IsNullOrEmpty(inicioDto.AssMotoristaRecolha))
                {
                    var assinaturaMotorista = new FotoPedido
                    {
                        PedidoId = inicioDto.PedidoId,
                        TipoPedido = TipoFotoPedido.Carga, // Assinatura
                        TipoFotoVeiculo = TipoFotoVeiculo.AssMotorista,
                        NomeArquivo = $"{Guid.NewGuid()}.jpg",  // Nome único para a imagem
                        DataUpload = DateTime.UtcNow
                    };

                    // Converte a string base64 da assinatura para bytes
                    var base64Data = RemoveBase64Prefix(inicioDto.AssMotoristaRecolha);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    // Faz o upload da imagem para o Blob Storage e armazena a URL
                    assinaturaMotorista.UrlFoto = await _blobStorageService.UploadFileAsync(assinaturaMotorista.NomeArquivo, new MemoryStream(imageBytes));

                    // Adiciona a foto do motorista ao contexto
                    _context.FotoPedido.Add(assinaturaMotorista);
                }

                if (!string.IsNullOrEmpty(inicioDto.AssClienteRecolha))
                {
                    var assinaturaCliente = new FotoPedido
                    {
                        PedidoId = inicioDto.PedidoId,
                        TipoPedido = TipoFotoPedido.Carga, // Assinatura
                        TipoFotoVeiculo = TipoFotoVeiculo.AssCliente,
                        NomeArquivo = $"{Guid.NewGuid()}.jpg",  // Nome único para a imagem
                        DataUpload = DateTime.UtcNow
                    };

                    // Converte a string base64 da assinatura para bytes
                    var base64Data = RemoveBase64Prefix(inicioDto.AssClienteRecolha);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    // Faz o upload da imagem para o Blob Storage e armazena a URL
                    assinaturaCliente.UrlFoto = await _blobStorageService.UploadFileAsync(assinaturaCliente.NomeArquivo, new MemoryStream(imageBytes));

                    // Adiciona a foto do cliente ao contexto
                    _context.FotoPedido.Add(assinaturaCliente);
                }


                // Novos campos
                if (!string.IsNullOrEmpty(inicioDto.ObservacaoCarga))
                {
                    pedido.ObservacaoCarga = inicioDto.ObservacaoCarga;
                }

                // Tratar o Tempo de Espera da Carga (em formato "HH:mm")
                if (!string.IsNullOrEmpty(inicioDto.TempoEsperaCarga))
                {
                    TimeSpan tempoEspera;
                    if (TimeSpan.TryParseExact(inicioDto.TempoEsperaCarga, "hh\\:mm", null, out tempoEspera))
                    {
                        pedido.TempoEsperaCarga = tempoEspera;
                    }
                    else
                    {
                        return BadRequest("Formato de Tempo de Espera da Carga inválido.");
                    }
                }

                _context.Update(pedido);
                await _context.SaveChangesAsync();

                // Se houver avarias no JSON, desserializar e salvar
                if (!string.IsNullOrEmpty(inicioDto.Avarias))
                {
                    try
                    {
                        var avarias = JsonConvert.DeserializeObject<List<Avaria>>(inicioDto.Avarias);

                        if (avarias != null && avarias.Any())
                        {
                            foreach (var avaria in avarias)
                            {
                                avaria.PedidoId = inicioDto.PedidoId;
                                _context.Avaria.Add(avaria);
                            }
                            _context.SaveChanges();
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Erro ao desserializar as avarias: {ex.Message}");
                    }
                }

                // Enviar e-mail para o cliente informando que a recolha foi iniciada
                if (!string.IsNullOrEmpty(pedido.ClienteCarga?.Email))
                {
                    string assunto = "Início do Processo de Recolha do Veículo";
                    string mensagem = $@"
        Prezado Cliente,<br/><br/>
        Informamos que o processo de recolha do seu veículo foi iniciado.<br/>
        <strong>Pedido ID:</strong> {pedido.Id}<br/>
        <strong>Data de Execução:</strong> {pedido.DataExecucao:dd/MM/yyyy}<br/>
        <strong>Hora de Início:</strong> {pedido.HoraInicio}<br/><br/>
        <strong>Pessoa Responsável Carga:</strong> {pedido.NomePessoaCar}<br/><br/>
        Em caso de dúvidas, entre em contato com nosso suporte.<br/><br/>
        Atenciosamente,<br/>
        Equipe de Atendimento";

                    // Gerar o PDF
                    string caminhoPdf = Path.Combine(Path.GetTempPath(), $"Pedido_{pedido.Id}_GuiaTransporte.pdf");

                    // Gerar o PDF usando o serviço GeneratePdfAsync
                    var fileStreamResult = await _pedidoPdfService.GeneratePdfAsync(pedido.Id);

                    if (fileStreamResult != null)
                    {
                        // Salvar o conteúdo do FileStreamResult em um arquivo temporário
                        using (var fileStream = new FileStream(caminhoPdf, FileMode.Create, FileAccess.Write))
                        {
                            await fileStreamResult.FileStream.CopyToAsync(fileStream);
                        }

                        // Enviar o e-mail com o anexo (PDF)
                        _ = Task.Run(async () =>
                        {
                            bool emailEnviado = await _email.EnviarAsync(pedido.ClienteCarga.Email, assunto, mensagem, caminhoPdf);
                            if (!emailEnviado)
                            {
                                Console.WriteLine("Erro ao enviar e-mail para o cliente.");
                            }
                            else
                            {
                                Console.WriteLine("E-mail enviado com sucesso(Cliente).");
                            }
                            bool emailEnviadoRes = await _email.EnviarAsync(pedido.EmailPessoaCar, assunto, mensagem, caminhoPdf);
                            if (!emailEnviadoRes)
                            {
                                Console.WriteLine("Erro ao enviar e-mail para o responsável.");
                            }
                            else
                            {
                                Console.WriteLine("E-mail enviado com sucesso(Responsável).");
                            }

                        });
                    }
                    else
                    {
                        Console.WriteLine("Erro ao gerar o PDF.");
                    }
                }

                return Ok(new { message = "Início da recolha registrado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registrar início da recolha: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Stop([FromForm] FinalRecolhaDto finalDto)
        {
            if (finalDto == null || finalDto.PedidoId <= 0)
            {
                return BadRequest("Dados de entrada inválidos.");
            }

            var pedido = await _context.Pedido
                .Include(p => p.ClienteCarga)
                .Include(p => p.ClienteDescarga)
                .Include(p => p.Veiculo)
                .Include(p => p.Motorista)
                .FirstOrDefaultAsync(p => p.Id == finalDto.PedidoId);

            if (pedido == null)
            {
                return NotFound("Pedido não encontrado.");
            }

            try
            {
                pedido.HoraFinal = DateTime.Now.TimeOfDay;
                pedido.KmFinal = finalDto.KmFinal;
                pedido.CombustivelFinal = (int)finalDto.CombustivelFinal;

                pedido.NomePessoaDes = finalDto.NomePessoaDes;
                pedido.EmailPessoaDes = finalDto.EmailPessoaDes;
                pedido.TelPessoaDes = finalDto.TelPessoaDes;



                // Salvar as assinaturas como FotoPedido
                if (!string.IsNullOrEmpty(finalDto.AssMotoristaEntrega))
                {
                    var assinaturaMotorista = new FotoPedido
                    {
                        PedidoId = finalDto.PedidoId,
                        TipoPedido = TipoFotoPedido.Descarga, // Assinatura de entrega
                        TipoFotoVeiculo = TipoFotoVeiculo.AssMotorista,
                        NomeArquivo = $"{Guid.NewGuid()}.jpg",  // Nome único para a imagem
                        DataUpload = DateTime.UtcNow
                    };

                    // Converte a string base64 da assinatura para bytes
                    var base64Data = RemoveBase64Prefix(finalDto.AssMotoristaEntrega);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    // Faz o upload da imagem para o Blob Storage e armazena a URL
                    assinaturaMotorista.UrlFoto = await _blobStorageService.UploadFileAsync(assinaturaMotorista.NomeArquivo, new MemoryStream(imageBytes));

                    // Adiciona a foto do motorista ao contexto
                    _context.FotoPedido.Add(assinaturaMotorista);
                }

                if (!string.IsNullOrEmpty(finalDto.AssClienteEntrega))
                {
                    var assinaturaCliente = new FotoPedido
                    {
                        PedidoId = finalDto.PedidoId,
                        TipoPedido = TipoFotoPedido.Descarga, // Assinatura de entrega
                        TipoFotoVeiculo = TipoFotoVeiculo.AssCliente,
                        NomeArquivo = $"{Guid.NewGuid()}.jpg",  // Nome único para a imagem
                        DataUpload = DateTime.UtcNow
                    };

                    // Converte a string base64 da assinatura para bytes
                    var base64Data = RemoveBase64Prefix(finalDto.AssClienteEntrega);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    // Faz o upload da imagem para o Blob Storage e armazena a URL
                    assinaturaCliente.UrlFoto = await _blobStorageService.UploadFileAsync(assinaturaCliente.NomeArquivo, new MemoryStream(imageBytes));

                    // Adiciona a foto do cliente ao contexto
                    _context.FotoPedido.Add(assinaturaCliente);
                }


                // Novos campos
                if (!string.IsNullOrEmpty(finalDto.ObservacaoDescarga))
                {
                    pedido.ObservacaoDescarga = finalDto.ObservacaoDescarga;
                }

                // Tratar o Tempo de Espera da Carga (em formato "HH:mm")
                if (!string.IsNullOrEmpty(finalDto.TempoEsperaDescarga))
                {
                    if (TimeSpan.TryParseExact(finalDto.TempoEsperaDescarga, "hh\\:mm", null, out TimeSpan tempoEspera))
                    {
                        pedido.TempoEsperaDescarga = tempoEspera;
                    }
                    else
                    {
                        return BadRequest("Formato de Tempo de Espera da Carga inválido.");
                    }
                }

                _context.Update(pedido);
                await _context.SaveChangesAsync();

                // Enviar e-mail para o cliente informando que a recolha foi finalizada
                if (!string.IsNullOrEmpty(pedido.ClienteDescarga?.Email))
                {
                    string assunto = "Finalizado o Processo de Recolha e Entrega do Veículo";
                    string mensagem = $@"
        Prezado Cliente,<br/><br/>
        Informamos que o processo de recolha e entrega do seu veículo foi finalizado.<br/>
        <strong>Pedido ID:</strong> {pedido.Id}<br/>
        <strong>Data de Execução:</strong> {pedido.DataExecucao:dd/MM/yyyy}<br/>
        <strong>Hora de Início:</strong> {pedido.HoraInicio}<br/><br/>
        <strong>Hora de Finalização:</strong> {pedido.HoraFinal}<br/><br/>

        <strong>Pessoa Responsável Carga:</strong> {pedido.NomePessoaCar}<br/><br/>
        <strong>Pessoa Responsável Descarga:</strong> {pedido.NomePessoaDes}<br/><br/>      

        Em anexo segue guia de transporte com detalhes da transferência
        Em caso de dúvidas, entre em contato com nosso suporte.<br/><br/>
        Atenciosamente,<br/>
        Equipe de Atendimento";

                    // Gerar o PDF
                    string caminhoPdf = Path.Combine(Path.GetTempPath(), $"Pedido_{pedido.Id}_GuiaTransporte.pdf");

                    // Gerar o PDF usando o serviço GeneratePdfAsync
                    var fileStreamResult = await _pedidoPdfService.GeneratePdfAsync(pedido.Id);

                    if (fileStreamResult != null)
                    {
                        // Salvar o conteúdo do FileStreamResult em um arquivo temporário
                        using (var fileStream = new FileStream(caminhoPdf, FileMode.Create, FileAccess.Write))
                        {
                            await fileStreamResult.FileStream.CopyToAsync(fileStream);
                        }

                        // Enviar o e-mail com o anexo (PDF)
                        _ = Task.Run(async () =>
                        {
                            bool emailEnviado = await _email.EnviarAsync(pedido.ClienteDescarga.Email, assunto, mensagem, caminhoPdf);
                            if (!emailEnviado)
                            {
                                Console.WriteLine("Erro ao enviar e-mail para o cliente.");
                            }
                            else
                            {
                                Console.WriteLine("E-mail enviado com sucesso.");
                            }
                            bool emailEnviadoRes = await _email.EnviarAsync(pedido.EmailPessoaDes, assunto, mensagem, caminhoPdf);
                            if (!emailEnviadoRes)
                            {
                                Console.WriteLine("Erro ao enviar e-mail para o responsável pelo recebimento.");
                            }
                            else
                            {
                                Console.WriteLine("E-mail enviado com sucesso(Resposável).");
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine("Erro ao gerar o PDF.");
                    }
                }

                return Ok(new { message = "Finalização da entrega registrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registrar finalização da entrega(Back): {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarFotoPedido(int pedidoId, string tipoFotoVeiculo, string imageData, int tipoPedido)
        {
            try
            {
                // Remove a parte "data:image/jpeg;base64," da string base64
                var base64Data = imageData.Split(',')[1];

                // Converte a string base64 para um array de bytes
                var imageBytes = Convert.FromBase64String(base64Data);

                // Converte o tipoFotoVeiculo de string para o tipo enum
                var tipoFotoVeiculoEnum = (TipoFotoVeiculo)Enum.Parse(typeof(TipoFotoVeiculo), tipoFotoVeiculo);

                // Obter o pedido para verificar as horas
                var pedido = _context.Pedido.FirstOrDefault(p => p.Id == pedidoId);
                if (pedido == null)
                {
                    return NotFound(new { message = "Pedido não encontrado!" });
                }

                // Definir o tipo de pedido com base nas horas
                TipoFotoPedido tipoFoto;
                if (pedido.HoraInicio.HasValue && !pedido.HoraFinal.HasValue)
                {
                    tipoFoto = TipoFotoPedido.Descarga;  // Tipo "Descarga" se HoraInicio tem valor e HoraFinal não
                }
                else if (!pedido.HoraInicio.HasValue && !pedido.HoraFinal.HasValue)
                {
                    tipoFoto = TipoFotoPedido.Carga;  // Tipo "Carga" se ambas HoraInicio e HoraFinal não têm valor
                }
                else
                {
                    tipoFoto = TipoFotoPedido.Carga;  // Valor padrão "Carga" caso não satisfaça as outras condições
                }

                // Verificar se já existe uma foto para o pedido e tipo selecionado
                var fotoExistente = _context.FotoPedido
                    .FirstOrDefault(f => f.PedidoId == pedidoId && f.TipoFotoVeiculo == tipoFotoVeiculoEnum && f.TipoPedido == tipoFoto);

                if (fotoExistente != null && fotoExistente.TipoFotoVeiculo != TipoFotoVeiculo.Diversos)
                {
                    // Atualizar a foto existente
                    fotoExistente.NomeArquivo = $"{Guid.NewGuid()}.jpg"; // Novo nome para evitar conflito
                    fotoExistente.DataUpload = DateTime.UtcNow;
                    fotoExistente.TipoPedido = tipoFoto; // Atualizar com o tipo de foto correto

                    // Faz o upload da imagem para o Blob Storage e armazena a URL
                    fotoExistente.UrlFoto = await _blobStorageService.UploadFileAsync(fotoExistente.NomeArquivo, new MemoryStream(imageBytes));

                    _context.FotoPedido.Update(fotoExistente);
                }
                else
                {
                    // Caso não exista, criar uma nova foto
                    var fotoPedido = new FotoPedido
                    {
                        PedidoId = pedidoId,
                        TipoFotoVeiculo = tipoFotoVeiculoEnum,
                        NomeArquivo = $"{Guid.NewGuid()}.jpg", // Nome único para o arquivo
                        DataUpload = DateTime.UtcNow,
                        TipoPedido = tipoFoto, // Define o tipo da foto com base na lógica
                    };

                    // Faz o upload da imagem para o Blob Storage e armazena a URL
                    fotoPedido.UrlFoto = await _blobStorageService.UploadFileAsync(fotoPedido.NomeArquivo, new MemoryStream(imageBytes));

                    _context.FotoPedido.Add(fotoPedido);
                }

                // Salva as alterações no banco de dados
                await _context.SaveChangesAsync();

                return Ok(new { message = "Foto salva com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao salvar a foto", error = ex.Message });
            }
        }



        // Função para limpar o prefixo da base64
        private string RemoveBase64Prefix(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return base64String;

            var prefix = "data:image/png;base64,";
            if (base64String.StartsWith(prefix))
            {
                return base64String.Substring(prefix.Length); // Remove o prefixo
            }

            return base64String;
        }

        [HttpGet]
        public IActionResult ObterPedido(int pedidoId)
        {
            var pedido = _context.Pedido
                .Where(p => p.Id == pedidoId)
                .Select(p => new
                {
                    p.HoraInicio,
                    p.HoraFinal
                })
                .FirstOrDefault();

            if (pedido == null)
            {
                return Json(new { erro = "Pedido não encontrado." });
            }

            // Retorna um booleano indicando a lógica desejada
            var isEstacionamento = pedido.HoraInicio != null && pedido.HoraFinal == null;

            return Json(new { isEstacionamento });
        }




    }
}

