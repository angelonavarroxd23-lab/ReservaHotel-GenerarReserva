# Reserva Hotel – Caso de uso «Generar Reserva de Habitación»

Prototipo web (ASP.NET Core MVC, .NET 10, EF Core 10, SQL Server) y documentación UML del caso de uso *Generar reserva de habitación* (actor: Recepcionista).

## Contenido
- `src/ReservaHotel.Web` – aplicación MVC (Controllers, Models, Services, Data, Views).
- `tests/ReservaHotel.Tests` – pruebas xUnit (cálculos y servicios contra SQL Server real).
- `database/01_crear_base_de_datos.sql` – script SQL Server idempotente (esquema + datos de ejemplo).
- `docs/Generar_Reserva_Habitacion_UML.docx` – diagramas de secuencia, comunicación y clases.
- `docs/diagramas/` – diagramas en PNG. `docs/capturas/` – capturas del prototipo.

## Requisitos
- .NET SDK 10
- SQL Server (Express/Developer/LocalDB u otra instancia)

## Configuración
Cadena de conexión en `src/ReservaHotel.Web/appsettings.json` (`ConnectionStrings:ReservaHotelDB`). Por defecto:
`Server=localhost;Database=ReservaHotelDB;Trusted_Connection=True;TrustServerCertificate=True;`
Para autenticación SQL use `User Id=...;Password=...;` en lugar de `Trusted_Connection`.

## Crear la base de datos (elegir una)
1. **Automática:** con `Database:AutoMigrate = true` (valor por defecto) la app aplica las migraciones EF Core al iniciar.
2. **Script SQL:** `sqlcmd -S localhost -E -i database/01_crear_base_de_datos.sql`
3. **Migraciones:** `dotnet ef database update --project src/ReservaHotel.Web`

## Ejecutar
```
dotnet run --project src/ReservaHotel.Web
```
Abrir la URL que muestra la consola (menú → «Generar Reserva»).

## Pruebas
```
dotnet test
```
Las pruebas de integración crean y borran una base temporal. Servidor distinto: variable de entorno `RESERVA_TEST_SERVER`.

## Funcionalidad (según el ECU)
Buscar/registrar cliente, fechas y cálculo de días, buscar habitaciones disponibles, huésped por habitación, agregar/eliminar habitaciones de la cuadrícula, subtotal/IGV/total, número de reserva automático, grabar reserva con detalle y pasar las habitaciones a *Reservado*. Mensajes del ECU: «Cliente no existe», «No hay habitaciones disponibles», «Reserva generada con el Nro. N».

## Decisiones técnicas (no vienen del ECU)
- IGV 18 % configurable (`Hotel:TasaIgv`); el ECU no define la tasa.
- Días = salida − llegada; monto por habitación = costo/día × días.
- Número de reserva = máximo + 1 dentro de una transacción Serializable con reintento por deadlock.
- Al grabar se revalida la disponibilidad y se recalculan los montos con los costos de la BD.
- Sin autenticación (el ECU supone Recepcionista ya logueada).
- Datos de ejemplo (5 clientes, 10 habitaciones) solo para pruebas.

## Limitaciones
Probado en SQL Server local con Windows Authentication; no se probó con carga ni en otros entornos.
