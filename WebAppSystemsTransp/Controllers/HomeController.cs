using Microsoft.AspNetCore.Mvc;
using WebAppSystems.Helper;
using WebAppSystems.Models;
using WebAppSystems.Models.Enums;
using WebAppSystems.Services;
using static WebAppSystems.Helper.Sessao;

namespace WebAppSystems.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISessao _sessao;

        public HomeController(ISessao sessao)
        {
            _sessao = sessao;
        }
        public async Task<IActionResult> Index()
        {

            var usuario = _sessao.BuscarSessaoDoUsuario();

            if (usuario == null)
            {
                return RedirectToAction("Index", "Login"); // Redireciona para o login se não estiver autenticado
            }

            return View(usuario); // Passa o usuário autenticado para a View
        }


        public IActionResult About()
        {
            return View();
        }
    }
}