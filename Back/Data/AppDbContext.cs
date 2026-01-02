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
        public DbSet<EstadoDePedido> EstadosDePedidos { get; set; }
        public DbSet<IntentoDeEntrega> IntentosDeEntregas { get; set; }
        public DbSet<HistorialDeEstados> HistorialesDeEstados { get; set; }

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
            modelBuilder.Entity<EstadoDePedido>().HasKey(e => e.IDEstadoDePedido);
            modelBuilder.Entity<IntentoDeEntrega>().HasKey(i => i.IDIntentoDeEntrega);
            modelBuilder.Entity<HistorialDeEstados>().HasKey(h => h.IDHistorialEstados);

            // REGLA DE ORO: En Pedidos, desactivamos el borrado en cascada para evitar el error 1785

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.IDUsuario) // Usamos tu variable
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

            base.OnModelCreating(modelBuilder);
        }
    }
}