using AutoMapper;
using Back.Data;
using Back.Repositories;
using Back.Repositories.Interfaces;
using Back.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace Back
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            

            // --- CONFIGURACI�N DE SERVICIOS ---

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configuraci�n de Base de Datos
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // AutoMapper
            builder.Services.AddAutoMapper(typeof(Back.Mappings.MappingProfile));

            // FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

            // --- CAPA DE REPOSITORIOS ---
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

            // Repositorios de Florencia (RF2, RF6)
            builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
            builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();

            // Repositorios Base
            builder.Services.AddScoped<IClientRepository, ClientRepository>(); // Asegurado con interfaz
            builder.Services.AddScoped<IProductRepository, ProductRepository>(); // Asegurado con interfaz
            builder.Services.AddScoped<ILocalityRepository, LocalityRepository>();

            // --- CAPA DE SERVICIOS ---
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ILocalidadService, LocalidadService>();
            builder.Services.AddScoped<IPedidoService, PedidoService>();

            // Servicios de Florencia (Pedidos y Flujo Operativo)
            builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
            builder.Services.AddScoped<ITrackingService, TrackingService>();

            // --- CONFIGURACI�N DE SEGURIDAD JWT ---
            var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value ?? "Clave_Super_Secreta_Farmacia_2024");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            var app = builder.Build();

            // --- INICIALIZACI�N DE DATOS (SEEDING) ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();

                    // ESTO BORRAR� TODA LA BASE DE DATOS Y LA CREAR� DE CERO
                    // �salo solo en desarrollo para limpiar las llaves for�neas rebeldes
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ocurri� un error al sembrar la base de datos.");
                }
            }

            // --- PIPELINE DE SOLICITUDES HTTP ---

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Habilitar CORS si es necesario para el Frontend
            app.UseCors(x => x

                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();

            // IMPORTANTE: Authentication debe ir SIEMPRE antes de Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
