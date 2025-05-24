namespace JekirdekCase.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public LoginResponseDto(string token, DateTime expiration, string username, string role)
        {
            Token = token;
            Expiration = expiration;
            Username = username;
            Role = role;
        }
    }
}
