using Back.Models;
using Microsoft.EntityFrameworkCore;


namespace Back.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 1. Las Tablas (DbSets)
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetalleDePedido> DetallesDePedidos { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<Barrio> Barrios { get; set; } // <-- NUEVA TABLA
        public DbSet<Zona> Zonas { get; set; } // <-- NUEVA TABLA DE ZONAS
        public DbSet<EstadoDePedido> EstadosDePedidos { get; set; }
        public DbSet<IntentoDeEntrega> IntentosDeEntregas { get; set; }
        public DbSet<HistorialDeEstados> HistorialesDeEstados { get; set; }
        public DbSet<MotivoCancelacion> MotivosCancelacion { get; set; }

        // 2. La Configuración 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definir PKs
            modelBuilder.Entity<Usuario>().HasKey(u => u.IDUsuario);
            modelBuilder.Entity<Producto>().HasKey(p => p.IDProducto);
            modelBuilder.Entity<Pedido>().HasKey(p => p.IDPedido);
            modelBuilder.Entity<DetalleDePedido>().HasKey(d => d.IDDetalleDePedido);
            modelBuilder.Entity<Sucursal>().HasKey(s => s.IDSucursal);
            modelBuilder.Entity<Cliente>().HasKey(c => c.IDCliente);
            modelBuilder.Entity<Localidad>().HasKey(l => l.IDLocalidad);
            modelBuilder.Entity<Barrio>().HasKey(b => b.IDBarrio); // PK para Barrio
            modelBuilder.Entity<Zona>().HasKey(z => z.Id); // PK para Zona
            modelBuilder.Entity<EstadoDePedido>().HasKey(e => e.IDEstadoDePedido);
            modelBuilder.Entity<IntentoDeEntrega>().HasKey(i => i.IDIntentoDeEntrega);
            modelBuilder.Entity<HistorialDeEstados>().HasKey(h => h.IDHistorialEstados);

            // --- CONFIGURACIÓN DE PRECISIÓN DECIMAL ---

            modelBuilder.Entity<DetalleDePedido>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioProducto)
                .HasPrecision(18, 2);

            // --- CONFIGURACIÓN DE RELACIONES ---

            // Relación Localidad -> Barrios (Una localidad tiene muchos barrios)
            modelBuilder.Entity<Barrio>()
                .HasOne(b => b.Localidad)
                .WithMany(l => l.Barrios)
                .HasForeignKey(b => b.IDLocalidad)
                .OnDelete(DeleteBehavior.NoAction);

            // Relaciones de Pedido (Existentes)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.IDUsuario)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Sucursal)
                .WithMany(s => s.Pedidos)
                .HasForeignKey(p => p.IDSucursal)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.IDCliente)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.EstadoDePedido)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(p => p.IDEstadoDePedido)
                .OnDelete(DeleteBehavior.NoAction);

            // --- CONFIGURACIÓN DE RELACIONES CON ZONAS ---

            // Relación Zona -> Barrios (Una zona tiene muchos barrios)
            modelBuilder.Entity<Barrio>()
                .HasOne(b => b.Zona)
                .WithMany(z => z.Barrios)
                .HasForeignKey(b => b.ZonaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación Zona -> Usuarios (Cadetes) (Una zona tiene muchos cadetes)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Zona)
                .WithMany(z => z.Cadetes)
                .HasForeignKey(u => u.ZonaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación Zona -> Pedidos (Una zona tiene muchos pedidos)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Zona)
                .WithMany(z => z.Pedidos)
                .HasForeignKey(p => p.ZonaId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}