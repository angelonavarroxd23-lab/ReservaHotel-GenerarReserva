namespace ReservaHotel.Web.Models.ViewModels;

/// <summary>Campos editables de la interfaz RESERVA que se envían en cada acción.</summary>
public class ReservaForm
{
    public string? CodigoCliente { get; set; }
    public DateOnly? FechaLlegada { get; set; }
    public DateOnly? FechaSalida { get; set; }
    public string? NombreHuesped { get; set; }

    /// <summary>Habitación seleccionada en la cuadrícula (para "Eliminar Habitación").</summary>
    public int? HabitacionSeleccionada { get; set; }
}
