using Application.Interfaces;
using Infrastructure.Mappings;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Identity;
using Infrastructure.Services.Audit;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Categories;
using Infrastructure.Services.Dashboard;
using Infrastructure.Services.Email;
using Infrastructure.Services.Events;
using Infrastructure.Services.Images;
using Infrastructure.Services.Orders;
using Infrastructure.Services.Payments;
using Infrastructure.Services.Reports;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Tickets;
using Infrastructure.Services.Users;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) {
        services.Configure<AppSettings>(
            configuration.GetSection(AppSettings.SectionName));
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StripeOptions>(
            configuration.GetSection(StripeOptions.SectionName));
        services.Configure<SupabaseOptions>(
            configuration.GetSection(SupabaseOptions.SectionName));
        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));
        services.Configure<SeedOptions>(
            configuration.GetSection(SeedOptions.SectionName));

        services.AddSingleton<IAppSettings>(sp =>
            sp.GetRequiredService<IOptions<AppSettings>>().Value);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options => {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddJwtAuthentication(configuration);
        services.AddHttpContextAccessor();
        services.AddAutoMapper(
            typeof(Application.DependencyInjection).Assembly,
            typeof(IdentityUserMappingProfile).Assembly);
        services.AddScoped<AuditLogWriter>();
        services.AddScoped<UserMapper>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<LoggingEmailService>();
        services.AddScoped<SmtpEmailService>();
        services.AddScoped<IEmailService>(serviceProvider => {
            var email = serviceProvider
                .GetRequiredService<IOptions<EmailOptions>>()
                .Value;

            if (string.IsNullOrWhiteSpace(email.Host) ||
                string.IsNullOrWhiteSpace(email.From)) {
                return serviceProvider.GetRequiredService<LoggingEmailService>();
            }

            return serviceProvider.GetRequiredService<SmtpEmailService>();
        });

        services.AddHttpClient<IStorageService, SupabaseStorageService>((serviceProvider, client) => {
            var supabase = serviceProvider
                .GetRequiredService<IOptions<SupabaseOptions>>()
                .Value;

            if (!string.IsNullOrWhiteSpace(supabase.Url)) {
                client.BaseAddress = new Uri(supabase.Url.TrimEnd('/') + "/");
            }

            if (!string.IsNullOrWhiteSpace(supabase.ServiceRoleKey)) {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        supabase.ServiceRoleKey);
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "apikey",
                    supabase.ServiceRoleKey);
            }
        });

        return services;
    }
}
