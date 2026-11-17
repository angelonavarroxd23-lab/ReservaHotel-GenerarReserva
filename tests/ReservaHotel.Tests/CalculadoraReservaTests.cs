using ReservaHotel.Web.Services;

namespace ReservaHotel.Tests;

public class CalculadoraReservaTests
{
    [Theory]
    [InlineData("2026-12-01", "2026-12-04", 3)]
    [InlineData("2026-12-30", "2027-01-02", 3)]   // cruza de año
    [InlineData("2028-02-28", "2028-03-01", 2)]   // año bisiesto
    [InlineData("2026-12-01", "2026-12-01", 0)]
    [InlineData("2026-12-05", "2026-12-01", -4)]
    public void CalcularDias_EsSalidaMenosLlegada(string llegada, string salida, int esperado) =>
        Assert.Equal(esperado, CalculadoraReserva.CalcularDias(DateOnly.Parse(llegada), DateOnly.Parse(salida)));

    [Fact]
    public void CalcularMontoHabitacion_EsCostoPorDias() =>
        Assert.Equal(360m, CalculadoraReserva.CalcularMontoHabitacion(120m, 3));

    [Fact]
    public void CalcularTotales_SubtotalIgvYTotal()
    {
        var (sub, igv, total) = CalculadoraReserva.CalcularTotales(new[] { 240m, 360m }, 0.18m);
        Assert.Equal(600m, sub);
        Assert.Equal(108m, igv);
        Assert.Equal(708m, total);
    }

    [Fact]
    public void CalcularTotales_RedondeaADosDecimales()
    {
        // 33.33 * 0.18 = 5.9994 -> 6.00 ; total 39.33
        var (sub, igv, total) = CalculadoraReserva.CalcularTotales(new[] { 33.33m }, 0.18m);
        Assert.Equal(33.33m, sub);
        Assert.Equal(6.00m, igv);
        Assert.Equal(39.33m, total);
    }

    [Fact]
    public void CalcularTotales_SinHabitaciones_EsCero()
    {
        var (sub, igv, total) = CalculadoraReserva.CalcularTotales(Array.Empty<decimal>(), 0.18m);
        Assert.Equal((0m, 0m, 0m), (sub, igv, total));
    }
}
