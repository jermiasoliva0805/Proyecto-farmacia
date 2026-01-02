using Microsoft.EntityFrameworkCore;
using Back.Data; // Ajustá según tu namespace

namespace Back.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Por ahora dejamos esto para que la base no nazca vacía
        public DbSet<Usuario> Usuarios { get; set; }
    }
}