namespace StarChampionship.Models.Dtos
{
    /// <summary>
    /// DTO para requisição de login do Admin
    /// </summary>
    public class LoginRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}
