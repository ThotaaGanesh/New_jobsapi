namespace JobsApi.Models
{
    public class AppSettings
    {
        public JwtConfig? JwtConfig { get; set; }
        public AzureFunctions? azureFunctions { get; set; }
    }
}
