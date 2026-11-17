using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReservaHotel.Web.Data;
using ReservaHotel.Web.Models.Entities;
using ReservaHotel.Web.Options;

namespace ReservaHotel.Web.Services;

public record LineaReservaCommand(int NumeroHabitacion, string NombreHuesped);

public record GrabarReservaCommand(
    int CodigoCliente,
    DateOnly FechaLlegada,
    DateOnly FechaSalida,
    IReadOnlyList<LineaReservaCommand> Lineas);

public interface IReservaService
{
    /// <summary>Pasos 16-17 del flujo básico. Devuelve el número de reserva generado.</summary>
    Task<int> GrabarAsync(GrabarReservaCommand comando, CancellationToken ct = default);
    Task<Reserva?> ObtenerAsync(int numeroReserva);
    Task<List<Reserva>> ListarAsync();
}

public class ReservaService(AppDbContext db, IOptions<HotelOptions> opciones, ILogger<ReservaService> log)
    : IReservaService
{
    private const int MaxIntentos = 3;
    private const int SqlDeadlock = 1205;

    public async Task<int> GrabarAsync(GrabarReservaCommand comando, CancellationToken ct = default)
    {
        Validar(comando);

        for (var intento = 1; ; intento++)
        {
            try
            {
                return await GrabarEnTransaccionAsync(comando, ct);
            }
            catch (Exception ex) when (EsDeadlock(ex) && intento < MaxIntentos)
            {
                // Dos recepcionistas grabando a la vez: se reintenta con el contexto limpio.
                log.LogWarning(ex, "Deadlock al grabar la reserva (intento {Intento}).", intento);
                db.ChangeTracker.Clear();
            }
            catch (Exception ex) when (EsDeadlock(ex))
            {
                throw new ReservaException("No se pudo generar la reserva por concurrencia. Intente nuevamente.");
            }
            catch (DbUpdateException ex)
            {
                log.LogError(ex, "Error de base de datos al grabar la reserva.");
                throw new ReservaException("No se pudo grabar la reserva. Verifique los datos e intente nuevamente.");
            }
        }
    }

    private async Task<int> GrabarEnTransaccionAsync(GrabarReservaCommand c, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Codigo == c.CodigoCliente, ct)
            ?? throw new ReservaException("Cliente no existe");

        var numeros = c.Lineas.Select(l => l.NumeroHabitacion).ToList();
        var habitaciones = await db.Habitaciones.Where(h => numeros.Contains(h.Numero)).ToListAsync(ct);

        foreach (var linea in c.Lineas)
        {
            var hab = habitaciones.FirstOrDefault(h => h.Numero == linea.NumeroHabitacion)
                ?? throw new ReservaException($"La habitación {linea.NumeroHabitacion} no existe.");
            if (hab.Estado != EstadoHabitacion.Disponible)
                throw new ReservaException(
                    $"La habitación {hab.Numero} ya no está disponible (estado: {hab.Estado}). Elimínela de la cuadrícula.");
        }

        // Paso 16: obtener el último número de reserva y autogenerar el siguiente.
        var ultimo = await db.Reservas.MaxAsync(r => (int?)r.NumeroReserva, ct) ?? 0;
        var numeroReserva = ultimo + 1;

        // Los montos se recalculan en el servidor con el costo vigente de cada habitación.
        var dias = CalculadoraReserva.CalcularDias(c.FechaLlegada, c.FechaSalida);
        var detalle = c.Lineas.Select(l =>
        {
            var hab = habitaciones.First(h => h.Numero == l.NumeroHabitacion);
            return new DetalleReserva
            {
                NumeroReserva = numeroReserva,
                NumeroHabitacion = hab.Numero,
                NombreHuesped = l.NombreHuesped.Trim(),
                CostoDia = hab.CostoPorDia,
                Dias = dias,
                MontoPagar = CalculadoraReserva.CalcularMontoHabitacion(hab.CostoPorDia, dias)
            };
        }).ToList();

        var (subtotal, igv, total) = CalculadoraReserva.CalcularTotales(
            detalle.Select(d => d.MontoPagar), opciones.Value.TasaIgv);

        db.Reservas.Add(new Reserva
        {
            NumeroReserva = numeroReserva,
            CodigoCliente = cliente.Codigo,
            FechaLlegada = c.FechaLlegada,
            FechaSalida = c.FechaSalida,
            CantidadDias = dias,
            Subtotal = subtotal,
            Igv = igv,
            MontoTotal = total,
            Detalle = detalle
        });

        // Paso 17: actualizar la disponibilidad de las habitaciones a "Reservado".
        foreach (var hab in habitaciones) hab.Estado = EstadoHabitacion.Reservado;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return numeroReserva;
    }

    private static void Validar(GrabarReservaCommand c)
    {
        if (c.CodigoCliente <= 0)
            throw new ReservaException("Seleccione un cliente.");
        if (CalculadoraReserva.CalcularDias(c.FechaLlegada, c.FechaSalida) <= 0)
            throw new ReservaException("La fecha de salida debe ser posterior a la fecha de llegada.");
        if (c.Lineas.Count == 0)
            throw new ReservaException("Agregue al menos una habitación a la reserva.");
        if (c.Lineas.Any(l => string.IsNullOrWhiteSpace(l.NombreHuesped)))
            throw new ReservaException("Cada habitación debe tener el nombre del huésped.");
        if (c.Lineas.Select(l => l.NumeroHabitacion).Distinct().Count() != c.Lineas.Count)
            throw new ReservaException("Una habitación no puede repetirse en la misma reserva.");
    }

    private static bool EsDeadlock(Exception ex) =>
        ex.GetBaseException() is SqlException { Number: SqlDeadlock };

    public Task<Reserva?> ObtenerAsync(int numeroReserva) =>
        db.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.Detalle).ThenInclude(d => d.Habitacion)
            .FirstOrDefaultAsync(r => r.NumeroReserva == numeroReserva);

    public Task<List<Reserva>> ListarAsync() =>
        db.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.Detalle)
            .OrderByDescending(r => r.NumeroReserva)
            .Take(200)
            .ToListAsync();
}
