using System.Text.Json.Serialization;
using NSwag;
using NSwag.Generation.Processors.Security;
using StorageCompany.Api.Middleware;
using StorageCompany.Core;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Repositories;


namespace StorageCompany.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppSettings"));

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument(cfg =>
        {
            cfg.Title = "Storage Company API";

            cfg.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Scheme = "Bearer ",
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            cfg.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentCors", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Repositories: infrastructure implementations behind Core interfaces.
        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IFacilityRepository, FacilityRepository>();
        builder.Services.AddSingleton<IStorageUnitTypeRepository, StorageUnitTypeRepository>();
        builder.Services.AddSingleton<IStorageUnitRepository, StorageUnitRepository>();
        builder.Services.AddSingleton<IReservationRepository, ReservationRepository>();
        builder.Services.AddSingleton<IRentalRepository, RentalRepository>();
        builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
        builder.Services.AddSingleton<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddSingleton<IAccessCodeRepository, AccessCodeRepository>();
        builder.Services.AddSingleton<ISupportRequestRepository, SupportRequestRepository>();

        // Core business services.
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IFacilityService, FacilityService>();
        builder.Services.AddScoped<IStorageUnitTypeService, StorageUnitTypeService>();
        builder.Services.AddScoped<IStorageUnitService, StorageUnitService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<IRentalService, RentalService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IInvoiceService, InvoiceService>();
        builder.Services.AddScoped<IAccessCodeService, AccessCodeService>();
        builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
        builder.Services.AddScoped<ISecurityService, SecurityService>();

        var app = builder.Build();

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseHttpsRedirection();
        app.UseCors("DevelopmentCors");
        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.Run();
    }
}
