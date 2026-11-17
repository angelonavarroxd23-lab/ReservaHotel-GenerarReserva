using System.Text.Json;
using ReservaHotel.Web.Models.ViewModels;

namespace ReservaHotel.Web.Infrastructure;

public static class SessionExtensions
{
    private const string Clave = "ReservaDraft";

    public static ReservaDraft ObtenerBorrador(this ISession session)
    {
        var json = session.GetString(Clave);
        if (json is null) return new ReservaDraft();
        try { return JsonSerializer.Deserialize<ReservaDraft>(json) ?? new ReservaDraft(); }
        catch (JsonException) { return new ReservaDraft(); }
    }

    public static void GuardarBorrador(this ISession session, ReservaDraft borrador) =>
        session.SetString(Clave, JsonSerializer.Serialize(borrador));

    public static void LimpiarBorrador(this ISession session) => session.Remove(Clave);
}
