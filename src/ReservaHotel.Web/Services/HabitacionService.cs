using Microsoft.EntityFrameworkCore;
using ReservaHotel.Web.Data;
using ReservaHotel.Web.Models.Entities;

namespace ReservaHotel.Web.Services;

public interface IHabitacionService
{
    Task<Habitacion?> ObtenerAsync(int numero);
    Task<List<Habitacion>> ListarAsync();
    Task<List<Habitacion>> ObtenerDisponiblesAsync(IReadOnlyCollection<int>? excluir = null);
}

/// <summary>Soporte del CU incluido "Buscar Habitación" y de la consulta de estados.</summary>
public class HabitacionService(AppDbContext db) : IHabitacionService
{
    public Task<Habitacion?> ObtenerAsync(int numero) =>
        db.Habitaciones.AsNoTracking().FirstOrDefaultAsync(h => h.Numero == numero);

    public Task<List<Habitacion>> ListarAsync() =>
        db.Habitaciones.AsNoTracking().OrderBy(h => h.Numero).ToListAsync();

    public Task<List<Habitacion>> ObtenerDisponiblesAsync(IReadOnlyCollection<int>? excluir = null)
    {
        var numerosExcluidos = excluir ?? Array.Empty<int>();
        return db.Habitaciones.AsNoTracking()
            .Where(h => h.Estado == EstadoHabitacion.Disponible && !numerosExcluidos.Contains(h.Numero))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }
}
