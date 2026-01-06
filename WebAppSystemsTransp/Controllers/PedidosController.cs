using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystems.Models.Enums;
using WebAppSystemsTransp.Models;
using WebAppSystemsTransp.Models.Dto;
using WebAppSystemsTransp.Models.Enums;
using iText.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.draw;
using System;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using WebAppSystemsTransp.Services;
using System.Text.RegularExpressions;




namespace WebAppSystemsTransp.Controllers
{
    public class PedidosController : Controller
    {
        private readonly WebAppSystemsContext _context;
        private readonly PedidoPdfService _pedidoPdfService;

        public PedidosController(WebAppSystemsContext context, PedidoPdfService pedidoPdfService)
        {
            _context = context;
            _pedidoPdfService = pedidoPdfService;
        }



        // Método auxiliar para converter valores 1 a 5 para suas respectivas descrições
        string ConverterCombustivel(int? valor)
        {
            return valor switch
            {
                1 => "Reserva",
                2 => "1/4",
                3 => "1/2",
                4 => "3/4",
                5 => "Cheio (F)",
                _ => "Desconhecido"
            };
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarCliente([FromForm] Cliente cliente)
        {
            if (cliente == null ||
                string.IsNullOrWhiteSpace(cliente.Nome) ||
                string.IsNullOrWhiteSpace(cliente.Local) ||
                string.IsNullOrWhiteSpace(cliente.Email) ||
                string.IsNullOrWhiteSpace(cliente.Telefone))
            {
                return BadRequest("Todos os campos são obrigatórios.");
            }

            try
            {
                _context.Cliente.Add(cliente);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    clienteId = cliente.Id,
                    nome = cliente.Nome,
                    local = cliente.Local
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao cadastrar cliente: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CadastrarVeiculo([FromForm] string matricula, [FromForm] int modeloId, [FromForm] bool matriculaEstrangeira)
        {
            // Verifica se os campos obrigatórios não estão vazios
            if (string.IsNullOrWhiteSpace(matricula) || modeloId == 0)
            {
                return Json(new { success = false, message = "Campos obrigatórios não preenchidos." });
            }

            // Se não for matrícula estrangeira, validar formato XX-XX-XX
            if (!matriculaEstrangeira)
            {
                var regex = new Regex(@"^[A-Z0-9]{2}-[A-Z0-9]{2}-[A-Z0-9]{2}$", RegexOptions.IgnoreCase);
                if (!regex.IsMatch(matricula))
                {
                    return Json(new { success = false, message = "Formato de matrícula inválido. Use XX-XX-XX, onde X pode ser letra ou número." });
                }

            }

            // Verifica se a matrícula já existe no banco de dados
            if (_context.Veiculo.Any(v => v.Matricula == matricula.ToUpper()))
            {
                return Json(new { success = false, message = "Matrícula já cadastrada." });
            }

            // Criação do novo veículo com os dados recebidos
            var novoVeiculo = new Veiculo
            {
                Matricula = matricula.ToUpper(), // Converte a matrícula para maiúsculo
                ModeloId = modeloId // Associa o ID do modelo ao veículo
            };

            // Valida e salva no banco de dados
            if (ModelState.IsValid)
            {
                _context.Veiculo.Add(novoVeiculo);
                _context.SaveChanges();

                // Retorna uma resposta de sucesso com dados do veículo cadastrado
                return Json(new
                {
                    success = true,
                    message = "Veículo cadastrado com sucesso!",
                    matricula = novoVeiculo.Matricula,
                    veiculoId = novoVeiculo.Id // Retorna o ID do veículo gerado
                });
            }

            // Caso algo não tenha sido válido, retorna uma resposta de erro
            return Json(new { success = false, message = "Erro ao salvar veículo." });
        }

        [HttpGet]
        public JsonResult GetMarcas()
        {
            var marcas = _context.Marca
                .Select(m => new { id = m.Id, nome = m.Nome }) // Retorna ID e Nome
                .Distinct()
                .ToList();

            return Json(marcas);
        }



        [HttpGet]
        public JsonResult PesquisarVeiculo(string placa)
        {
            var veiculos = _context.Veiculo
                .Where(v => v.Matricula.Contains(placa))
                .Select(v => new { id = v.Id, matricula = v.Matricula, modelo = v.Modelo.Nome })
                .ToList();

            return Json(veiculos); // Retorna sempre um array (vazio ou com itens)
        }

        [HttpGet]
        public JsonResult PesquisarCliente(string nome)
        {
            var clientes = _context.Cliente
                .Where(v => v.Nome.Contains(nome))
                .Select(v => new { id = v.Id, nome = v.Nome, local = v.Local })
                .ToList();

            return Json(clientes); // Retorna sempre um array (vazio ou com itens)
        }


        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedido
                .Include(p => p.Veiculo) // Garante que a propriedade Veiculo será carregada
                    .ThenInclude(v => v.Modelo)
               // .Include(p => p.Veiculo) // Garante que a propriedade Veiculo será carregada
               // .Include(p => p.FotoPedidos) // Inclui as fotos associadas ao pedido                                
                .Include(p => p.ClienteCarga) // Inclui o cliente relacionado à carga
                .Include(p => p.ClienteDescarga) // Inclui o cliente relacionado à descarga
                //.Include(p => p.Motorista) // Inclui o cliente relacionado à descarga
                .OrderBy(p => p.HoraInicio.HasValue && p.HoraFinal.HasValue) // Coloca pedidos com HoraInicio e HoraFinal zeradas primeiro
                .ThenByDescending(p => p.DataPedido) // Ordena pela DataPedido em ordem decrescente
                .ThenByDescending(p => p.HoraPedido) // Ordena pela DataPedido em ordem decrescente
                .ThenByDescending(p => p.HoraFinal.HasValue ? p.HoraFinal : p.HoraInicio) // Ordena por HoraFinal ou HoraInicio
                .ToListAsync();

            return View(pedidos);
        }

        // Método para gerar e fazer o download do PDF
        public async Task<IActionResult> GeneratePdf(int id)
        {
            // Chama o serviço de geração do PDF
            var fileStreamResult = await _pedidoPdfService.GeneratePdfAsync(id);

            // Retorna o arquivo PDF para download
            return File(fileStreamResult.FileStream, "application/pdf", "GuiaTransporte.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GetAvarias(int pedidoId)
        {
            var avarias = await _context.Avaria
                .Where(a => a.PedidoId == pedidoId)
                .Select(a => new
                {
                    a.X,
                    a.Y,
                    TipoAvaria = (int)a.TipoAvaria // Retorna o código do tipo de avaria
                })
                .ToListAsync();

            return Json(avarias);
        }





        // GET: Pedidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pedido == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedido
               .Include(p => p.ClienteCarga) // Inclui o cliente relacionado à carga
               .Include(p => p.ClienteDescarga) // Inclui o cliente relacionado à descarga
               .Include(p => p.Veiculo) // Inclui o veículo
                   .ThenInclude(v => v.Modelo) // Inclui o modelo do veículo
                   .ThenInclude(m => m.Marca) // Inclui a marca do modelo
               .Include(p => p.Motorista) // Inclui o motorista
               .FirstOrDefaultAsync(m => m.Id == id);


            return View(pedido);
        }

        // GET: Pedidos/Create     
        public IActionResult Create()
        {


            ViewBag.DataPretendida = DateTime.Now.ToString("yyyy-MM-dd");
            // Passa os valores do enum para a view
            ViewBag.Veiculos = new SelectList(_context.Veiculo, "Id", "Matricula");

            ViewBag.Clientes = new SelectList(_context.Cliente, "Id", "Nome");

            // Obtém a lista de motoristas com o perfil padrão
            var motoristas = _context.Attorney
                                     .Where(a => a.Perfil == ProfileEnum.Padrao)
                                     .Select(a => new { a.Id, a.Name })
                                     .ToList();

            // Adiciona a opção "Selecione um motorista" no início da lista
            motoristas.Insert(0, new { Id = 0, Name = "Selecione um motorista" });

            // Passa a lista para a ViewBag
            ViewBag.Motoristas = new SelectList(motoristas, "Id", "Name");

            // Garantir que a ViewBag esteja preenchida corretamente
            ViewBag.TipoPedidoOptions = Enum.GetValues(typeof(TipoPedidoEnum)).Cast<TipoPedidoEnum>().ToList();

            return View();
        }

        // POST: Pedidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoInicialDto pedidoDto)
        {
            // Remova validações de campos que não fazem parte desta etapa
            ModelState.Remove("Veiculo");
            ModelState.Remove("ClienteCarga");
            ModelState.Remove("ClienteDescarga");
            ModelState.Remove("Motorista");

            ModelState.Remove("DataPedido");
            ModelState.Remove("HoraPedido");
            ModelState.Remove("AdicionalCarga");
            ModelState.Remove("AdicionalDescarga");


            if (ModelState.IsValid)
            {
                // Mapeando o DTO para a entidade Pedido
                var pedido = new Pedido
                {
                    DataPedido = DateTime.Now.Date, // Data atual
                    HoraPedido = DateTime.Now.TimeOfDay, // Hora atual
                    DataPretendida = pedidoDto.DataPretendida,
                    AdicionalCarga = pedidoDto.AdicionalCarga,
                    AdicionalDescarga = pedidoDto.AdicionalDescarga,
                    TipoPedido = pedidoDto.TipoPedido,
                    VeiculoId = pedidoDto.VeiculoId,
                    ClienteCargaId = pedidoDto.ClienteCargaId,
                    ClienteDescargaId = pedidoDto.ClienteDescargaId,
                    MotoristaId = pedidoDto.MotoristaId > 0 ? pedidoDto.MotoristaId : (int?)null // Permite valor nulo

                };

                // Adiciona o pedido ao contexto
                _context.Add(pedido);
                await _context.SaveChangesAsync();

                // Redireciona após o sucesso
                return RedirectToAction(nameof(Index));
            }

            // Preenche os ViewBags para re-renderizar o formulário em caso de erro
            ViewBag.Clientes = new SelectList(_context.Cliente, "Id", "Nome", pedidoDto.ClienteCargaId);
            ViewBag.Veiculos = new SelectList(_context.Veiculo, "Id", "Matricula", pedidoDto.VeiculoId);
            ViewBag.Motoristas = new SelectList(_context.Attorney.Where(a => a.Perfil == ProfileEnum.Padrao), "Id", "Name", pedidoDto.MotoristaId);

            // Retorna a View com os dados do DTO em caso de falha
            return View(pedidoDto);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var pedido = await _context.Pedido
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Preenche os dados necessários para a view
            ViewBag.Clientes = new SelectList(_context.Cliente, "Id", "Nome", pedido.ClienteCargaId);
            ViewBag.Veiculos = new SelectList(_context.Veiculo, "Id", "Matricula", pedido.VeiculoId);
            ViewBag.Motoristas = new SelectList(_context.Attorney.Where(a => a.Perfil == ProfileEnum.Padrao), "Id", "Name", pedido.MotoristaId);
            // Atualizado para ViewBag.TipoPedidoOptions
            ViewBag.TipoPedidoOptions = Enum.GetValues(typeof(TipoPedidoEnum)).Cast<TipoPedidoEnum>().ToList();


            return View(pedido);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PedidoInicialDto pedidoDto)
        {
            // Remova validações de campos que não fazem parte desta etapa
            ModelState.Remove("Veiculo");
            ModelState.Remove("ClienteCarga");
            ModelState.Remove("ClienteDescarga");
            ModelState.Remove("Motorista");

            ModelState.Remove("DataPedido");
            ModelState.Remove("HoraPedido");
            ModelState.Remove("AdicionalCarga");
            ModelState.Remove("AdicionalDescarga");
            if (id != pedidoDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var pedidoAtualizado = await _context.Pedido.FindAsync(id);

                    if (pedidoAtualizado == null)
                    {
                        return NotFound();
                    }

                    // Atualiza os campos necessários
                    pedidoAtualizado.DataPretendida = pedidoDto.DataPretendida;
                    pedidoAtualizado.AdicionalCarga = pedidoDto.AdicionalCarga;
                    pedidoAtualizado.AdicionalDescarga = pedidoDto.AdicionalDescarga;
                    pedidoAtualizado.TipoPedido = pedidoDto.TipoPedido;
                    pedidoAtualizado.VeiculoId = pedidoDto.VeiculoId;
                    pedidoAtualizado.ClienteCargaId = pedidoDto.ClienteCargaId;
                    pedidoAtualizado.ClienteDescargaId = pedidoDto.ClienteDescargaId;
                    pedidoAtualizado.MotoristaId = pedidoDto.MotoristaId > 0 ? pedidoDto.MotoristaId : (int?)null;

                    // Campos DataPedido e HoraPedido não são modificados

                    _context.Update(pedidoAtualizado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedidoDto.Id))
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

            // Preenche os ViewBags para re-renderizar o formulário em caso de erro
            ViewBag.Clientes = new SelectList(_context.Cliente, "Id", "Nome", pedidoDto.ClienteCargaId);
            ViewBag.Veiculos = new SelectList(_context.Veiculo, "Id", "Matricula", pedidoDto.VeiculoId);
            ViewBag.Motoristas = new SelectList(_context.Attorney.Where(a => a.Perfil == ProfileEnum.Padrao), "Id", "Name", pedidoDto.MotoristaId);
            ViewBag.TipoPedidoOptions = Enum.GetValues(typeof(TipoPedidoEnum)).Cast<TipoPedidoEnum>().ToList();

            return View(pedidoDto);
        }



        // GET: Pedidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pedido == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedido
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pedido == null)
            {
                return Problem("Entity set 'WebAppSystemsContext.Pedido'  is null.");
            }
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedido.Remove(pedido);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
            return (_context.Pedido?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
