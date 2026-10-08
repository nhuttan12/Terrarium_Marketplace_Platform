using api.Utilities;
using api.Repository;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using api.Database;

namespace api.Extensions
{
    /// <summary>
    /// Đăng ký CORS, Exception Handler, AutoMapper.
    /// </summary>
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MyAppDbContext>(options => options.UseSqlServer(EnvironmentVariables.ConnectionString));

            // Đăng ký AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(Program).Assembly);
            });
            services.AddTransient(typeof(PagedResultConverter<,>));
            // Đăng ký CORS nếu cần thiết (ví dụ: cho phép frontend truy cập API) 
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                     builder => builder
                         .WithOrigins(allowedOrigins ?? ["http://localhost:3000", "https://localhost:3000", "http://127.0.0.1:3000"]) //TODO: Thay đổi thành URL frontend của bạn
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .AllowCredentials()); // BẮT BUỘC: Cho phép nhận Cookie/Credentials

            });
            // Đăng ký Exception Handler Middleware: tự động sinh id
            services.AddSingleton<IIdGenerator, SqlServerSequentialIdGenerator>();
            return services;
        }
    }
}
