namespace StarChampionship.Models.Dtos
{
    /// <summary>
    /// DTO para resposta de login com o token JWT
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
