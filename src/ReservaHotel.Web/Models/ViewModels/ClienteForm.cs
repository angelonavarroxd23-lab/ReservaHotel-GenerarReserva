using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Web.Models.ViewModels;

public class ClienteForm
{
    [Required(ErrorMessage = "Ingrese los nombres."), StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Ingrese los apellidos."), StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = "";

    /// <summary>"reserva" cuando se llega desde la interfaz RESERVA (punto de extensión del paso 4).</summary>
    public string? Retorno { get; set; }
}
