using JobsApi.Models;

namespace JobsApi.Services.Contracts
{
    public interface IAuthService
    {
        Task<User> Login(LoginViewModel userViewModel);
    }
}
