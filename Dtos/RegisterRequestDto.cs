using System.ComponentModel.DataAnnotations;

namespace JekirdekCase.Dtos
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress] // Email formatını da doğrulayalım
        public string Email { get; set; } // YENİ EKLENDİ

        [Required]
        [StringLength(50, MinimumLength = 6)] // Şifre uzunluğu gibi kurallar eklenebilir
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // Örn: "User", "Admin"
    }
}
