using System.Text.Json.Serialization;
using StorageCompany.Api.Middleware;
using StorageCompany.Core.Interfaces.Repositories;
using StorageCompany.Core.Interfaces.Services;
using StorageCompany.Core.Services;
using StorageCompany.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Storage Company Showcase API",
        Version = "v1",
        Description = "A mock storage unit rental API built with a SOLID-friendly Core/API/Infrastructure structure."
    });
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
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
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
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IStorageUnitTypeService, StorageUnitTypeService>();
builder.Services.AddScoped<IStorageUnitService, StorageUnitService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAccessCodeService, AccessCodeService>();
builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Storage Company Showcase API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("DevelopmentCors");
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
