using api.Database;
using api.Database.Interceptors;
using api.Exceptions;
using api.Extensions.EmailExtensions;
using api.Models;
using api.Models.Jwts;
using api.Repository;
using api.Repository.Analyst;
using api.Repository.BannerRepo;
using api.Repository.CartRepo;
using api.Repository.Categories;
using api.Repository.Coupons;
using api.Repository.Inventory;
using api.Repository.InvoiceRepo;
using api.Repository.PaymentRepo;
using api.Repository.ProductRepo;
using api.Repository.Promotions;
using api.Repository.RoleRepo;
using api.Repository.Shops;
using api.Repository.UserRepo;
using api.Services.Analyst;
using api.Services.Auths;
using api.Services.Banners;
using api.Services.Carts;
using api.Services.Categorys;
using api.Services.Coupons;
using api.Services.Inventory;
using api.Services.Invoices;
using api.Services.Mail;
using api.Services.Payment;
using api.Services.Products;
using api.Services.Promotions;
using api.Services.Shops;
using api.Services.Users;
using api.Utilities.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    /// <summary>
    /// Đăng ký Database, Repositories, và Services.
    /// </summary>
    public static class DependencyInjection
    {
        private const string Variable = "MSSQL_PORT";

        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // đăng ký các interceptor của EF Core
            services.AddSingleton<AuditableEntityInterceptor>();
            // Đăng ký các dịch vụ bảo mật tại đây 
            // Đăng ký DbContext với SQL Server
            
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            // email 
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IMailService, MailService>();
            // paypal
            services.AddHttpClient<api.Services.Payment.PayPalService>();
            // dang ký global exception handler
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepo, RoleRepo>();
            services.AddScoped<IAuthRepo, AuthRepo>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepo, CategoryRepo>();
            services.AddScoped<IStoredProcedureRepository, StoredProcedureRepository>();
            services.AddScoped<IBankingRepository, BankingRepository>();
            services.AddScoped<IUserBannerRepository, UserBannerRepository>();
            services.AddScoped<IAdminBannerRepository, AdminBannerRepository>();
            services.AddScoped<IAdminCouponRepository, AdminCouponRepository>();
            services.AddScoped<IUserCouponRepository, UserCouponRepository>();
            services.AddScoped<IUserPromotionRepository, UserPromotionRepository>();
            services.AddScoped<IAdminPromotionRepository, AdminPromotionRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
            services.AddScoped<IShopUserRepository, ShopUserRepository>();
            services.AddScoped<IShopAdminRepository, ShopAdminRepository>();
            services.AddScoped<IGoodsSupplierRepository, GoodsSupplierRepository>();
            services.AddScoped<IGoodsStockRepository, GoodsStockRepository>();
            services.AddScoped<IGoodsIssueRepository, GoodsIssueRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IDeliveryRepository, DeliveryRepository>();
            services.AddScoped<IAnalystRepository, AnalystRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IVariantRepository, VariantRepository>();

            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAdminBannerService, AdminBannerService>();
            services.AddScoped<IUserBannerService, UserBannerService>();
            services.AddScoped<IBankingService, BankingService>();
            services.AddScoped<IAdminCouponService, AdminCouponService>();
            services.AddScoped<IUserCouponService, UserCouponService>();
            services.AddScoped<IUserPromotionService, UserPromotionService>();
            services.AddScoped<IAdminPromotionService, AdminPromotionService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
            services.AddScoped<IShopUserService, ShopUserService>();
            services.AddScoped<IShopAdminService, ShopAdminService>();
            services.AddScoped<IGoodsStockService, GoodsStockService>();
            services.AddScoped<IGoodsSupplierService, GoodsSupplierService>();
            services.AddScoped<IGoodsIssueService, GoodsIssueService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IAnalystService, AnalystService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IVariantService, VariantService>();

            // Đăng ký Data Seeders
            services.AddScoped<IDataSeeder, CategorySeeder>();
            services.AddScoped<IDataSeeder, ShopSeeder>();
            services.AddScoped<IDataSeeder, ProductSeeder>();

            return services;
        }
    }
}
