namespace ReservaHotel.Web.Services;

/// <summary>Error de regla de negocio con mensaje apto para mostrarse a la Recepcionista.</summary>
public class ReservaException(string message) : Exception(message);
