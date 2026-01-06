using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Google;
using WebAppSystems.Data;
using WebAppSystemsTransp.Models;
using Microsoft.EntityFrameworkCore;


public class ImportacaoController : Controller
{
    private readonly FipeService _fipeService;
    private readonly WebAppSystemsContext _context;

    public ImportacaoController(WebAppSystemsContext context, FipeService fipeService)
    {
        _context = context;
        _fipeService = fipeService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Importar()
    {
        // Buscar as marcas da API FIPE
        var marcas = await _fipeService.ObterMarcasAsync();

        // Carregar todas as marcas e modelos existentes no banco de dados
        //var marcasExistentes = _context.Marca.ToList();
        //var modelosExistentes = _context.Modelo.ToList();

        //var marcasParaAdicionar = new List<Marca>();
        //var modelosParaAdicionar = new List<Modelo>();

        foreach (var marca in marcas)
        {
            if (!_context.Marca.Any(m => m.Nome == marca.Nome))
            {
                _context.Marca.Add(marca);
                await _context.SaveChangesAsync(); // Salva e gera o ID da marca
            }

            // Obtém a marca salva com o ID gerado pelo banco
            var marcaSalva = await _context.Marca.FirstOrDefaultAsync(m => m.Nome == marca.Nome);
            if (marcaSalva == null) continue; // Garante que a marca foi salva

            try
            {
//                var modelos = await _fipeService.ObterModelosPorMarcaAsync(marca.CodigoFipe); // Busca os modelos da API
                var modelos = await _fipeService.ObterModelosPorMarcaAsync(int.Parse(marcaSalva.CodigoFipe));


                foreach (var modelo in modelos)
                {
                    if (!_context.Modelo.Any(m => m.Nome == modelo.Nome)) // Usa Nome para evitar duplicidade
                    {
                        modelo.MarcaId = marcaSalva.Id; // Usa o ID gerado no banco!
                        _context.Modelo.Add(modelo);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["Erro"] = $"Erro ao buscar modelos para a marca {marca.Nome}.";
            }
        }







        return RedirectToAction("Index");
    }




}
