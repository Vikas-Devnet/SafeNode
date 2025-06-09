using Microsoft.OpenApi.Models;
using SafeNodeAPI.Extensions;
using System.Text.Json.Serialization;

namespace SafeNodeAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafeNode API v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SafeNode API",
                    Version = "v1",
                    Description = "A secure file vault API"
                });
            });
            builder.Services.AddAppConfigurations(builder.Configuration);
            builder.Services.AddDatabase(builder.Configuration);

            // Register repositories and services
            builder.Services.AddRepositories();
            builder.Services.AddApplicationServices();
        }
    }
}
