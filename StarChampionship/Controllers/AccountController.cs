using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Configuration; // Necessário para ler o appsettings

namespace StarChampionship.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        // Injeta o IConfiguration
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string senha)
        {
            // Lê a senha da configuração (seja do appsettings ou do Render)
            var senhaCorreta = _configuration["AdminConfig:Password"];

            if (senha == senhaCorreta)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Admin") };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Erro = "Senha incorreta!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}