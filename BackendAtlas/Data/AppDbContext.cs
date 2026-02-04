using Microsoft.EntityFrameworkCore;
using BackendAtlas.Domain;
using BCrypt.Net;

namespace BackendAtlas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<EstadoPedido> EstadosPedido { get; set; }
        public DbSet<TipoEntrega> TiposEntrega { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Negocio> Negocios { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de EstadoPedido
            modelBuilder.Entity<EstadoPedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(200);
            });

            // Configuración de TipoEntrega (global, sin sucursal)
            modelBuilder.Entity<TipoEntrega>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PrecioBase).HasColumnType("decimal(10,2)");
            });

            // Configuración de MetodoPago (global, sin sucursal)
            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(200);
            });

            // Configuración de Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Sucursal)
                      .WithMany(s => s.Categorias)
                      .HasForeignKey(e => e.SucursalId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Precio).HasColumnType("decimal(10,2)");
                entity.Property(e => e.UrlImagen).HasMaxLength(500);

                entity.HasOne(e => e.Categoria)
                      .WithMany(c => c.Productos)
                      .HasForeignKey(e => e.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Sucursal)
                      .WithMany(s => s.Productos)
                      .HasForeignKey(e => e.SucursalId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Promocion
            modelBuilder.Entity<Promocion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DescuentoPorcentaje).HasColumnType("decimal(5,2)");
            });

            // Configuración de Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasColumnType("decimal(10,2)");
                entity.Property(e => e.NombreCliente).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TelefonoCliente).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DireccionCliente).HasMaxLength(200);
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                entity.HasOne(e => e.EstadoPedido)
                      .WithMany(ep => ep.Pedidos)
                      .HasForeignKey(e => e.EstadoPedidoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipoEntrega)
                      .WithMany(te => te.Pedidos)
                      .HasForeignKey(e => e.TipoEntregaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MetodoPago)
                      .WithMany(mp => mp.Pedidos)
                      .HasForeignKey(e => e.MetodoPagoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Sucursal)
                      .WithMany(s => s.Pedidos)
                      .HasForeignKey(e => e.SucursalId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de DetallePedido
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.Pedido)
                      .WithMany(p => p.DetallesPedido)
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Producto)
                      .WithMany(pr => pr.DetallesPedido)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Negocio
            modelBuilder.Entity<Negocio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Slug).HasMaxLength(100);
                entity.Property(e => e.UrlLogo).HasMaxLength(500);
            });

            // Configuración de Sucursal
            modelBuilder.Entity<Sucursal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Direccion).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PrecioDelivery).HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.Negocio)
                      .WithMany(n => n.Sucursales)
                      .HasForeignKey(e => e.NegocioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Rol).HasConversion<string>();

                entity.HasOne(e => e.Negocio)
                      .WithMany(n => n.Usuarios)
                      .HasForeignKey(e => e.NegocioId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false); // Nullable

                entity.HasOne(e => e.Sucursal)
                    .WithMany()
                    .HasForeignKey(e => e.SucursalId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // ============ HIGH PRIORITY #2: Performance Indexes ============
            // Critical indexes for frequently queried columns

            // Negocio indexes
            modelBuilder.Entity<Negocio>()
                .HasIndex(n => n.Slug)
                .HasDatabaseName("IX_Negocios_Slug")
                .IsUnique();

            // Sucursal indexes
            modelBuilder.Entity<Sucursal>()
                .HasIndex(s => s.Slug)
                .HasDatabaseName("IX_Sucursales_Slug")
                .IsUnique();

            modelBuilder.Entity<Sucursal>()
                .HasIndex(s => s.NegocioId)
                .HasDatabaseName("IX_Sucursales_NegocioId");

            modelBuilder.Entity<Sucursal>()
                .HasIndex(s => new { s.NegocioId, s.Activo })
                .HasDatabaseName("IX_Sucursales_NegocioId_Activo");

            // Producto indexes - critical for catalog queries
            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.SucursalId)
                .HasDatabaseName("IX_Productos_SucursalId");

            modelBuilder.Entity<Producto>()
                .HasIndex(p => new { p.SucursalId, p.Activo })
                .HasDatabaseName("IX_Productos_SucursalId_Activo");

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.CategoriaId)
                .HasDatabaseName("IX_Productos_CategoriaId");

            // Categoria indexes
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.SucursalId)
                .HasDatabaseName("IX_Categorias_SucursalId");

            modelBuilder.Entity<Categoria>()
                .HasIndex(c => new { c.SucursalId, c.Activa })
                .HasDatabaseName("IX_Categorias_SucursalId_Activa");

            // Pedido indexes - critical for orders listing and filtering
            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.SucursalId)
                .HasDatabaseName("IX_Pedidos_SucursalId");

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.EstadoPedidoId)
                .HasDatabaseName("IX_Pedidos_EstadoPedidoId");

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => new { p.SucursalId, p.EstadoPedidoId, p.FechaCreacion })
                .HasDatabaseName("IX_Pedidos_SucursalId_EstadoPedidoId_FechaCreacion");

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.FechaCreacion)
                .HasDatabaseName("IX_Pedidos_FechaCreacion");

            // DetallePedido indexes
            modelBuilder.Entity<DetallePedido>()
                .HasIndex(d => d.PedidoId)
                .HasDatabaseName("IX_DetallesPedido_PedidoId");

            modelBuilder.Entity<DetallePedido>()
                .HasIndex(d => d.ProductoId)
                .HasDatabaseName("IX_DetallesPedido_ProductoId");

            // Usuario indexes - for authentication and authorization
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .HasDatabaseName("IX_Usuarios_Email")
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NegocioId)
                .HasDatabaseName("IX_Usuarios_NegocioId");

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.SucursalId)
                .HasDatabaseName("IX_Usuarios_SucursalId");

            // PasswordResetToken configuration - SECURITY CRITICAL
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(t => t.Id);
                
                // Token debe ser único para evitar colisiones
                entity.Property(t => t.Token)
                    .IsRequired()
                    .HasMaxLength(36); // GUID string
                
                entity.HasIndex(t => t.Token)
                    .HasDatabaseName("IX_PasswordResetTokens_Token")
                    .IsUnique(); // CRITICAL: índice único para lookups rápidos
                
                // Index para limpiar tokens expirados periódicamente
                entity.HasIndex(t => t.FechaExpiracion)
                    .HasDatabaseName("IX_PasswordResetTokens_FechaExpiracion");
                
                // Index para buscar tokens activos por usuario
                entity.HasIndex(t => new { t.UsuarioId, t.Usado })
                    .HasDatabaseName("IX_PasswordResetTokens_Usuario_Usado");
                
                // Relación con Usuario
                entity.HasOne(t => t.Usuario)
                    .WithMany() // Un usuario puede tener múltiples tokens (historial)
                    .HasForeignKey(t => t.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se elimina usuario, eliminar tokens
            });

            // Data Seeding
            modelBuilder.Entity<Negocio>().HasData(
                new Negocio { Id = 1, Nombre = "Pizzeria Don Pepe", Slug = "pizzeria-don-pepe", UrlLogo = null, FechaRegistro = DateTime.Now }
            );

            modelBuilder.Entity<Sucursal>().HasData(
                new Sucursal { Id = 1, NegocioId = 1, Nombre = "Centro", Direccion = "Calle Principal 123", Telefono = "123456789", Slug = "pizzeria-don-pepe-centro" }
            );

            modelBuilder.Entity<EstadoPedido>().HasData(
                new EstadoPedido { Id = 1, Nombre = "Pendiente", Descripcion = "Pedido recibido, esperando confirmación" },
                new EstadoPedido { Id = 2, Nombre = "En Preparacion", Descripcion = "Pedido en proceso de preparación" },
                new EstadoPedido { Id = 3, Nombre = "Listo", Descripcion = "Pedido listo para entrega o retiro" },
                new EstadoPedido { Id = 4, Nombre = "Entregado", Descripcion = "Pedido entregado al cliente" },
                new EstadoPedido { Id = 5, Nombre = "Cancelado", Descripcion = "Pedido cancelado" }
            );

            // Seeding de TiposEntrega (globales para toda la plataforma)
            modelBuilder.Entity<TipoEntrega>().HasData(
                new TipoEntrega { Id = 1, Nombre = "Retiro en Local", PrecioBase = 0.00m },
                new TipoEntrega { Id = 2, Nombre = "Delivery", PrecioBase = 5.00m }
            );

            // Seeding de MetodosPago (globales para toda la plataforma)
            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { Id = 1, Nombre = "Efectivo", Descripcion = "Pago en efectivo", EsActivo = true },
                new MetodoPago { Id = 2, Nombre = "Tarjeta de Crédito", Descripcion = "Pago con tarjeta de crédito", EsActivo = true },
                new MetodoPago { Id = 3, Nombre = "Tarjeta de Débito", Descripcion = "Pago con tarjeta de débito", EsActivo = true },
                new MetodoPago { Id = 4, Nombre = "Transferencia", Descripcion = "Pago por transferencia bancaria", EsActivo = true }
            );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, Email = "admin@sistema.com", PasswordHash = "$2a$11$TNMi61YITP34tpj5/fBNg.FNeAa9YuL.3LpV89ac9DhmDlT6vbAkO", Nombre = "Super Admin", NegocioId = null, Rol = RolUsuario.SuperAdmin }
            );
        }
    }
}
