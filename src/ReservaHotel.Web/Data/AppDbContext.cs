using Microsoft.EntityFrameworkCore;
using ReservaHotel.Web.Models.Entities;

namespace ReservaHotel.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<DetalleReserva> DetallesReserva => Set<DetalleReserva>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(c => c.Codigo);
            e.Property(c => c.Nombres).HasMaxLength(80).IsRequired();
            e.Property(c => c.Apellidos).HasMaxLength(80).IsRequired();
            e.Ignore(c => c.NombreCompleto);

            // Datos de ejemplo: cumplen la precondición "Lista de Clientes disponibles".
            e.HasData(
                new Cliente { Codigo = 1, Nombres = "Carlos Alberto", Apellidos = "Ramírez Soto" },
                new Cliente { Codigo = 2, Nombres = "María Fernanda", Apellidos = "Quispe Huamán" },
                new Cliente { Codigo = 3, Nombres = "Luis Miguel", Apellidos = "Torres Vargas" },
                new Cliente { Codigo = 4, Nombres = "Ana Lucía", Apellidos = "Flores Medina" },
                new Cliente { Codigo = 5, Nombres = "José Antonio", Apellidos = "Castillo Rojas" });
        });

        mb.Entity<Habitacion>(e =>
        {
            e.ToTable("Habitaciones", t =>
            {
                t.HasCheckConstraint("CK_Habitaciones_Costo", "[CostoPorDia] > 0");
                t.HasCheckConstraint("CK_Habitaciones_Estado", "[Estado] IN (N'Disponible', N'Reservado', N'Ocupado')");
            });
            e.HasKey(h => h.Numero);
            e.Property(h => h.Numero).ValueGeneratedNever();
            e.Property(h => h.Tipo).HasMaxLength(30).IsRequired();
            e.Property(h => h.CostoPorDia).HasPrecision(10, 2);
            e.Property(h => h.Estado).HasConversion<string>().HasMaxLength(15).IsRequired();

            // Datos de ejemplo: cumplen la precondición "Lista de habitaciones disponibles"
            // y muestran los tres estados (Disponible, Reservado, Ocupado).
            e.HasData(
                new Habitacion { Numero = 101, Tipo = "Simple", CostoPorDia = 80m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 102, Tipo = "Simple", CostoPorDia = 80m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 103, Tipo = "Simple", CostoPorDia = 80m, Estado = EstadoHabitacion.Ocupado },
                new Habitacion { Numero = 201, Tipo = "Doble", CostoPorDia = 120m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 202, Tipo = "Doble", CostoPorDia = 120m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 203, Tipo = "Doble", CostoPorDia = 120m, Estado = EstadoHabitacion.Reservado },
                new Habitacion { Numero = 301, Tipo = "Matrimonial", CostoPorDia = 150m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 302, Tipo = "Matrimonial", CostoPorDia = 150m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 401, Tipo = "Suite", CostoPorDia = 250m, Estado = EstadoHabitacion.Disponible },
                new Habitacion { Numero = 402, Tipo = "Suite", CostoPorDia = 250m, Estado = EstadoHabitacion.Ocupado });
        });

        mb.Entity<Reserva>(e =>
        {
            e.ToTable("Reservas", t =>
            {
                t.HasCheckConstraint("CK_Reservas_Fechas", "[FechaSalida] > [FechaLlegada]");
                t.HasCheckConstraint("CK_Reservas_Dias", "[CantidadDias] > 0");
            });
            e.HasKey(r => r.NumeroReserva);
            // El número lo autogenera el sistema (último número + 1), no una columna IDENTITY (paso 16 del ECU).
            e.Property(r => r.NumeroReserva).ValueGeneratedNever();
            e.Property(r => r.Subtotal).HasPrecision(12, 2);
            e.Property(r => r.Igv).HasPrecision(12, 2);
            e.Property(r => r.MontoTotal).HasPrecision(12, 2);
            e.HasOne(r => r.Cliente).WithMany(c => c.Reservas)
                .HasForeignKey(r => r.CodigoCliente).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<DetalleReserva>(e =>
        {
            e.ToTable("DetallesReserva", t =>
            {
                t.HasCheckConstraint("CK_DetallesReserva_Dias", "[Dias] > 0");
            });
            e.HasKey(d => new { d.NumeroReserva, d.NumeroHabitacion });
            e.Property(d => d.NombreHuesped).HasMaxLength(120).IsRequired();
            e.Property(d => d.CostoDia).HasPrecision(10, 2);
            e.Property(d => d.MontoPagar).HasPrecision(12, 2);
            e.HasOne(d => d.Reserva).WithMany(r => r.Detalle)
                .HasForeignKey(d => d.NumeroReserva).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Habitacion).WithMany(h => h.Detalles)
                .HasForeignKey(d => d.NumeroHabitacion).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
