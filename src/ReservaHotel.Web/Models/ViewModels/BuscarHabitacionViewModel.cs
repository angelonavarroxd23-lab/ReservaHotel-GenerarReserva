using ReservaHotel.Web.Models.Entities;

namespace ReservaHotel.Web.Models.ViewModels;

public class BuscarHabitacionViewModel
{
    public List<Habitacion> Habitaciones { get; set; } = new();
    public int Dias { get; set; }
}
