using System.Globalization;

namespace ReservaHotel.Web.Infrastructure;

public static class Formato
{
    private static readonly CultureInfo Pe = new("es-PE");

    public static string Soles(this decimal valor) => "S/ " + valor.ToString("N2", Pe);
    public static string Soles(this decimal? valor) => valor.HasValue ? valor.Value.Soles() : "";
    public static string Fecha(this DateOnly fecha) => fecha.ToString("dd/MM/yyyy", Pe);
    public static string Fecha(this DateOnly? fecha) => fecha.HasValue ? fecha.Value.Fecha() : "";
    public static string Iso(this DateOnly? fecha) => fecha?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
}
