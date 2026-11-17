using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ReservaHotel.Web.Infrastructure;
using ReservaHotel.Web.Models.ViewModels;
using ReservaHotel.Web.Options;
using ReservaHotel.Web.Services;

namespace ReservaHotel.Web.Controllers;

/// <summary>
/// Clase de control «control» del caso de uso "Generar reserva de habitación".
/// Las vistas Reserva/* son la clase de frontera «boundary» (interfaz RESERVA y Buscar Habitación).
/// </summary>
public class ReservaController(
    IClienteService clientes,
    IHabitacionService habitaciones,
    IReservaService reservas,
    IOptions<HotelOptions> opciones) : Controller
{
    private ISession Sesion => HttpContext.Session;

    // ---- Inicio del caso de uso: "Generar Reserva" en el menú principal ----
    [HttpGet]
    public IActionResult Iniciar()
    {
        Sesion.LimpiarBorrador();
        return RedirectToAction(nameof(Nueva));
    }

    // ---- Paso 1: el sistema muestra la interfaz RESERVA ----
    [HttpGet]
    public IActionResult Nueva()
    {
        var vm = Construir(Sesion.ObtenerBorrador());
        vm.OfrecerRegistroCliente = TempData["OfrecerRegistro"] is true;
        return View(vm);
    }

    // ---- Pasos 2-4: Buscar Cliente (incluye el CU "Buscar Cliente") ----
    [HttpPost]
    public async Task<IActionResult> BuscarCliente(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);

        if (string.IsNullOrEmpty(d.CodigoClienteTexto))
        {
            // Sin código: se abre la búsqueda de clientes (CU incluido).
            Sesion.GuardarBorrador(d);
            return RedirectToAction("Buscar", "Clientes", new { retorno = "reserva" });
        }

        if (!int.TryParse(d.CodigoClienteTexto, out var codigo) || codigo <= 0)
        {
            TempData["Error"] = "El código del cliente debe ser un número.";
        }
        else
        {
            var cliente = await clientes.ObtenerAsync(codigo);
            if (cliente is null)
            {
                // Flujo alternativo "Cliente no existe" (paso 4): mensaje y ofrecer registrar al nuevo cliente.
                d.CodigoCliente = null;
                d.NombreCliente = null;
                TempData["Error"] = "Cliente no existe";
                TempData["OfrecerRegistro"] = true;
            }
            else
            {
                d.CodigoCliente = cliente.Codigo;
                d.NombreCliente = cliente.NombreCompleto;
            }
        }

        Sesion.GuardarBorrador(d);
        return RedirectToAction(nameof(Nueva));
    }

    /// <summary>Punto de extensión del paso 4: extiende a "Mantener Clientes – Agregar Cliente".</summary>
    [HttpPost]
    public IActionResult AgregarCliente(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);
        Sesion.GuardarBorrador(d);
        return RedirectToAction("Crear", "Clientes", new { retorno = "reserva" });
    }

    /// <summary>Retorno desde "Buscar Cliente" o desde "Agregar Cliente": carga los datos del cliente (paso 4).</summary>
    [HttpGet]
    public async Task<IActionResult> SeleccionarCliente(int codigo)
    {
        var d = Sesion.ObtenerBorrador();
        var cliente = await clientes.ObtenerAsync(codigo);
        if (cliente is null)
        {
            TempData["Error"] = "Cliente no existe";
            TempData["OfrecerRegistro"] = true;
        }
        else
        {
            d.CodigoClienteTexto = cliente.Codigo.ToString();
            d.CodigoCliente = cliente.Codigo;
            d.NombreCliente = cliente.NombreCompleto;
        }
        Sesion.GuardarBorrador(d);
        return RedirectToAction(nameof(Nueva));
    }

    // ---- Pasos 5-7: fechas y cálculo de la cantidad de días ----
    [HttpPost]
    public IActionResult FijarFechas(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);
        if (d.FechaLlegada.HasValue && d.FechaSalida.HasValue && d.CantidadDias <= 0)
            TempData["Error"] = "La fecha de salida debe ser posterior a la fecha de llegada.";
        Sesion.GuardarBorrador(d);
        return RedirectToAction(nameof(Nueva));
    }

    // ---- Paso 8: Buscar Habitación disponible ----
    [HttpPost]
    public IActionResult BuscarHabitacion(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);
        Sesion.GuardarBorrador(d);

        var error = ValidarParaHabitaciones(d);
        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Nueva));
        }
        return RedirectToAction(nameof(BuscarHabitacion));
    }

    // ---- Pasos 9-10: CU incluido "Buscar Habitación" ----
    [HttpGet]
    public async Task<IActionResult> BuscarHabitacion()
    {
        var d = Sesion.ObtenerBorrador();
        var error = ValidarParaHabitaciones(d);
        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Nueva));
        }

        var libres = await habitaciones.ObtenerDisponiblesAsync(d.Detalle.Select(x => x.NumeroHabitacion).ToList());
        if (libres.Count == 0)
        {
            if (d.Detalle.Count == 0)
            {
                // Flujo alternativo "Habitaciones no disponibles" (paso 10): mensaje y fin del caso de uso.
                Sesion.LimpiarBorrador();
                TempData["Advertencia"] = "No hay habitaciones disponibles";
                return RedirectToAction("Index", "Home");
            }
            // Ya hay habitaciones en la cuadrícula: no se descarta el trabajo de la Recepcionista.
            TempData["Advertencia"] = "No hay habitaciones disponibles";
            return RedirectToAction(nameof(Nueva));
        }

        return View(new BuscarHabitacionViewModel { Habitaciones = libres, Dias = d.CantidadDias });
    }

    /// <summary>Paso 10: el sistema muestra los datos de la habitación seleccionada.</summary>
    [HttpGet]
    public async Task<IActionResult> SeleccionarHabitacion(int numero)
    {
        var d = Sesion.ObtenerBorrador();
        var hab = await habitaciones.ObtenerAsync(numero);
        if (hab is null || hab.Estado != Models.Entities.EstadoHabitacion.Disponible)
            TempData["Advertencia"] = $"La habitación {numero} no está disponible.";
        else if (d.Detalle.Any(x => x.NumeroHabitacion == numero))
            TempData["Advertencia"] = $"La habitación {numero} ya está en la cuadrícula.";
        else
        {
            d.NumeroHabitacion = hab.Numero;
            d.TipoHabitacion = hab.Tipo;
            d.CostoPorDia = hab.CostoPorDia;
            d.NombreHuesped = null;
        }
        Sesion.GuardarBorrador(d);
        return RedirectToAction(nameof(Nueva));
    }

    // ---- Pasos 11-13: nombre del huésped y "Agregar Habitación" ----
    [HttpPost]
    public async Task<IActionResult> AgregarHabitacion(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);

        string? error = null;
        if (d.NumeroHabitacion is null)
            error = "Busque y seleccione una habitación.";
        else if (string.IsNullOrWhiteSpace(d.NombreHuesped))
            error = "Ingrese el nombre del huésped de la habitación.";
        else if (d.CantidadDias <= 0)
            error = "Ingrese una fecha de llegada y de salida válidas.";
        else if (d.Detalle.Any(x => x.NumeroHabitacion == d.NumeroHabitacion))
            error = $"La habitación {d.NumeroHabitacion} ya está en la cuadrícula.";

        if (error is null)
        {
            var hab = await habitaciones.ObtenerAsync(d.NumeroHabitacion!.Value);
            if (hab is null || hab.Estado != Models.Entities.EstadoHabitacion.Disponible)
            {
                error = $"La habitación {d.NumeroHabitacion} ya no está disponible.";
                LimpiarHabitacionEnEdicion(d);
            }
            else
            {
                // Paso 13: calcula el pago de la habitación y la agrega a la cuadrícula.
                d.Detalle.Add(new DetalleDraft
                {
                    NumeroHabitacion = hab.Numero,
                    Tipo = hab.Tipo,
                    NombreHuesped = d.NombreHuesped!,
                    CostoDia = hab.CostoPorDia,
                    Dias = d.CantidadDias,
                    MontoPagar = CalculadoraReserva.CalcularMontoHabitacion(hab.CostoPorDia, d.CantidadDias)
                });
                LimpiarHabitacionEnEdicion(d);
            }
        }

        if (error is not null) TempData["Error"] = error;
        Sesion.GuardarBorrador(d);
        return RedirectToAction(nameof(Nueva));
    }

    // ---- Flujo alternativo "Eliminar Habitación de Cuadrícula" ----
    [HttpPost]
    public IActionResult EliminarHabitacion(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);

        if (f.HabitacionSeleccionada is null)
            TempData["Error"] = "Seleccione una habitación de la cuadrícula.";
        else if (d.Detalle.RemoveAll(x => x.NumeroHabitacion == f.HabitacionSeleccionada) == 0)
            TempData["Error"] = "La habitación seleccionada no está en la cuadrícula.";
        else
            TempData["Mensaje"] = $"Habitación {f.HabitacionSeleccionada} eliminada de la cuadrícula.";

        Sesion.GuardarBorrador(d);
        return RedirectToAction(nameof(Nueva));
    }

    // ---- Pasos 15-18: Grabar Reserva ----
    [HttpPost]
    public async Task<IActionResult> Grabar(ReservaForm f)
    {
        var d = Sesion.ObtenerBorrador();
        Aplicar(d, f);
        Sesion.GuardarBorrador(d);

        if (d.CodigoCliente is null)
        {
            TempData["Error"] = "Busque y seleccione un cliente antes de grabar la reserva.";
            return RedirectToAction(nameof(Nueva));
        }
        if (d.FechaLlegada is null || d.FechaSalida is null)
        {
            TempData["Error"] = "Ingrese la fecha de llegada y la fecha de salida.";
            return RedirectToAction(nameof(Nueva));
        }

        try
        {
            var comando = new GrabarReservaCommand(
                d.CodigoCliente.Value, d.FechaLlegada.Value, d.FechaSalida.Value,
                d.Detalle.Select(x => new LineaReservaCommand(x.NumeroHabitacion, x.NombreHuesped)).ToList());

            var numero = await reservas.GrabarAsync(comando);

            Sesion.LimpiarBorrador();
            TempData["Mensaje"] = $"Reserva generada con el Nro. {numero}";
            return RedirectToAction(nameof(Detalle), new { id = numero });
        }
        catch (ReservaException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Nueva));
        }
    }

    // ---- Paso 19: cerrar la interfaz RESERVA y volver al menú principal ----
    [HttpPost]
    public IActionResult Salir()
    {
        Sesion.LimpiarBorrador();
        return RedirectToAction("Index", "Home");
    }

    // ---- Consultas de apoyo (verificar el registro de la reserva y su detalle) ----
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var reserva = await reservas.ObtenerAsync(id);
        if (reserva is null) return NotFound();
        ViewBag.TasaIgv = opciones.Value.TasaIgv;
        return View(reserva);
    }

    [HttpGet]
    public async Task<IActionResult> Listado() => View(await reservas.ListarAsync());

    // ---- Auxiliares ----
    private ReservaViewModel Construir(Models.ViewModels.ReservaDraft d)
    {
        var (sub, igv, total) = CalculadoraReserva.CalcularTotales(
            d.Detalle.Select(x => x.MontoPagar), opciones.Value.TasaIgv);
        return new ReservaViewModel
        {
            Draft = d, Subtotal = sub, Igv = igv, MontoTotal = total, TasaIgv = opciones.Value.TasaIgv
        };
    }

    private static void Aplicar(ReservaDraft d, ReservaForm f)
    {
        var texto = f.CodigoCliente?.Trim();
        // Si la Recepcionista cambió el código a mano, el cliente anterior ya no es válido.
        if (texto != d.CodigoCliente?.ToString())
        {
            d.CodigoCliente = null;
            d.NombreCliente = null;
        }
        d.CodigoClienteTexto = texto;
        d.FechaLlegada = f.FechaLlegada;
        d.FechaSalida = f.FechaSalida;
        d.NombreHuesped = f.NombreHuesped?.Trim();
        RecalcularDias(d);
    }

    private static void RecalcularDias(ReservaDraft d)
    {
        if (d.FechaLlegada is { } llegada && d.FechaSalida is { } salida
            && CalculadoraReserva.CalcularDias(llegada, salida) > 0)
        {
            d.CantidadDias = CalculadoraReserva.CalcularDias(llegada, salida);
            // Si cambian las fechas con habitaciones ya agregadas, se recalcula cada pago.
            foreach (var x in d.Detalle)
            {
                x.Dias = d.CantidadDias;
                x.MontoPagar = CalculadoraReserva.CalcularMontoHabitacion(x.CostoDia, x.Dias);
            }
        }
        else
        {
            d.CantidadDias = 0;
        }
    }

    private static string? ValidarParaHabitaciones(ReservaDraft d)
    {
        if (d.CodigoCliente is null)
            return "Busque y seleccione un cliente antes de buscar habitaciones.";
        if (d.CantidadDias <= 0)
            return "Ingrese una fecha de llegada y una fecha de salida válidas (la salida debe ser posterior a la llegada).";
        return null;
    }

    private static void LimpiarHabitacionEnEdicion(ReservaDraft d)
    {
        d.NumeroHabitacion = null;
        d.TipoHabitacion = null;
        d.CostoPorDia = null;
        d.NombreHuesped = null;
    }
}
