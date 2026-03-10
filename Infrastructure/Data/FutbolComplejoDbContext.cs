using FutbolComplejo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FutbolComplejo.Infrastructure.Data;

public class FutbolComplejoDbContext : DbContext
{
    public FutbolComplejoDbContext(DbContextOptions<FutbolComplejoDbContext> options)
        : base(options) { }

    public DbSet<Cancha> Canchas => Set<Cancha>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<ListaEspera> ListasEspera => Set<ListaEspera>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Cancha ────────────────────────────────────────────────────────
        modelBuilder.Entity<Cancha>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            e.Property(c => c.Descripcion).HasMaxLength(300);
            e.Property(c => c.PrecioPorHora).HasColumnType("decimal(10,2)");
            e.Property(c => c.Tipo).HasConversion<int>();

            // Seed: las 3 canchas del enunciado
            e.HasData(
                new Cancha { Id = 1, Nombre = "Cancha 1 - Fútbol 7", Descripcion = "Cancha de fútbol 7 con césped sintético premium.", Tipo = Domain.Enums.TipoCancha.Futbol7, PrecioPorHora = 45_000, CapacidadJugadores = 14, EstaActiva = true, FechaCreacion = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Cancha { Id = 2, Nombre = "Cancha 2 - Fútbol 5", Descripcion = "Cancha de fútbol 5 con iluminación LED.", Tipo = Domain.Enums.TipoCancha.Futbol5, PrecioPorHora = 35_000, CapacidadJugadores = 10, EstaActiva = true, FechaCreacion = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Cancha { Id = 3, Nombre = "Cancha 3 - Fútbol 5", Descripcion = "Cancha de fútbol 5 techada.", Tipo = Domain.Enums.TipoCancha.Futbol5, PrecioPorHora = 30_000, CapacidadJugadores = 10, EstaActiva = true, FechaCreacion = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        // ── Usuario ───────────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.NombreCompleto).HasMaxLength(150).IsRequired();
            e.Property(u => u.Telefono).HasMaxLength(20).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150);
            e.HasIndex(u => u.Telefono).IsUnique();
        });

        // ── Turno ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Turno>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.PrecioTotal).HasColumnType("decimal(10,2)");
            e.Property(t => t.NombreEquipo).HasMaxLength(100);
            e.Property(t => t.NotasAdicionales).HasMaxLength(500);
            e.Property(t => t.MotivoCancelacion).HasMaxLength(300);
            e.Property(t => t.Estado).HasConversion<int>();

            e.HasOne(t => t.Cancha)
             .WithMany(c => c.Turnos)
             .HasForeignKey(t => t.CanchaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Usuario)
             .WithMany(u => u.Turnos)
             .HasForeignKey(t => t.UsuarioId)
             .OnDelete(DeleteBehavior.Restrict);

            // Índice compuesto para búsquedas de disponibilidad eficientes
            e.HasIndex(t => new { t.CanchaId, t.FechaHoraInicio, t.FechaHoraFin });

        });

        // ── ListaEspera ───────────────────────────────────────────────────
        modelBuilder.Entity<ListaEspera>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.TelefonoWhatsApp).HasMaxLength(20).IsRequired();

            e.HasOne(l => l.Turno)
             .WithMany(t => t.ListasEspera)
             .HasForeignKey(l => l.TurnoId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.Usuario)
             .WithMany(u => u.ListasEspera)
             .HasForeignKey(l => l.UsuarioId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Cancha)
             .WithMany()
             .HasForeignKey(l => l.CanchaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(l => new { l.CanchaId, l.FechaHoraDeseada, l.Posicion });
        });
    }
}
