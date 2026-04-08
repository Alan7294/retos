using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using pw2_clase5.Models;

namespace pw2_clase5.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reto> Retos { get; set; }
        public DbSet<Solucion> Soluciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Forzar uso de SERIAL en lugar de IDENTITY
            modelBuilder.Entity<Usuario>()
                .Property(u => u.IdUsuario)
                .UseSerialColumn();

            modelBuilder.Entity<Reto>()
                .Property(r => r.IdReto)
                .UseSerialColumn();

            modelBuilder.Entity<Solucion>()
                .Property(s => s.IdSolucion)
                .UseSerialColumn();

            // Relación Solucion -> Usuario
            modelBuilder.Entity<Solucion>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.Soluciones)
                .HasForeignKey(s => s.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Solucion -> Reto
            modelBuilder.Entity<Solucion>()
                .HasOne(s => s.Reto)
                .WithMany(r => r.Soluciones)
                .HasForeignKey(s => s.IdReto)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para evitar doble puntaje
            modelBuilder.Entity<Solucion>()
                .HasIndex(s => new { s.IdUsuario, s.IdReto })
                .IsUnique()
                .HasFilter("estado = 'correcto'");

            // 🔑 Conversión global de DateTime a UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                }
            }
        }
    }
}
