using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAtlas.Migrations
{
    /// <inheritdoc />
    public partial class AddAclaracionesToDetallePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aclaraciones",
                table: "DetallesPedido",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 1, 26, 21, 42, 0, 661, DateTimeKind.Local).AddTicks(5844));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aclaraciones",
                table: "DetallesPedido");

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 1, 26, 21, 36, 58, 476, DateTimeKind.Local).AddTicks(3021));
        }
    }
}
