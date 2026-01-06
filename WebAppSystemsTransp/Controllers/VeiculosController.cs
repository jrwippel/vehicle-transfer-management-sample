using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystemsTransp.Models;

namespace WebAppSystemsTransp.Controllers
{
    public class VeiculosController : Controller
    {
        private readonly WebAppSystemsContext _context;

        public VeiculosController(WebAppSystemsContext context)
        {
            _context = context;
        }

        // GET: Veiculos
        public async Task<IActionResult> Index()
        {
            var veiculosComModelosEMarcas = await _context.Veiculo
                .Include(v => v.Modelo)        // Incluir a entidade Modelo
                    .ThenInclude(m => m.Marca) // Incluir a Marca associada ao Modelo
                .ToListAsync();

            return _context.Veiculo != null ?
                       View(veiculosComModelosEMarcas) :
                       Problem("Entity set 'WebAppSystemsContext.Veiculo' is null.");
        }



        // GET: Veiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Veiculo == null)
            {
                return NotFound();
            }

            var veiculo = await _context.Veiculo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (veiculo == null)
            {
                return NotFound();
            }

            return View(veiculo);
        }

        // GET: Veiculos/Create
        public IActionResult Create()
        {
            // Passa as marcas para o ViewBag
            ViewBag.Marcas = new SelectList(_context.Marca, "Id", "Nome"); // Marcas disponíveis           

            // Inicializa o ViewBag de modelos como vazio, já que não sabemos qual será a marca selecionada
            ViewBag.Modelos = new SelectList(Enumerable.Empty<object>());  // Inicializa modelos vazios


            return View();
        }

        [HttpGet]
        public IActionResult GetModelosPorMarca(int marcaId)
        {
            Console.WriteLine($"MarcaId recebido: {marcaId}"); // Log para depuração

            var modelos = _context.Modelo
                .Where(m => m.MarcaId == marcaId)
                .Select(m => new { m.Id, m.Nome })
                .ToList();

            return Json(modelos);
        }




        // POST: Veiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Veiculo veiculo)
        {
            // Verificar se já existe um veículo com a matrícula informada
            var veiculoExistente = await _context.Veiculo
                                                 .FirstOrDefaultAsync(v => v.Matricula == veiculo.Matricula);

            if (veiculoExistente != null)
            {
                // Se a matrícula já existir, adicionar uma mensagem de erro e retornar para a view
                ModelState.AddModelError("Matricula", "Já existe um veículo cadastrado com esta matrícula.");

                // Recarregar a ViewBag caso precise das marcas ou outros dados na view
                ViewBag.Marcas = new SelectList(_context.Marca, "Id", "Nome");

                // Retornar a view com o modelo e os erros
                return View(veiculo);
            }

            // Garantir que a coleção de pedidos não seja null
            if (veiculo.Pedidos == null)
            {
                veiculo.Pedidos = new List<Pedido>();
            }

            // Adicionar o novo veículo no contexto
            _context.Add(veiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: Veiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var veiculo = await _context.Veiculo
                .Include(v => v.Modelo)           // Incluir o modelo
                .ThenInclude(m => m.Marca)        // Incluir a marca associada ao modelo
                .FirstOrDefaultAsync(m => m.Id == id);

            if (veiculo == null)
            {
                return NotFound();
            }

            // Passar a lista de modelos para a ViewBag
            ViewBag.Modelos = new SelectList(_context.Modelo, "Id", "Nome", veiculo.ModeloId);

            // Passar a lista de marcas para a ViewBag (se necessário)
            ViewBag.Marcas = new SelectList(_context.Marca, "Id", "Nome", veiculo.Modelo.MarcaId);

            return View(veiculo);
        }


        // POST: Veiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ModeloId,Matricula")] Veiculo veiculo)
        {
            if (id != veiculo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(veiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VeiculoExists(veiculo.Id))
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
            return View(veiculo);
        }

        // GET: Veiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Veiculo == null)
            {
                return NotFound();
            }

            var veiculo = await _context.Veiculo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (veiculo == null)
            {
                return NotFound();
            }

            return View(veiculo);
        }

        // POST: Veiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Veiculo == null)
            {
                return Problem("Entity set 'WebAppSystemsContext.Veiculo'  is null.");
            }
            var veiculo = await _context.Veiculo.FindAsync(id);
            if (veiculo != null)
            {
                _context.Veiculo.Remove(veiculo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VeiculoExists(int id)
        {
            return (_context.Veiculo?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
