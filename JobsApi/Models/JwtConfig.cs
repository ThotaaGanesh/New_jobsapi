namespace JobsApi.Models
{
    public class JwtConfig
    {
        public string? Secret { get; set; }
        public string? ValidAudience { get; set; }
        public string? ValidIssuer { get; set; }
        public int TokenExpirationMinutes { get; set; }
    }

    public class AzureFunctions
    {
        public required string WhatsupFunctionUrl { get; set; }
        public required string EmailFunctionUrl { get; set; }
        public required string SmsFunctionUrl { get; set; }
    }
}
