using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReservaHotel.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CostoPorDia = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Numero);
                    table.CheckConstraint("CK_Habitaciones_Costo", "[CostoPorDia] > 0");
                    table.CheckConstraint("CK_Habitaciones_Estado", "[Estado] IN (N'Disponible', N'Reservado', N'Ocupado')");
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    NumeroReserva = table.Column<int>(type: "int", nullable: false),
                    CodigoCliente = table.Column<int>(type: "int", nullable: false),
                    FechaLlegada = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaSalida = table.Column<DateOnly>(type: "date", nullable: false),
                    CantidadDias = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Igv = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.NumeroReserva);
                    table.CheckConstraint("CK_Reservas_Dias", "[CantidadDias] > 0");
                    table.CheckConstraint("CK_Reservas_Fechas", "[FechaSalida] > [FechaLlegada]");
                    table.ForeignKey(
                        name: "FK_Reservas_Clientes_CodigoCliente",
                        column: x => x.CodigoCliente,
                        principalTable: "Clientes",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesReserva",
                columns: table => new
                {
                    NumeroReserva = table.Column<int>(type: "int", nullable: false),
                    NumeroHabitacion = table.Column<int>(type: "int", nullable: false),
                    NombreHuesped = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CostoDia = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Dias = table.Column<int>(type: "int", nullable: false),
                    MontoPagar = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesReserva", x => new { x.NumeroReserva, x.NumeroHabitacion });
                    table.CheckConstraint("CK_DetallesReserva_Dias", "[Dias] > 0");
                    table.ForeignKey(
                        name: "FK_DetallesReserva_Habitaciones_NumeroHabitacion",
                        column: x => x.NumeroHabitacion,
                        principalTable: "Habitaciones",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesReserva_Reservas_NumeroReserva",
                        column: x => x.NumeroReserva,
                        principalTable: "Reservas",
                        principalColumn: "NumeroReserva",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Codigo", "Apellidos", "Nombres" },
                values: new object[,]
                {
                    { 1, "Ramírez Soto", "Carlos Alberto" },
                    { 2, "Quispe Huamán", "María Fernanda" },
                    { 3, "Torres Vargas", "Luis Miguel" },
                    { 4, "Flores Medina", "Ana Lucía" },
                    { 5, "Castillo Rojas", "José Antonio" }
                });

            migrationBuilder.InsertData(
                table: "Habitaciones",
                columns: new[] { "Numero", "CostoPorDia", "Estado", "Tipo" },
                values: new object[,]
                {
                    { 101, 80m, "Disponible", "Simple" },
                    { 102, 80m, "Disponible", "Simple" },
                    { 103, 80m, "Ocupado", "Simple" },
                    { 201, 120m, "Disponible", "Doble" },
                    { 202, 120m, "Disponible", "Doble" },
                    { 203, 120m, "Reservado", "Doble" },
                    { 301, 150m, "Disponible", "Matrimonial" },
                    { 302, 150m, "Disponible", "Matrimonial" },
                    { 401, 250m, "Disponible", "Suite" },
                    { 402, 250m, "Ocupado", "Suite" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesReserva_NumeroHabitacion",
                table: "DetallesReserva",
                column: "NumeroHabitacion");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CodigoCliente",
                table: "Reservas",
                column: "CodigoCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesReserva");

            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
