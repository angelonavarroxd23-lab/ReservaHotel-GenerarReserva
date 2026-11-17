namespace ReservaHotel.Web.Models.Entities;

/// <summary>Clase de entidad «entity» Cliente (Código, Nombres y Apellidos según el ECU).</summary>
public class Cliente
{
    public int Codigo { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
