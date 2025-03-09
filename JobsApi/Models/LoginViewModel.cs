using System.ComponentModel.DataAnnotations;

namespace JobsApi.Models
{
    public class LoginViewModel
    {
        public string? UserName { get; set; }

        public string? Password { get; set; }
    }
}
