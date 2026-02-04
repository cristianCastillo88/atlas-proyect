using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAtlas.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioDeliveryToSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioDelivery",
                table: "Sucursales",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 1, 26, 23, 12, 39, 633, DateTimeKind.Local).AddTicks(8658));

            migrationBuilder.UpdateData(
                table: "Sucursales",
                keyColumn: "Id",
                keyValue: 1,
                column: "PrecioDelivery",
                value: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioDelivery",
                table: "Sucursales");

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 1, 26, 21, 42, 0, 661, DateTimeKind.Local).AddTicks(5844));
        }
    }
}
