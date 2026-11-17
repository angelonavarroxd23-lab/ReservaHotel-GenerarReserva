namespace ReservaHotel.Web.Services;

/// <summary>
/// Reglas de cálculo del caso de uso (pasos 6, 13 y 17 del flujo básico). Lógica pura, sin acceso a datos.
/// </summary>
public static class CalculadoraReserva
{
    /// <summary>Cantidad de días de hospedaje = fecha de salida − fecha de llegada.</summary>
    public static int CalcularDias(DateOnly llegada, DateOnly salida) => salida.DayNumber - llegada.DayNumber;

    /// <summary>Pago de una habitación = costo por día × días.</summary>
    public static decimal CalcularMontoHabitacion(decimal costoDia, int dias) => Redondear(costoDia * dias);

    /// <summary>Subtotal (suma de los pagos), IGV (subtotal × tasa) y monto total.</summary>
    public static (decimal Subtotal, decimal Igv, decimal MontoTotal) CalcularTotales(
        IEnumerable<decimal> montos, decimal tasaIgv)
    {
        var subtotal = Redondear(montos.Sum());
        var igv = Redondear(subtotal * tasaIgv);
        return (subtotal, igv, Redondear(subtotal + igv));
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
