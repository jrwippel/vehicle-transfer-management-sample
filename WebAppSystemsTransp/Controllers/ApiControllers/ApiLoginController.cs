using Microsoft.AspNetCore.Mvc;
using WebAppSystems.Helper;
using WebAppSystems.Models;
using WebAppSystems.Services;
using WebAppSystemsTransp.Models;

namespace WebAppSystems.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiLoginController : ControllerBase
    {
        private readonly AttorneyService _attorneyService;
        private readonly ISessao _sessao;

        public ApiLoginController(AttorneyService attorneyService, ISessao sessao)
        {
            _attorneyService = attorneyService;
            _sessao = sessao;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = _attorneyService.FindByLoginAsync(loginModel.Login);
                if (usuario != null)
                {
                    if (usuario.ValidaSenha(loginModel.Senha))
                    {
                        // Autenticação bem-sucedida, crie um token de autenticação ou retorne uma resposta de sucesso
                        return Ok(new { Message = "Login bem-sucedido", Usuario = usuario });
                    }
                    return Unauthorized(new { Message = "Senha do usuário é inválida." });
                }
                return Unauthorized(new { Message = "Usuário e/ou senha inválido(s)." });
            }

            return BadRequest(ModelState);
        }
    }
}
