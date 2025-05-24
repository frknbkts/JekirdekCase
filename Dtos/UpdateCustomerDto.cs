using System.ComponentModel.DataAnnotations;

namespace JekirdekCase.Dtos
{
    public class UpdateCustomerDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name can't be longer than 100 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name can't be longer than 100 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email can't be longer than 150 characters.")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "Region can't be longer than 100 characters.")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "Registration date is required.")]
        public DateTime RegistrationDate { get; set; }
    }
}
