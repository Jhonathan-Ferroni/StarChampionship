using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace StarChampionship.Services
{
    /// <summary>
    /// Serviço responsável pela geração e validação de tokens JWT.
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gera um token JWT com a role "Admin".
        /// </summary>
        /// <returns>Token JWT e data de expiração</returns>
        public (string token, DateTime expiresAt) GenerateAdminToken()
        {
            var secretKey = _configuration["JwtSettings:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey not configured");
            
            var issuer = _configuration["JwtSettings:Issuer"] ?? "StarChampionshipApi";
            var audience = _configuration["JwtSettings:Audience"] ?? "StarChampionshipUsers";
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

            // Cria os claims (dados do token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("sub", "admin-user")
            };

            // Cria a chave de segurança
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define quando o token expira
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // Cria o descriptor do token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            // Gera o token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("Token JWT gerado com sucesso para Admin");
            return (tokenString, expiresAt);
        }
    }
}
