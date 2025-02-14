using System.ComponentModel.DataAnnotations;


namespace StudentAPI.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
