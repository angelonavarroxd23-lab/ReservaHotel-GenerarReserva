namespace ReservaHotel.Web.Models.Entities;

/// <summary>Clase de entidad «entity» Habitación (Número, Tipo, Costo por día y estado).</summary>
public class Habitacion
{
    public int Numero { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal CostoPorDia { get; set; }
    public EstadoHabitacion Estado { get; set; } = EstadoHabitacion.Disponible;

    public ICollection<DetalleReserva> Detalles { get; set; } = new List<DetalleReserva>();
}
