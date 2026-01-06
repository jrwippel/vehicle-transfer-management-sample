using Microsoft.AspNetCore.Mvc;
using WebAppSystems.Services;
using WebAppSystemsTransp.Models;
using WebAppSystemsTransp.Models.Dto;
using WebAppSystemsTransp.Models.Enums;
using WebAppSystemsTransp.Models.ViewModels; // ViewModel para passar dados à View
using WebAppSystemsTransp.Services;

public class ChecklistsController : Controller
{
    private readonly PedidoService _pedidoService;
    private readonly BlobStorageService _blobStorageService;
    private readonly FotoService _fotoService;

    public ChecklistsController(PedidoService pedidoService, BlobStorageService blobStorageService, FotoService fotoService)
    {
        _pedidoService = pedidoService;
        _blobStorageService = blobStorageService;
        _fotoService = fotoService;
    }

    public async Task<IActionResult> DownloadFotoPedido(int id)
    {
        // Buscar a foto usando o serviço
        var foto = await _fotoService.GetByIdAsync(id); // Use "await" para obter o resultado do método assíncrono
        if (foto == null || string.IsNullOrEmpty(foto.UrlFoto))
        {
            return NotFound(new { Message = "Foto não encontrada ou URL inválida." });
        }

        try
        {
            // Baixar o arquivo do Blob Storage
            var stream = await _blobStorageService.DownloadFileAsync(foto.UrlFoto); // Aqui você pode passar a URL ou o nome do arquivo
            var fileName = Path.GetFileName(foto.UrlFoto); // Obtém o nome do arquivo da URL

            // Retorna o arquivo para download
            return File(stream, "application/octet-stream", fileName); // Formato genérico para download
        }
        catch (Exception ex)
        {
            // Tratar erros no download
            return BadRequest(new { Message = $"Erro ao tentar baixar o arquivo: {ex.Message}" });
        }
    }



    [HttpGet]
    public IActionResult GetFotosCarga(int id)
    {
        var pedido = _pedidoService.GetPhotoPedidoById(id);
        if (pedido == null || pedido.FotoPedidos == null)
        {
            return NotFound(new { Message = $"Nenhuma foto encontrada para o pedido com ID {id}." });
        }

        var fotosDto = pedido.FotoPedidos
            //.Where(f => (f.TipoFotoVeiculo == TipoFotoVeiculo.AssMotorista || f.TipoFotoVeiculo == TipoFotoPedido.Descarga))
            .Select(f => new FotoDto
            {                
                Id = f.Id, // Certifique-se de que está populando o ID
                UrlFoto = f.UrlFoto,
                TipoFoto = f.TipoFotoVeiculo,
                NomeArquivo = f.NomeArquivo
            }).ToList();

        return View("FotosCarga", fotosDto); // Certifique-se de que o nome da view está correto
    }

    public IActionResult Carga(int id)
    {
        // Busca o pedido pelo ID usando o PedidoService
        var pedido = _pedidoService.GetPedidoById(id);

        if (pedido == null)
        {
            return NotFound(); // Retorna erro 404 caso o pedido não seja encontrado
        }

        // Verifica se a hora inicial já foi preenchida
        var isRegistroExistente = pedido.HoraInicio.HasValue;

        // Cria um DTO com as informações necessárias para a checklist
        var checklistDto = new ChecklistCargaDto
        {
            PedidoId = pedido.Id,
            HoraInicio = pedido.HoraInicio.HasValue
                ? (long)(pedido.HoraInicio.Value.TotalMilliseconds) // Hora existente do banco
                : 0, // Novo registro, hora será dinâmica
            KmInicial = pedido.KmInicial,
            CombustivelInicial = pedido.CombustivelInicial,
            TrianguloHomologado = pedido.TrianguloHomologado,
            ColeteHomologado = pedido.ColeteHomologado,
            DocumentoSeguro = pedido.DocumentoSeguro,
            DocumentoVeiculo = pedido.DocumentoVeiculo,
            IsRegistroExistente = isRegistroExistente // Define se o registro já existe
        };

        // Passa o DTO para a view
        return View(checklistDto);
    }


    [HttpPost]
    public IActionResult SalvarChecklist(ChecklistCargaDto checklistDto)
    {
        if (checklistDto == null)
        {
            return BadRequest("Dados inválidos.");
        }

        // Obter o pedido pelo ID
        var pedido = _pedidoService.GetPedidoById(checklistDto.PedidoId);
        if (pedido == null)
        {
            return NotFound($"Pedido com ID {checklistDto.PedidoId} não encontrado.");
        }

        // Atualizar os dados do pedido com as informações do checklist
        pedido.HoraInicio = DateTimeOffset.UtcNow.TimeOfDay; // Define a hora atual do sistema
        pedido.KmInicial = checklistDto.KmInicial;
        pedido.CombustivelInicial = checklistDto.CombustivelInicial;
        pedido.TrianguloHomologado = checklistDto.TrianguloHomologado;
        pedido.ColeteHomologado = checklistDto.ColeteHomologado;
        pedido.DocumentoSeguro = checklistDto.DocumentoSeguro;
        pedido.DocumentoVeiculo = checklistDto.DocumentoVeiculo;

        try
        {
            // Salvar alterações no banco
            _pedidoService.UpdatePedido(pedido);           
            return RedirectToAction("Details", "Pedidos", new { id = checklistDto.PedidoId });

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao salvar checklist: {ex.Message}");
        }
    }
}
