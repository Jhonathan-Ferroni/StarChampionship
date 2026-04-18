using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarChampionship.Models.Dtos;
using StarChampionship.Services;

namespace StarChampionship.Controllers.Api
{
    /// <summary>
    /// Controller de Autenticação responsável por gerar tokens JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration, 
            JwtTokenService jwtTokenService,
            ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de login do Admin. Retorna um token JWT se a senha estiver correta.
        /// </summary>
        /// <param name="request">Objeto contendo a senha do admin</param>
        /// <returns>Token JWT e data de expiração ou erro 401</returns>
        /// <remarks>
        /// Exemplo de requisição:
        /// POST /api/auth/login
        /// {
        ///   "password": "senha"
        /// }
        /// </remarks>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Tentativa de login com senha vazia");
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Password is required" 
                });
            }

            // Lê a senha correta da configuração (ambiente ou appsettings)
            var senhaCorreta = _configuration["AdminConfig:Password"];

            if (request.Password != senhaCorreta)
            {
                _logger.LogWarning("Tentativa de login com senha incorreta");
                return Unauthorized(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Invalid password" 
                });
            }

            try
            {
                // Gera o token JWT
                var (token, expiresAt) = _jwtTokenService.GenerateAdminToken();

                _logger.LogInformation("Admin login bem-sucedido. Token gerado.");
                
                return Ok(new LoginResponse 
                { 
                    Success = true, 
                    Message = "Login successful",
                    Token = token,
                    ExpiresAt = expiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar token JWT");
                return StatusCode(500, new LoginResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }
    }
}
