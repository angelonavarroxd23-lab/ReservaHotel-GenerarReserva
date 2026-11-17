namespace ReservaHotel.Web.Models.Entities;

/// <summary>Clase de entidad «entity» Reserva (cabecera).</summary>
public class Reserva
{
    public int NumeroReserva { get; set; }
    public int CodigoCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateOnly FechaLlegada { get; set; }
    public DateOnly FechaSalida { get; set; }
    public int CantidadDias { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal MontoTotal { get; set; }

    public ICollection<DetalleReserva> Detalle { get; set; } = new List<DetalleReserva>();
}
