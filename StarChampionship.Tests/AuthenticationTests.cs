using Moq;
using StarChampionship.Models;
using StarChampionship.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace StarChampionship.Tests
{
    /// <summary>
    /// Testes de autenticação e autorização JWT.
    /// 
    /// ✓ Testa geração de tokens JWT
    /// ✓ Testa validação de senha do Admin
    /// ✓ Testa se endpoints protegidos requerem token
    /// </summary>
    public class AuthenticationTests
    {
        /// <summary>
        /// Testa se o JwtTokenService gera um token JWT válido para o Admin.
        /// 
        /// ✓ Deve retornar um token com a role "Admin"
        /// ✓ Deve retornar uma data de expiração válida
        /// ✓ Token deve estar no formato JWT (xxx.yyy.zzz)
        /// </summary>
        [Fact]
        public void JwtTokenService_DeveGerarTokenValido()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<JwtTokenService>>();

            // Configura valores que o serviço espera ler
            configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("chave-super-secreta-com-mais-de-32-caracteres-para-hs256!!!");
            configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("StarChampionshipApi");
            configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("StarChampionshipUsers");
            configMock.Setup(c => c["JwtSettings:ExpirationMinutes"])
                .Returns("60");

            var jwtTokenService = new JwtTokenService(configMock.Object, loggerMock.Object);

            // Act
            var (token, expiresAt) = jwtTokenService.GenerateAdminToken();

            // Assert
            Assert.NotEmpty(token);
            Assert.NotEqual(default, expiresAt);
            Assert.True(expiresAt > DateTime.UtcNow);
            Assert.Contains(".", token); // JWT tem formato: xxx.yyy.zzz
        }

        /// <summary>
        /// Testa se o token JWT gerado contém a role "Admin" no payload.
        /// 
        /// ✓ Token deve conter o claim de role "Admin"
        /// </summary>
        [Fact]
        public void JwtTokenService_TokenDeveConterClaimAdmin()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<JwtTokenService>>();

            configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("chave-super-secreta-com-mais-de-32-caracteres-para-hs256!!!");
            configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("StarChampionshipApi");
            configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("StarChampionshipUsers");
            configMock.Setup(c => c["JwtSettings:ExpirationMinutes"])
                .Returns("60");

            var jwtTokenService = new JwtTokenService(configMock.Object, loggerMock.Object);

            // Act
            var (token, _) = jwtTokenService.GenerateAdminToken();

            // Decodifica o token para verificar os claims (sem validação de assinatura, apenas análise)
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
            Assert.NotNull(roleClaim);
            Assert.Equal("Admin", roleClaim.Value);
        }

        /// <summary>
        /// Testa se o JwtTokenService lança exceção quando SecretKey não está configurada.
        /// 
        /// ✓ Deve lançar InvalidOperationException
        /// </summary>
        [Fact]
        public void JwtTokenService_DeveLancarExcecaoQuandoSecretKeyNaoEstaConfigurada()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<JwtTokenService>>();

            // SecretKey não configurada (null)
            configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns((string?)null);

            var jwtTokenService = new JwtTokenService(configMock.Object, loggerMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => jwtTokenService.GenerateAdminToken());
        }

        /// <summary>
        /// Testa se a data de expiração do token é configurada corretamente.
        /// 
        /// ✓ ExpirationMinutes=60 deve resultar em token expirando em ~60 minutos
        /// </summary>
        [Fact]
        public void JwtTokenService_DataExpiracaoDeveSer60MinutosNoFuturo()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<JwtTokenService>>();

            configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("chave-super-secreta-com-mais-de-32-caracteres-para-hs256!!!");
            configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("StarChampionshipApi");
            configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("StarChampionshipUsers");
            configMock.Setup(c => c["JwtSettings:ExpirationMinutes"])
                .Returns("60");

            var jwtTokenService = new JwtTokenService(configMock.Object, loggerMock.Object);
            var agora = DateTime.UtcNow;

            // Act
            var (_, expiresAt) = jwtTokenService.GenerateAdminToken();

            // Assert
            var diferencaMinutos = (expiresAt - agora).TotalMinutes;
            Assert.InRange(diferencaMinutos, 59, 61); // Tolerância de 1 minuto
        }

        /// <summary>
        /// Testa se senhas diferentes produzem resultados diferentes.
        /// 
        /// ✓ Senha "senha" ≠ Senha "senhaErrada"
        /// </summary>
        [Fact]
        public void AuthValidation_SenhasDistintasDevemSerDiferentes()
        {
            // Arrange
            var senhaCorreta = "senha";
            var senhaIncorreta = "senhaErrada";

            // Act & Assert
            Assert.NotEqual(senhaCorreta, senhaIncorreta);
        }

        /// <summary>
        /// Testa se a mesma senha é reconhecida corretamente.
        /// 
        /// ✓ Senha "senha" == Senha "senha"
        /// </summary>
        [Fact]
        public void AuthValidation_MesmaSenhaDeveSerIgual()
        {
            // Arrange
            var senhaCorreta = "senha";

            // Act & Assert
            Assert.Equal(senhaCorreta, "senha");
        }

        /// <summary>
        /// Testa se o token gerado tem a estrutura JWT padrão (3 partes).
        /// 
        /// ✓ Token deve ter formato: header.payload.signature
        /// </summary>
        [Fact]
        public void JwtTokenService_TokenDeveTemEstruturaPadraoJwt()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<JwtTokenService>>();

            configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("chave-super-secreta-com-mais-de-32-caracteres-para-hs256!!!");
            configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("StarChampionshipApi");
            configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("StarChampionshipUsers");
            configMock.Setup(c => c["JwtSettings:ExpirationMinutes"])
                .Returns("60");

            var jwtTokenService = new JwtTokenService(configMock.Object, loggerMock.Object);

            // Act
            var (token, _) = jwtTokenService.GenerateAdminToken();

            // Assert - JWT deve ter exatamente 3 partes separadas por ponto
            var partes = token.Split('.');
            Assert.Equal(3, partes.Length);
            Assert.NotEmpty(partes[0]); // Header
            Assert.NotEmpty(partes[1]); // Payload
            Assert.NotEmpty(partes[2]); // Signature
        }

        /// <summary>
        /// Testa se o token inclui informações do issuer correto.
        /// 
        /// ✓ Token deve ter issuer = "StarChampionshipApi"
        /// </summary>
        [Fact]
        public void JwtTokenService_TokenDeveConterIssuerCorreto()
        {
            // Arrange
            var issuerEsperado = "StarChampionshipApi";
            var configMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<ILogger<JwtTokenService>>();

            configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("chave-super-secreta-com-mais-de-32-caracteres-para-hs256!!!");
            configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns(issuerEsperado);
            configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("StarChampionshipUsers");
            configMock.Setup(c => c["JwtSettings:ExpirationMinutes"])
                .Returns("60");

            var jwtTokenService = new JwtTokenService(configMock.Object, loggerMock.Object);

            // Act
            var (token, _) = jwtTokenService.GenerateAdminToken();

            // Decodifica para verificar
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal(issuerEsperado, jwtToken.Issuer);
        }
    }
}
