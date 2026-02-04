using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAtlas.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialMediaToSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlFacebook",
                table: "Sucursales",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UrlInstagram",
                table: "Sucursales",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 1, 26, 21, 36, 58, 476, DateTimeKind.Local).AddTicks(3021));

            migrationBuilder.UpdateData(
                table: "Sucursales",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "UrlFacebook", "UrlInstagram" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlFacebook",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "UrlInstagram",
                table: "Sucursales");

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 29, 15, 48, 7, 194, DateTimeKind.Local).AddTicks(361));
        }
    }
}
