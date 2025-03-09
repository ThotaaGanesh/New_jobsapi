using JobsApi.Models;
using JobsApi.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobsApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JobsApiContext _context;
        public AuthService(JobsApiContext context) => _context = context;

        public async Task<User> Login(LoginViewModel userViewModel)
        {
           // Assuming userViewModel includes necessary fields like username and password
           User user = await _context.Users.Include(x => x.Role)
                                     .FirstOrDefaultAsync(u => u.Name == userViewModel.UserName);

            if (user != null && VerifyPassword(userViewModel.Password, user.Password))
            {
                return user;
            }

            return null;
        }

        private bool VerifyPassword(string providedPassword, string storedPasswordHash)
        {
            // Assuming hashing or password verification logic here
            return providedPassword == storedPasswordHash; // Simplification for example purposes
        }
    }
}
