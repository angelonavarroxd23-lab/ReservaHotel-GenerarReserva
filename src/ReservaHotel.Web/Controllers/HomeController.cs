using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReservaHotel.Web.Models;

namespace ReservaHotel.Web.Controllers;

/// <summary>Interfaz del menú principal del sistema.</summary>
public class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code = null) =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Codigo = code });
}
