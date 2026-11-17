namespace ReservaHotel.Web.Models.Entities;

/// <summary>Clase de entidad «entity» Detalle de la reserva (una fila de la cuadrícula).</summary>
public class DetalleReserva
{
    public int NumeroReserva { get; set; }
    public Reserva Reserva { get; set; } = null!;
    public int NumeroHabitacion { get; set; }
    public Habitacion Habitacion { get; set; } = null!;
    public string NombreHuesped { get; set; } = string.Empty;
    public decimal CostoDia { get; set; }
    public int Dias { get; set; }
    public decimal MontoPagar { get; set; }
}
