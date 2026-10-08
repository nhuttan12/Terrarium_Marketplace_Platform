using api.Database;
using api.Extensions;
using api.Services.Auths;
using api.Utilities;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAppSecurity(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration)
    .AddRepositories()
    .AddBusinessServices();
builder.Services.AddConfiguredOpenApi(builder.Configuration);

// Build the app
var app = builder.Build();
// Global exception handling middleware
app.UseExceptionHandler();

// Configure the HTTP request pipeline.|
// create user admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    var context = services.GetRequiredService<MyAppDbContext>();
    var authService = services.GetRequiredService<IAuthService>();
    var idGenerator = services.GetRequiredService<IIdGenerator>();

    await api.Database.Seeders.DatabaseSeeder.SeedAsync(app.Services, idGenerator);
}

await app.SeedSmartDataAsync(app.Lifetime.ApplicationStopped);

// 2. Cấu hình Middleware
if (app.Environment.IsDevelopment())
{
    // Sinh file JSON đặc tả tại /openapi/v1.json
    app.MapOpenApi();

    // Thiết lập Swagger UI để hiển thị giao diện
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "E-Commerce API v1");
        options.RoutePrefix = "swagger"; // Truy cập qua: http://localhost:5161/swagger
        options.DocumentTitle = " Documentation";
    });
}

// Chuyển hướng HTTP sang HTTPS
app.UseHttpsRedirection();
// CORS middleware
app.UseCors("AllowSpecificOrigin");

// Authentication & Authorization middleware 
// tự động kiểm tra token trong header của các request đến và xác thực người dùng dựa trên token đó
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();
app.Run();
