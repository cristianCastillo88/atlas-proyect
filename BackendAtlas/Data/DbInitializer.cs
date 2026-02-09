using BackendAtlas.Domain;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BackendAtlas.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, IConfiguration configuration)
        {
            // Ejecutar migraciones automáticamente al iniciar
            context.Database.Migrate();

            // --- RUTINA DE LIMPIEZA DE DATOS DE PRUEBA ---
            // Borramos el negocio de ejemplo de la migración inicial si existe
            var demoNegocio = context.Negocios.FirstOrDefault(n => n.Nombre == "Pizzeria Don Pepe" || n.Slug == "pizzeria-don-pepe");
            if (demoNegocio != null)
            {
                context.Negocios.Remove(demoNegocio);
                context.SaveChanges();
            }
            // ---------------------------------------------

            // 1. Sembrar Estados de Pedido
            if (!context.EstadosPedido.Any())
            {
                context.EstadosPedido.AddRange(
                    new EstadoPedido { Nombre = "Pendiente", Descripcion = "Pedido recibido, esperando confirmación" },
                    new EstadoPedido { Nombre = "En Preparacion", Descripcion = "Pedido en proceso de preparación" },
                    new EstadoPedido { Nombre = "Listo", Descripcion = "Pedido listo para entrega o retiro" },
                    new EstadoPedido { Nombre = "Entregado", Descripcion = "Pedido entregado al cliente" },
                    new EstadoPedido { Nombre = "Cancelado", Descripcion = "Pedido cancelado" }
                );
            }

            // 2. Sembrar Tipos de Entrega
            if (!context.TiposEntrega.Any())
            {
                context.TiposEntrega.AddRange(
                    new TipoEntrega { Nombre = "Retiro en Local", PrecioBase = 0.00m },
                    new TipoEntrega { Nombre = "Delivery", PrecioBase = 5.00m }
                );
            }

            // 3. Sembrar Métodos de Pago
            if (!context.MetodosPago.Any())
            {
                context.MetodosPago.AddRange(
                    new MetodoPago { Nombre = "Efectivo", Descripcion = "Pago en efectivo", EsActivo = true },
                    new MetodoPago { Nombre = "Tarjeta de Crédito", Descripcion = "Pago con tarjeta de crédito", EsActivo = true },
                    new MetodoPago { Nombre = "Tarjeta de Débito", Descripcion = "Pago con tarjeta de débito", EsActivo = true },
                    new MetodoPago { Nombre = "Transferencia", Descripcion = "Pago por transferencia bancaria", EsActivo = true }
                );
            }

            // 4. Sembrar/Sincronizar SuperAdmin
            var adminEmail = configuration["SUPERADMIN_EMAIL"] ?? "admin@sistema.com";
            var adminPassword = configuration["SUPERADMIN_PASSWORD"] ?? "Admin123*";
            var adminNombre = configuration["SUPERADMIN_NOMBRE"] ?? "Super Admin";

            var existingAdmin = context.Usuarios.FirstOrDefault(u => u.Rol == RolUsuario.SuperAdmin);

            if (existingAdmin == null)
            {
                var superAdmin = new Usuario
                {
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Nombre = adminNombre,
                    Rol = RolUsuario.SuperAdmin,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                context.Usuarios.Add(superAdmin);
            }
            else
            {
                // Si ya existe pero el email es diferente al de la configuración, lo actualizamos
                // Esto permite corregir el correo si las variables de entorno cambiaron
                if (existingAdmin.Email != adminEmail)
                {
                    existingAdmin.Email = adminEmail;
                    existingAdmin.Nombre = adminNombre;
                    // También actualizamos el password por si acaso es el primer despliegue con variables reales
                    existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                }
            }

            context.SaveChanges();
        }
    }
}
