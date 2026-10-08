using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace api.Extensions
{
    /// <summary>
    ///  Chứa toàn bộ logic về JWT, Cookie, Google Auth và OpenAPI.
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// thiết lập Authentication và Authorization cho ứng dụng, bao gồm:
        /// - JWT Bearer Authentication
        /// - Cookie Authentication (để lưu token an toàn trên trình duyệt)
        /// - Google Authentication (đăng nhập bằng tài khoản Google)
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IServiceCollection AddAppSecurity(this IServiceCollection services, IConfiguration configuration)
        {

            // Cấu hình JWT Authentication
            var jwtSettings = configuration.GetSection("JwtSettings");
            var jwtKey = EnvironmentVariables.JwtKey;
            var key = Encoding.ASCII.GetBytes(jwtKey);
            // Cấu hình Authentication với JWT Bearer
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = jwtSettings["Issuer"],
                            ValidAudience = jwtSettings["Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(key),

                            ClockSkew = TimeSpan.Zero
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token =
                                context.Request.Cookies["X-Access-Token"];

                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = EnvironmentVariables.GoogleClientId;
                    options.ClientSecret = EnvironmentVariables.GoogleClientSecret;
                    options.SignInScheme = Microsoft.AspNetCore.Authentication.Cookies
                            .CookieAuthenticationDefaults
                            .AuthenticationScheme;
                    options.ClaimActions.MapJsonKey("picture", "picture");
                });
            // Cấu hình Authorization
            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// thiết lập OpenAPI (Swagger) cho ứng dụng
        /// - Thêm định nghĩa Security để Swagger UI có thể gửi token qua Cookie khi thử nghiệm API 
        /// - Cấu hình thông tin API như tiêu đề, phiên bản, và mô tả
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfiguredOpenApi(this IServiceCollection services, IConfiguration configuration)
        {
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    // 1. Thêm định nghĩa Security cho Cookie
                    var requirements = new Dictionary<string, OpenApiSecurityScheme>
                    {
                        ["CookieAuth"] = new OpenApiSecurityScheme
                        {
                            // Thay vì Http/Bearer, ta dùng ApiKey trong Cookie
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Cookie,
                            Name = "X-Access-Token", // Tên này PHẢI trùng với tên trong Response.Cookies.Append
                            Description = "Hệ thống sử dụng HttpOnly Cookie. Hãy đăng nhập qua API Login để nhận Token tự động."
                        }
                    };

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes = requirements;

                    // 2. Áp dụng Security cho tất cả Endpoint
                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "CookieAuth", // Trùng với key ở trên
                                Type = ReferenceType.SecurityScheme
                            }
                        }] = Array.Empty<string>()
                    });

                    // 3. Thông tin API
                    document.Info.Title = "E-Commerce API System";
                    document.Info.Version = "v1";
                    document.Info.Description = "Hệ thống API sử dụng JWT lưu trong HttpOnly Cookie.";

                    return Task.CompletedTask;
                });
            });
            return services;
        }
    }
}
