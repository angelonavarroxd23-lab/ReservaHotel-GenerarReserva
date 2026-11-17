namespace ReservaHotel.Web.Options;

/// <summary>Parámetros de negocio configurables (appsettings.json, sección "Hotel").</summary>
public class HotelOptions
{
    public const string Seccion = "Hotel";

    /// <summary>
    /// Tasa de IGV aplicada sobre el subtotal. El documento del caso de uso muestra el campo IGV
    /// pero no fija la tasa; se configura aquí (18 % por defecto, tasa vigente en el Perú).
    /// </summary>
    public decimal TasaIgv { get; set; } = 0.18m;
}
