using Microsoft.AspNetCore.Mvc;
using ReservaHotel.Web.Services;

namespace ReservaHotel.Web.Controllers;

/// <summary>Consulta de las habitaciones y su estado: reservado, ocupado o disponible.</summary>
public class HabitacionesController(IHabitacionService habitaciones) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await habitaciones.ListarAsync());
}
