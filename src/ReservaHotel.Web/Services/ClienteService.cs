using Microsoft.EntityFrameworkCore;
using ReservaHotel.Web.Data;
using ReservaHotel.Web.Models.Entities;

namespace ReservaHotel.Web.Services;

public interface IClienteService
{
    Task<Cliente?> ObtenerAsync(int codigo);
    Task<List<Cliente>> BuscarAsync(string? termino);
    Task<Cliente> RegistrarAsync(string nombres, string apellidos);
}

/// <summary>Soporte del CU incluido "Buscar Cliente" y del CU extendido "Mantener Clientes – Agregar Cliente".</summary>
public class ClienteService(AppDbContext db) : IClienteService
{
    public Task<Cliente?> ObtenerAsync(int codigo) =>
        db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Codigo == codigo);

    public async Task<List<Cliente>> BuscarAsync(string? termino)
    {
        var consulta = db.Clientes.AsNoTracking().AsQueryable();
        termino = termino?.Trim();
        if (!string.IsNullOrEmpty(termino))
        {
            if (int.TryParse(termino, out var codigo))
                consulta = consulta.Where(c => c.Codigo == codigo
                    || c.Nombres.Contains(termino) || c.Apellidos.Contains(termino));
            else
                consulta = consulta.Where(c => c.Nombres.Contains(termino)
                    || c.Apellidos.Contains(termino)
                    || (c.Nombres + " " + c.Apellidos).Contains(termino));
        }
        return await consulta.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).Take(200).ToListAsync();
    }

    public async Task<Cliente> RegistrarAsync(string nombres, string apellidos)
    {
        nombres = nombres?.Trim() ?? "";
        apellidos = apellidos?.Trim() ?? "";
        if (nombres.Length == 0 || apellidos.Length == 0)
            throw new ReservaException("Ingrese los nombres y los apellidos del cliente.");

        var cliente = new Cliente { Nombres = nombres, Apellidos = apellidos };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente;
    }
}
