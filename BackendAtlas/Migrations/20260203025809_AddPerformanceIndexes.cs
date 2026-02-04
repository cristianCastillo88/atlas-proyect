using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAtlas.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 2, 2, 23, 58, 9, 184, DateTimeKind.Local).AddTicks(8750));

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_NegocioId_Activo",
                table: "Sucursales",
                columns: new[] { "NegocioId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_Slug",
                table: "Sucursales",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SucursalId_Activo",
                table: "Productos",
                columns: new[] { "SucursalId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_FechaCreacion",
                table: "Pedidos",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_SucursalId_EstadoPedidoId_FechaCreacion",
                table: "Pedidos",
                columns: new[] { "SucursalId", "EstadoPedidoId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Negocios_Slug",
                table: "Negocios",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_SucursalId_Activa",
                table: "Categorias",
                columns: new[] { "SucursalId", "Activa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_NegocioId_Activo",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_Slug",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Productos_SucursalId_Activo",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_FechaCreacion",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_SucursalId_EstadoPedidoId_FechaCreacion",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Negocios_Slug",
                table: "Negocios");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_SucursalId_Activa",
                table: "Categorias");

            migrationBuilder.UpdateData(
                table: "Negocios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2026, 1, 26, 23, 12, 39, 633, DateTimeKind.Local).AddTicks(8658));
        }
    }
}
