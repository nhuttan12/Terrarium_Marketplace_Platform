using api.Database;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace api.Repository
{
    public class MyAppDbContextFactory : IDesignTimeDbContextFactory<MyAppDbContext>
    {
        public MyAppDbContext CreateDbContext(string[] args)
        {
            // 1. Chỉ định rõ thư mục hiện tại để chắc chắn tìm thấy file .env
            var basePath = Directory.GetCurrentDirectory();
            Env.Load(Path.Combine(basePath, ".env"));

            // 2. Lấy thông tin
            var dbPort = Environment.GetEnvironmentVariable("MSSQL_PORT");
            var dbPass = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");

            // Kiểm tra an toàn
            if (string.IsNullOrEmpty(dbPass))
            {
                throw new Exception("Lỗi Design-Time: Không tìm thấy MSSQL_SA_PASSWORD trong file .env!");
            }

            var port = string.IsNullOrEmpty(dbPort) ? "1433" : dbPort;
            var connectionString = $"Server=localhost,{port};Database=tmdt_2026;User Id=sa;Password={dbPass};TrustServerCertificate=True;MultipleActiveResultSets=true";

            // 3. Khởi tạo và trả về DbContext độc lập
            var optionsBuilder = new DbContextOptionsBuilder<MyAppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new MyAppDbContext(optionsBuilder.Options);
        }
    }
}
