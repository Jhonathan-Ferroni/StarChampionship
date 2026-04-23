using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StarChampionship.Services;

namespace StarChampionship.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;

        public AccountApiController(JwtTokenService jwtTokenService, IConfiguration configuration)
        {
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
        }

        public class LoginRequest
        {
            public string Password { get; set; }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var senhaCorreta = _configuration["AdminConfig:Password"];
            if (request == null || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Password is required" });

            if (request.Password != senhaCorreta)
                return Unauthorized(new { error = "Invalid password" });

            var (token, expiresAt) = _jwtTokenService.GenerateAdminToken();
            return Ok(new { token, expiresAt });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // For JWT stateless logout is handled on client side (remove token)
            return NoContent();
        }
    }
}
