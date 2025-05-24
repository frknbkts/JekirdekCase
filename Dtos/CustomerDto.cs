namespace JekirdekCase.Dtos
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}"; // Okuma amaçlı bir property
        public string Email { get; set; }
        public string? Region { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
