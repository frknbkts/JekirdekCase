using System.ComponentModel.DataAnnotations;

namespace JekirdekCase.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
