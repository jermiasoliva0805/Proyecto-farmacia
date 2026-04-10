using AutoMapper;
using Back.Data;
using Back.DTOS;
using Back.Interfaces;
using Back.Repositories;
using Back.Repositories.Interfaces;
using Back.Services;
using Back.Services.Interfaces;
using Back.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Back
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------------------------------------------------------------
            // LOGGING (clave para ver algo en Azure Log Stream)
            // ------------------------------------------------------------
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // ------------------------------------------------------------
            // URLS / PUERTO (Azure App Service en contenedor)
            // Recomendado setear en Azure:
            //   ASPNETCORE_URLS = http://0.0.0.0:8080
            //   WEBSITES_PORT   = 8080
            // Si no está seteado, fallback a 8080.
            // ------------------------------------------------------------
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (string.IsNullOrWhiteSpace(urls))
            {
                builder.WebHost.UseUrls("http://0.0.0.0:8080");
            }

            // 1. Configuración de Controladores y JSON
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddEndpointsApiExplorer();

            // 2. Configuración de Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Farmacia", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 3. DB Context
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 4. AutoMapper y Validaciones
            builder.Services.AddAutoMapper(typeof(Back.Mappings.MappingProfile));
            builder.Services.AddFluentValidationAutoValidation();

            // Registro manual del/los validadores para evitar el error:
            // CS1061: IServiceCollection no contiene AddValidatorsFromAssemblyContaining
            builder.Services.AddScoped<IValidator<RegisterDTO>, RegisterUserValidator>();

            // 5. Inyección de Repositorios
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
            builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
            builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ILocalityRepository, LocalityRepository>();
            builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
            builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
            builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
            builder.Services.AddScoped<ICancellationRepository, CancellationRepository>();

            // 6. Inyección de Servicios
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ILocalidadService, LocalidadService>();
            builder.Services.AddScoped<IPedidoService, PedidoService>();
            builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
            builder.Services.AddScoped<ITrackingService, TrackingService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IHistoryService, HistoryService>();
            builder.Services.AddScoped<IDeliveryService, DeliveryService>();
            builder.Services.AddScoped<ICancellationService, CancellationService>();
            builder.Services.AddScoped<ClientProductRelationService>();

            // 6a. SMTP Configuration - Lee del formato Azure (Smtp__Host) o appsettings (Smtp:Host)
            // En Azure: usar Smtp__Host, Smtp__Port, Smtp__User, Smtp__Password, Smtp__EnableSsl
            static int GetInt(string? value, int fallback) => int.TryParse(value, out var v) ? v : fallback;
            static bool GetBool(string? value, bool fallback) => bool.TryParse(value, out var v) ? v : fallback;

            var smtpConfig = builder.Configuration.GetSection("Smtp");
            var host = smtpConfig["Host"] ?? "smtp.gmail.com";
            var port = GetInt(smtpConfig["Port"], 587);
            var user = smtpConfig["User"] ?? "";
            var password = smtpConfig["Password"] ?? "";
            var enableSsl = GetBool(smtpConfig["EnableSsl"], true);

            builder.Services.Configure<SmtpSettings>(s =>
            {
                s.Host = host;
                s.Port = port;
                s.User = user;
                s.Password = password;
                s.EnableSsl = enableSsl;
            });

            // Log seguro (sin user/password)
            Console.WriteLine($"[SMTP Config] Host: {host}, Port: {port}, EnableSsl: {enableSsl}");
            if (string.IsNullOrEmpty(user))
                Console.WriteLine("[SMTP Config] ADVERTENCIA: Usuario SMTP no configurado. Los emails NO se enviarán.");

            builder.Services.AddSingleton<EmailTemplateService>();
            builder.Services.AddTransient<EmailSender>();

            // 7. CORS - Orígenes permitidos desde configuración
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // 8. SEGURIDAD JWT
            var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value ?? "Clave_Super_Secreta_Farmacia_2024");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            var app = builder.Build();

            // Logs útiles para Azure
            app.Logger.LogInformation("Environment: {env}", app.Environment.EnvironmentName);
            app.Logger.LogInformation("ASPNETCORE_URLS: {urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
            app.Logger.LogInformation("WEBSITES_PORT: {port}", Environment.GetEnvironmentVariable("WEBSITES_PORT"));

            // 9. Seeding (SIEMPRE en startup, pero seguro: no borra existentes)
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Error al sembrar la base de datos.");
            }

            // Swagger: habilitar en Development o si la config lo permite explícitamente
            var enableSwagger =
                app.Environment.IsDevelopment() ||
                builder.Configuration.GetValue<bool>("EnableSwagger");

            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowSpecificOrigins");

            // En App Service (contenedor), a veces forzar HTTPS rompe el warmup interno.
            // Si lo necesitás, dejalo solo en Dev o si lo habilitás por config.
            var forceHttps = builder.Configuration.GetValue<bool>("ForceHttpsRedirection");
            if (app.Environment.IsDevelopment() || forceHttps)
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            // Health endpoint (warmup probe)
            app.MapGet("/health", () => Results.Ok("ok"));

            app.MapControllers();
            app.Run();
        }
    }
}
            app.MapControllers();
            app.Run();
        }
    }
}
