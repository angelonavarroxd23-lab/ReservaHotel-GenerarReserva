using Microsoft.AspNetCore.Mvc;
using ReservaHotel.Web.Models.ViewModels;
using ReservaHotel.Web.Services;

namespace ReservaHotel.Web.Controllers;

/// <summary>
/// Soporta el CU incluido "Buscar Cliente" y el CU extendido "Mantener Clientes – Agregar Cliente".
/// </summary>
public class ClientesController(IClienteService clientes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await clientes.BuscarAsync(q));
    }

    /// <summary>Búsqueda de cliente invocada desde la interfaz RESERVA (paso 3).</summary>
    [HttpGet]
    public async Task<IActionResult> Buscar(string? q, string? retorno)
    {
        ViewBag.Q = q;
        ViewBag.Retorno = retorno;
        return View(await clientes.BuscarAsync(q));
    }

    [HttpGet]
    public IActionResult Crear(string? retorno) => View(new ClienteForm { Retorno = retorno });

    [HttpPost]
    public async Task<IActionResult> Crear(ClienteForm form)
    {
        if (!ModelState.IsValid) return View(form);

        try
        {
            var cliente = await clientes.RegistrarAsync(form.Nombres, form.Apellidos);
            TempData["Mensaje"] = $"Cliente registrado con el código {cliente.Codigo}.";
            return form.Retorno == "reserva"
                ? RedirectToAction("SeleccionarCliente", "Reserva", new { codigo = cliente.Codigo })
                : RedirectToAction(nameof(Index));
        }
        catch (ReservaException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }
    }
}
