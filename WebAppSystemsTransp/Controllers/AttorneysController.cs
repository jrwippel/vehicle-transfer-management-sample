using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using WebAppSystems.Filters;
using WebAppSystems.Models;
using WebAppSystems.Models.Enums;
using WebAppSystems.Models.ViewModels;
using WebAppSystems.Services;
using WebAppSystems.Services.Exceptions;
using static WebAppSystems.Helper.Sessao;

namespace WebAppSystems.Controllers
{
    [PaginaRestritaSomenteAdmin]
    public class AttorneysController : Controller
    {
        private readonly AttorneyService _attorneyService;
      

        public AttorneysController (AttorneyService attorneyService)
        {
            _attorneyService = attorneyService;           
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _attorneyService.FindAllAsync();
                return View(list);
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a página de login se a sessão expirou
                TempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }

       
        public async Task<IActionResult> Create()
        {       
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attorney attorney)
        {
            // Verifica se o modelo é inválido
            if (!ModelState.IsValid)
            {
                return View(attorney); // Retorna a mesma View com os erros de validação
            }

            // Insere o registro no banco de dados
            await _attorneyService.InsertAsync(attorney);

            // Redireciona para a lista ou outra ação após a criação
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not provided"});
            }
  
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _attorneyService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }catch (IntegrityException ex)
            {
                return RedirectToAction(nameof(Error), new { message = ex.Message });
            }          
        }

        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not provided" }); ;
            }
            var obj = await _attorneyService.FindByIdAsync(id.Value);
            if (obj == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not found" });
            }
            return View(obj);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not provided" });
            }
            var obj = await _attorneyService.FindByIdAsync(id.Value);
            if (obj == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not found" });
            }           
            AttorneyFormViewModel viewModel = new AttorneyFormViewModel { Attorney = obj};
            return View(viewModel);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attorney attorney)     
        
        {
            if (ModelState.IsValid)
            {               
               
                return View();
            }

            if (id != attorney.Id) 
            { 
                return RedirectToAction(nameof(Error), new { message = "Id not mismatch" });
            }
            try
            {                
                await _attorneyService.UpdateAsync(attorney);
                return RedirectToAction(nameof(Index));
            }catch (NotFoundException ex)
            {
                return RedirectToAction(nameof(Error), new { message = ex.Message });
            }
        }

        public IActionResult Error (string message)
        {
            var viewModel = new ErrorViewModel
            {
               Message = message,
               RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(viewModel);
        }

    }
}
