using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ReservaHotel.Web.Data;
using ReservaHotel.Web.Models.Entities;
using ReservaHotel.Web.Options;
using ReservaHotel.Web.Services;

namespace ReservaHotel.Tests;

/// <summary>
/// Pruebas de integración contra SQL Server real. Cada prueba crea su propia base de datos
/// (aplicando las migraciones) y la elimina al terminar.
/// Servidor: variable de entorno RESERVA_TEST_SERVER (por defecto: localhost con autenticación de Windows).
/// </summary>
public class ServiciosIntegracionTests : IAsyncLifetime
{
    private readonly string _cadena;

    public ServiciosIntegracionTests()
    {
        var servidor = Environment.GetEnvironmentVariable("RESERVA_TEST_SERVER")
                       ?? "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True;";
        _cadena = $"{servidor.TrimEnd(';')};Database=ReservaHotel_Test_{Guid.NewGuid():N};";
    }

    private AppDbContext Ctx() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_cadena).Options);

    private static ReservaService Servicio(AppDbContext ctx) =>
        new(ctx, Options.Create(new HotelOptions { TasaIgv = 0.18m }), NullLogger<ReservaService>.Instance);

    public async Task InitializeAsync()
    {
        await using var ctx = Ctx();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var ctx = Ctx();
        await ctx.Database.EnsureDeletedAsync();
    }

    private static GrabarReservaCommand Cmd(int cliente, params (int hab, string huesped)[] lineas) =>
        new(cliente, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 4),
            lineas.Select(l => new LineaReservaCommand(l.hab, l.huesped)).ToList());

    [Fact]
    public async Task Migracion_CargaDatosDeEjemplo()
    {
        await using var ctx = Ctx();
        Assert.Equal(5, await ctx.Clientes.CountAsync());
        Assert.Equal(10, await ctx.Habitaciones.CountAsync());
        Assert.Equal(7, await ctx.Habitaciones.CountAsync(h => h.Estado == EstadoHabitacion.Disponible));
    }

    [Fact]
    public async Task Grabar_GeneraNumeroConsecutivo_CalculaMontos_RegistraDetalle_YReservaHabitaciones()
    {
        int n1, n2;
        await using (var ctx = Ctx())
        {
            var s = Servicio(ctx);
            n1 = await s.GrabarAsync(Cmd(1, (101, "Juan Pérez"), (201, "Ana Díaz")));
            n2 = await s.GrabarAsync(Cmd(2, (301, "Luis Soto")));
        }
        Assert.Equal(1, n1);
        Assert.Equal(2, n2);

        await using var v = Ctx();
        var r = await v.Reservas.Include(x => x.Detalle).SingleAsync(x => x.NumeroReserva == 1);
        Assert.Equal(1, r.CodigoCliente);
        Assert.Equal(3, r.CantidadDias);
        Assert.Equal(600m, r.Subtotal);   // 3*80 + 3*120
        Assert.Equal(108m, r.Igv);
        Assert.Equal(708m, r.MontoTotal);
        Assert.Equal(2, r.Detalle.Count);
        var d101 = r.Detalle.Single(d => d.NumeroHabitacion == 101);
        Assert.Equal("Juan Pérez", d101.NombreHuesped);
        Assert.Equal(80m, d101.CostoDia);
        Assert.Equal(3, d101.Dias);
        Assert.Equal(240m, d101.MontoPagar);

        foreach (var num in new[] { 101, 201, 301 })
            Assert.Equal(EstadoHabitacion.Reservado, (await v.Habitaciones.FindAsync(num))!.Estado);
        Assert.Equal(EstadoHabitacion.Disponible, (await v.Habitaciones.FindAsync(102))!.Estado);
    }

    [Fact]
    public async Task Grabar_HabitacionNoDisponible_LanzaYRevierteTodo()
    {
        await using (var ctx = Ctx())
        {
            // 103 está Ocupada en los datos de ejemplo.
            var ex = await Assert.ThrowsAsync<ReservaException>(() =>
                Servicio(ctx).GrabarAsync(Cmd(1, (101, "A"), (103, "B"))));
            Assert.Contains("103", ex.Message);
        }
        await using var v = Ctx();
        Assert.Equal(0, await v.Reservas.CountAsync());
        Assert.Equal(EstadoHabitacion.Disponible, (await v.Habitaciones.FindAsync(101))!.Estado);
    }

    [Fact]
    public async Task Grabar_MismaHabitacionDosVeces_SegundaReservaFalla()
    {
        await using var ctx = Ctx();
        var s = Servicio(ctx);
        await s.GrabarAsync(Cmd(1, (101, "A")));
        await Assert.ThrowsAsync<ReservaException>(() => s.GrabarAsync(Cmd(2, (101, "B"))));
        Assert.Equal(1, await Ctx().Reservas.CountAsync());
    }

    [Fact]
    public async Task Grabar_ClienteInexistente_DiceClienteNoExiste()
    {
        await using var ctx = Ctx();
        var ex = await Assert.ThrowsAsync<ReservaException>(() => Servicio(ctx).GrabarAsync(Cmd(999, (101, "A"))));
        Assert.Equal("Cliente no existe", ex.Message);
    }

    [Fact]
    public async Task Grabar_DatosInvalidos_Lanza()
    {
        await using var ctx = Ctx();
        var s = Servicio(ctx);
        await Assert.ThrowsAsync<ReservaException>(() => s.GrabarAsync(Cmd(1)));                          // sin habitaciones
        await Assert.ThrowsAsync<ReservaException>(() => s.GrabarAsync(Cmd(1, (101, "  "))));             // sin huésped
        await Assert.ThrowsAsync<ReservaException>(() => s.GrabarAsync(Cmd(1, (101, "A"), (101, "B")))); // repetida
        await Assert.ThrowsAsync<ReservaException>(() => s.GrabarAsync(                                   // fechas inválidas
            new GrabarReservaCommand(1, new DateOnly(2026, 12, 4), new DateOnly(2026, 12, 4),
                new[] { new LineaReservaCommand(101, "A") })));
        Assert.Equal(0, await Ctx().Reservas.CountAsync());
    }

    [Fact]
    public async Task Grabar_Concurrente_NoDuplicaNumerosDeReserva()
    {
        async Task<int> Grabar(int cliente, int hab)
        {
            await using var ctx = Ctx();
            return await Servicio(ctx).GrabarAsync(Cmd(cliente, (hab, "Huésped " + hab)));
        }

        var numeros = await Task.WhenAll(Grabar(1, 101), Grabar(2, 102), Grabar(3, 201), Grabar(4, 202));
        Assert.Equal(new[] { 1, 2, 3, 4 }, numeros.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Habitaciones_Disponibles_ExcluyeReservadasOcupadasYLasIndicadas()
    {
        await using var ctx = Ctx();
        var s = new HabitacionService(ctx);
        var todas = await s.ObtenerDisponiblesAsync();
        Assert.DoesNotContain(todas, h => h.Numero is 103 or 203 or 402);
        Assert.Equal(7, todas.Count);
        var sin101 = await s.ObtenerDisponiblesAsync(new[] { 101 });
        Assert.Equal(6, sin101.Count);
        Assert.DoesNotContain(sin101, h => h.Numero == 101);
    }

    [Fact]
    public async Task Clientes_RegistrarYBuscar()
    {
        await using var ctx = Ctx();
        var s = new ClienteService(ctx);
        var nuevo = await s.RegistrarAsync("  Rosa Elena ", " Paredes Cruz ");
        Assert.Equal(6, nuevo.Codigo);
        Assert.Equal("Rosa Elena Paredes Cruz", (await s.ObtenerAsync(6))!.NombreCompleto);
        Assert.Single(await s.BuscarAsync("paredes"));
        Assert.Single(await s.BuscarAsync("6"));
        Assert.Null(await s.ObtenerAsync(777));
        await Assert.ThrowsAsync<ReservaException>(() => s.RegistrarAsync("", "X"));
    }
}
