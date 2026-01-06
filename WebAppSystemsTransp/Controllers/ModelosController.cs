using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystems.Helper;
using WebAppSystems.Models;
using WebAppSystems.Services;
using WebAppSystemsTransp.Models;
using WebAppSystemsTransp.Services;
using static WebAppSystems.Helper.Sessao;

namespace WebAppSystemsTransp.Controllers
{
    public class ModelosController : Controller
    {
        private readonly WebAppSystemsContext _context;
        private readonly ModeloService _modeloService;
        private readonly ISessao _isessao;
        private readonly AttorneyService _attorneyService;


        public ModelosController(WebAppSystemsContext context, ModeloService modeloService, ISessao isessao, AttorneyService attorneyService)
        {
            _context = context;
            _modeloService = modeloService;
            _isessao = isessao;
            _attorneyService = attorneyService;
        }

        // GET: Modeloes
        public async Task<IActionResult> Index()
        {
            //var webAppSystemsContext = _context.Modelo.Include(m => m.Marca);
            //return View(await webAppSystemsContext.ToListAsync());


            try
            {
                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;
                return View(Enumerable.Empty<Modelo>()); // Retorna um modelo vazio
            }
            catch (SessionExpiredException)
            {
                TempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";
                return RedirectToAction("Index", "Login");
            }

        }

        // GET: Modeloes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Modelo == null)
            {
                return NotFound();
            }

            var modelo = await _context.Modelo
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (modelo == null)
            {
                return NotFound();
            }

            return View(modelo);
        }

        // GET: Modeloes/Create
        public IActionResult Create()
        {
            ViewData["MarcaId"] = new SelectList(_context.Marca, "Id", "Nome"); // Aqui, 'Nome' é o campo de descrição da marca
            return View();
        }

        // POST: Modeloes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,MarcaId")] Modelo modelo)
        {
          //  if (ModelState.IsValid)
          //  {
                _context.Add(modelo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
         //   }
          //  ViewData["MarcaId"] = new SelectList(_context.Marca, "Id", "Id", modelo.MarcaId);
          //  return View(modelo);
        }

        // GET: Modeloes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Modelo == null)
            {
                return NotFound();
            }

            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo == null)
            {
                return NotFound();
            }            
            ViewData["MarcaId"] = new SelectList(_context.Marca, "Id", "Nome"); // Aqui, 'Nome' é o campo de descrição da marca

            return View(modelo);
        }

        // POST: Modeloes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,MarcaId")] Modelo modelo)
        {
            if (id != modelo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modelo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeloExists(modelo.Id))
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
            ViewData["MarcaId"] = new SelectList(_context.Marca, "Id", "Id", modelo.MarcaId);
            return View(modelo);
        }

        // GET: Modeloes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Modelo == null)
            {
                return NotFound();
            }

            var modelo = await _context.Modelo
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (modelo == null)
            {
                return NotFound();
            }

            return View(modelo);
        }

        // POST: Modeloes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Modelo == null)
            {
                return Problem("Entity set 'WebAppSystemsContext.Modelo'  is null.");
            }
            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo != null)
            {
                _context.Modelo.Remove(modelo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModeloExists(int id)
        {
            return (_context.Modelo?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<JsonResult> GetModelos(int draw, int start, int length, string search = "", int orderColumn = 0, string orderDir = "asc")
        {
            try
            {
                // Calcula a página atual
                int page = (start / length) + 1;

                // Configura filtros de pesquisa
                string searchValue = search?.Trim().ToLower();

                // Busca os registros com filtros e total de registros
                var (records, totalRecords) = await _modeloService.FindAllAsync(page, length, searchValue, orderColumn, orderDir);

                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;

                // Formata os dados no formato esperado pelo DataTables
                var result = new
                {
                    draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = records.Select(pr => new
                    {
                        pr.Nome,
                        Marca = pr.Marca.Nome, // Adicionei isso
                        EditLink = pr.Id == ViewBag.LoggedUserId
                            ? Url.Action("Edit", new { id = pr.Id })
                            : null,
                        DetailsLink = Url.Action("Details", new { id = pr.Id }),
                        DeleteLink = pr.Id == ViewBag.LoggedUserId
                            ? Url.Action("Delete", new { id = pr.Id })
                            : null
                    })
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                // Log do erro
                Console.WriteLine($"Erro ao buscar modelos: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

    }
}
