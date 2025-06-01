namespace JekirdekCase.Configuration
{
    public class JwtSettings
    {
        public string Issuer { get; set; } // yayinlayan
        public string Audience { get; set; } // hedeflenen
        public string SecretKey { get; set; } 
        public int ExpirationInMinutes { get; set; }
    }
}
