namespace ReservaHotel.Web.Models.ViewModels;

public class ReservaViewModel
{
    public ReservaDraft Draft { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal TasaIgv { get; set; }
    public bool OfrecerRegistroCliente { get; set; }
}
