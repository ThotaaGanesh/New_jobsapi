using JobsApi.Services;
using JobsApi.Services.Contracts;
using JobsApi.Services.Implementations;

namespace JobsApi.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection RegisterService(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<HttpService>();
            services.AddTransient<IAuthService, AuthService>();
            return services;
        }
    }
}
