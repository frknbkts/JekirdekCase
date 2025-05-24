using System.ComponentModel.DataAnnotations;

namespace JekirdekCase.Dtos
{
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name can't be longer than 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name can't be longer than 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(50, ErrorMessage = "Email can't be longer than 50 characters.")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Region can't be longer than 50 characters.")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "Registration date is required.")]
        public DateTime RegistrationDate { get; set; }
    }
}
