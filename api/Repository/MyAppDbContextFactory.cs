using api.Database;
using api.Extensions;
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
            var envPath = Path.Combine(Directory.GetCurrentDirectory(),".env");
            DotNetEnv.Env.Load(envPath);
            
            // 3. Khởi tạo và trả về DbContext độc lập
            var optionsBuilder = new DbContextOptionsBuilder<MyAppDbContext>();
            
            optionsBuilder.UseSqlServer(EnvironmentVariables.ConnectionString);

            return new MyAppDbContext(optionsBuilder.Options);
        }
    }
}
