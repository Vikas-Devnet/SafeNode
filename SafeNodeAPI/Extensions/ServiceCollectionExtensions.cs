using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Data;
using SafeNodeAPI.Models.Constants;
using SafeNodeAPI.Src.Repository.User;
using SafeNodeAPI.Src.Services.Auth;

namespace SafeNodeAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"Using DB connection: {connString}");

            services.AddDbContext<SafeNodeDbContext>(options =>
                options.UseSqlServer(connString));

            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
