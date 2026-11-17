namespace ReservaHotel.Web.Models.ViewModels;

/// <summary>Una fila de la cuadrícula "Detalle de la Reserva" (aún sin grabar).</summary>
public class DetalleDraft
{
    public int NumeroHabitacion { get; set; }
    public string Tipo { get; set; } = "";
    public string NombreHuesped { get; set; } = "";
    public decimal CostoDia { get; set; }
    public int Dias { get; set; }
    public decimal MontoPagar { get; set; }
}

/// <summary>
/// Estado de la interfaz RESERVA mientras la Recepcionista la completa. Se conserva en la sesión
/// hasta "Grabar Reserva" o "Salir"; nada se persiste en la base de datos antes de grabar.
/// </summary>
public class ReservaDraft
{
    // Datos del cliente
    public string? CodigoClienteTexto { get; set; }
    public int? CodigoCliente { get; set; }
    public string? NombreCliente { get; set; }

    // Datos de la reserva
    public DateOnly? FechaLlegada { get; set; }
    public DateOnly? FechaSalida { get; set; }
    public int CantidadDias { get; set; }

    // Datos de la habitación en edición
    public int? NumeroHabitacion { get; set; }
    public string? TipoHabitacion { get; set; }
    public decimal? CostoPorDia { get; set; }
    public string? NombreHuesped { get; set; }

    // Cuadrícula
    public List<DetalleDraft> Detalle { get; set; } = new();
}
