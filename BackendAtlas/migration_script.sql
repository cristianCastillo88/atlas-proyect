CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226_AddActivoPropertyLegacy') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226_AddActivoPropertyLegacy', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `Categorias` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Activa` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Categorias` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `EstadosPedido` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_EstadosPedido` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `MetodosPago` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `EsActivo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_MetodosPago` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `Promociones` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `DescuentoPorcentaje` decimal(5,2) NOT NULL,
        `FechaInicio` datetime(6) NOT NULL,
        `FechaFin` datetime(6) NOT NULL,
        `Activa` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Promociones` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `TiposEntrega` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `PrecioBase` decimal(10,2) NOT NULL,
        CONSTRAINT `PK_TiposEntrega` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `Productos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descripcion` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Precio` decimal(10,2) NOT NULL,
        `UrlImagen` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Activo` tinyint(1) NOT NULL,
        `CategoriaId` int NOT NULL,
        CONSTRAINT `PK_Productos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Productos_Categorias_CategoriaId` FOREIGN KEY (`CategoriaId`) REFERENCES `Categorias` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `Pedidos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FechaCreacion` datetime(6) NOT NULL,
        `Total` decimal(10,2) NOT NULL,
        `EstadoPedidoId` int NOT NULL,
        `TipoEntregaId` int NOT NULL,
        `MetodoPagoId` int NOT NULL,
        `NombreCliente` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `TelefonoCliente` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `DireccionCliente` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Observaciones` varchar(500) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Pedidos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Pedidos_EstadosPedido_EstadoPedidoId` FOREIGN KEY (`EstadoPedidoId`) REFERENCES `EstadosPedido` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Pedidos_MetodosPago_MetodoPagoId` FOREIGN KEY (`MetodoPagoId`) REFERENCES `MetodosPago` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Pedidos_TiposEntrega_TipoEntregaId` FOREIGN KEY (`TipoEntregaId`) REFERENCES `TiposEntrega` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE TABLE `DetallesPedido` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PedidoId` int NOT NULL,
        `ProductoId` int NOT NULL,
        `Cantidad` int NOT NULL,
        `PrecioUnitario` decimal(10,2) NOT NULL,
        CONSTRAINT `PK_DetallesPedido` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DetallesPedido_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_DetallesPedido_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    INSERT INTO `EstadosPedido` (`Id`, `Descripcion`, `Nombre`)
    VALUES (1, 'Pedido recibido, esperando confirmación', 'Pendiente'),
    (2, 'Pedido en proceso de preparación', 'En Preparacion'),
    (3, 'Pedido listo para entrega o retiro', 'Listo'),
    (4, 'Pedido entregado al cliente', 'Entregado'),
    (5, 'Pedido cancelado', 'Cancelado');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    INSERT INTO `MetodosPago` (`Id`, `Descripcion`, `EsActivo`, `Nombre`)
    VALUES (1, 'Pago en efectivo al momento de la entrega', TRUE, 'Efectivo'),
    (2, 'Transferencia bancaria - CBU: 1234567890123456789012', TRUE, 'Transferencia'),
    (3, 'Pago mediante código QR', TRUE, 'QR');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    INSERT INTO `TiposEntrega` (`Id`, `Nombre`, `PrecioBase`)
    VALUES (1, 'Retiro', 0.0),
    (2, 'Delivery', 5.0);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE INDEX `IX_DetallesPedido_PedidoId` ON `DetallesPedido` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE INDEX `IX_DetallesPedido_ProductoId` ON `DetallesPedido` (`ProductoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE INDEX `IX_Pedidos_EstadoPedidoId` ON `Pedidos` (`EstadoPedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE INDEX `IX_Pedidos_MetodoPagoId` ON `Pedidos` (`MetodoPagoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE INDEX `IX_Pedidos_TipoEntregaId` ON `Pedidos` (`TipoEntregaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    CREATE INDEX `IX_Productos_CategoriaId` ON `Productos` (`CategoriaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226193541_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226193541_InitialCreate', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226194132_SecondCreate') THEN

    INSERT INTO `MetodosPago` (`Id`, `Descripcion`, `EsActivo`, `Nombre`)
    VALUES (4, 'Pago mediante tarjeta de débito o crédito', TRUE, 'Tarjeta Débito/Crédito');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226194132_SecondCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226194132_SecondCreate', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `DetallesPedido` DROP FOREIGN KEY `FK_DetallesPedido_Pedidos_PedidoId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `DetallesPedido` DROP FOREIGN KEY `FK_DetallesPedido_Productos_ProductoId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` DROP FOREIGN KEY `FK_Pedidos_EstadosPedido_EstadoPedidoId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` DROP FOREIGN KEY `FK_Pedidos_MetodosPago_MetodoPagoId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` DROP FOREIGN KEY `FK_Pedidos_TiposEntrega_TipoEntregaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Productos` DROP FOREIGN KEY `FK_Productos_Categorias_CategoriaId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `TiposEntrega` ADD `SucursalId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Productos` ADD `SucursalId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` ADD `SucursalId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `MetodosPago` ADD `SucursalId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Categorias` ADD `SucursalId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE TABLE `Negocios` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `UrlLogo` varchar(500) CHARACTER SET utf8mb4 NULL,
        `FechaRegistro` datetime(6) NOT NULL,
        CONSTRAINT `PK_Negocios` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE TABLE `Sucursales` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `NegocioId` int NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Direccion` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Telefono` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Slug` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Sucursales` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Sucursales_Negocios_NegocioId` FOREIGN KEY (`NegocioId`) REFERENCES `Negocios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE TABLE `Usuarios` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `NegocioId` int NOT NULL,
        CONSTRAINT `PK_Usuarios` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Usuarios_Negocios_NegocioId` FOREIGN KEY (`NegocioId`) REFERENCES `Negocios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    UPDATE `MetodosPago` SET `SucursalId` = 1
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    UPDATE `MetodosPago` SET `SucursalId` = 1
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    UPDATE `MetodosPago` SET `SucursalId` = 1
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    UPDATE `MetodosPago` SET `SucursalId` = 1
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    INSERT INTO `Negocios` (`Id`, `FechaRegistro`, `Nombre`, `UrlLogo`)
    VALUES (1, TIMESTAMP '2025-12-26 20:22:46', 'Pizzeria Don Pepe', NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    UPDATE `TiposEntrega` SET `SucursalId` = 1
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    UPDATE `TiposEntrega` SET `SucursalId` = 1
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    INSERT INTO `Sucursales` (`Id`, `Direccion`, `NegocioId`, `Nombre`, `Slug`, `Telefono`)
    VALUES (1, 'Calle Principal 123', 1, 'Centro', 'pizzeria-don-pepe-centro', '123456789');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_TiposEntrega_SucursalId` ON `TiposEntrega` (`SucursalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_Productos_SucursalId` ON `Productos` (`SucursalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_Pedidos_SucursalId` ON `Pedidos` (`SucursalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_MetodosPago_SucursalId` ON `MetodosPago` (`SucursalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_Categorias_SucursalId` ON `Categorias` (`SucursalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_Sucursales_NegocioId` ON `Sucursales` (`NegocioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    CREATE INDEX `IX_Usuarios_NegocioId` ON `Usuarios` (`NegocioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Categorias` ADD CONSTRAINT `FK_Categorias_Sucursales_SucursalId` FOREIGN KEY (`SucursalId`) REFERENCES `Sucursales` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `DetallesPedido` ADD CONSTRAINT `FK_DetallesPedido_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `DetallesPedido` ADD CONSTRAINT `FK_DetallesPedido_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `MetodosPago` ADD CONSTRAINT `FK_MetodosPago_Sucursales_SucursalId` FOREIGN KEY (`SucursalId`) REFERENCES `Sucursales` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` ADD CONSTRAINT `FK_Pedidos_EstadosPedido_EstadoPedidoId` FOREIGN KEY (`EstadoPedidoId`) REFERENCES `EstadosPedido` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` ADD CONSTRAINT `FK_Pedidos_MetodosPago_MetodoPagoId` FOREIGN KEY (`MetodoPagoId`) REFERENCES `MetodosPago` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` ADD CONSTRAINT `FK_Pedidos_Sucursales_SucursalId` FOREIGN KEY (`SucursalId`) REFERENCES `Sucursales` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Pedidos` ADD CONSTRAINT `FK_Pedidos_TiposEntrega_TipoEntregaId` FOREIGN KEY (`TipoEntregaId`) REFERENCES `TiposEntrega` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Productos` ADD CONSTRAINT `FK_Productos_Categorias_CategoriaId` FOREIGN KEY (`CategoriaId`) REFERENCES `Categorias` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `Productos` ADD CONSTRAINT `FK_Productos_Sucursales_SucursalId` FOREIGN KEY (`SucursalId`) REFERENCES `Sucursales` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    ALTER TABLE `TiposEntrega` ADD CONSTRAINT `FK_TiposEntrega_Sucursales_SucursalId` FOREIGN KEY (`SucursalId`) REFERENCES `Sucursales` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232246_MultiSucursalAuth') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226232246_MultiSucursalAuth', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    DELETE FROM `MetodosPago`
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    DELETE FROM `MetodosPago`
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    DELETE FROM `MetodosPago`
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    DELETE FROM `MetodosPago`
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-26 20:23:47'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    INSERT INTO `Usuarios` (`Id`, `Email`, `NegocioId`, `Nombre`, `PasswordHash`)
    VALUES (1, 'admin@pizzeriadonpepe.com', 1, 'Admin', '$2a$11$TNMi61YITP34tpj5/fBNg.FNeAa9YuL.3LpV89ac9DhmDlT6vbAkO');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232347_AddUsuarioSeeding') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226232347_AddUsuarioSeeding', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232459_TransformacionMultiSucursal') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-26 20:24:59'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232459_TransformacionMultiSucursal') THEN

    UPDATE `Usuarios` SET `PasswordHash` = '$2a$11$cAymBcCREHyp69GTkA9jD.qCJEeG0zK5XYJGMBY2WOl20AO6MUZw6'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226232459_TransformacionMultiSucursal') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226232459_TransformacionMultiSucursal', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226233247_AddRolesAndSeeding') THEN

    ALTER TABLE `Usuarios` MODIFY COLUMN `NegocioId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226233247_AddRolesAndSeeding') THEN

    ALTER TABLE `Usuarios` ADD `Rol` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226233247_AddRolesAndSeeding') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-26 20:32:46'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226233247_AddRolesAndSeeding') THEN

    UPDATE `Usuarios` SET `Email` = 'admin@sistema.com', `NegocioId` = NULL, `Nombre` = 'Super Admin', `PasswordHash` = '$2a$11$TNMi61YITP34tpj5/fBNg.FNeAa9YuL.3LpV89ac9DhmDlT6vbAkO', `Rol` = 'SuperAdmin'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251226233247_AddRolesAndSeeding') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251226233247_AddRolesAndSeeding', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227021301_AddActivoProperty') THEN


                    SET @cnt := (SELECT COUNT(*) FROM information_schema.COLUMNS 
                                 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Usuarios' AND COLUMN_NAME = 'Activo');
                    SET @sql := IF(@cnt = 0, 'ALTER TABLE Usuarios ADD COLUMN Activo TINYINT(1) NOT NULL DEFAULT 0', 'SELECT 1');
                    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227021301_AddActivoProperty') THEN


                    SET @cnt := (SELECT COUNT(*) FROM information_schema.COLUMNS 
                                 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Negocios' AND COLUMN_NAME = 'Activo');
                    SET @sql := IF(@cnt = 0, 'ALTER TABLE Negocios ADD COLUMN Activo TINYINT(1) NOT NULL DEFAULT 0', 'SELECT 1');
                    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227021301_AddActivoProperty') THEN

    UPDATE `Negocios` SET `Activo` = TRUE, `FechaRegistro` = TIMESTAMP '2025-12-26 23:13:01'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227021301_AddActivoProperty') THEN

    UPDATE `Usuarios` SET `Activo` = TRUE
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227021301_AddActivoProperty') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251227021301_AddActivoProperty', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227160527_AddSucursalActivo') THEN

    ALTER TABLE `Sucursales` ADD `Activo` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227160527_AddSucursalActivo') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-27 13:05:27'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227160527_AddSucursalActivo') THEN

    UPDATE `Sucursales` SET `Activo` = TRUE
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227160527_AddSucursalActivo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251227160527_AddSucursalActivo', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227163318_AddUsuarioSucursal') THEN

    ALTER TABLE `Usuarios` ADD `SucursalId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227163318_AddUsuarioSucursal') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-27 13:33:18'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227163318_AddUsuarioSucursal') THEN

    UPDATE `Usuarios` SET `SucursalId` = NULL
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227163318_AddUsuarioSucursal') THEN

    CREATE INDEX `IX_Usuarios_SucursalId` ON `Usuarios` (`SucursalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227163318_AddUsuarioSucursal') THEN

    ALTER TABLE `Usuarios` ADD CONSTRAINT `FK_Usuarios_Sucursales_SucursalId` FOREIGN KEY (`SucursalId`) REFERENCES `Sucursales` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227163318_AddUsuarioSucursal') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251227163318_AddUsuarioSucursal', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227202906_AddMetodoPagoSeeding') THEN

    INSERT INTO `MetodosPago` (`Id`, `Descripcion`, `EsActivo`, `Nombre`, `SucursalId`)
    VALUES (1, 'Pago en efectivo', TRUE, 'Efectivo', 1),
    (2, 'Pago con tarjeta de crédito', TRUE, 'Tarjeta de Crédito', 1),
    (3, 'Pago con tarjeta de débito', TRUE, 'Tarjeta de Débito', 1),
    (4, 'Pago por transferencia bancaria', TRUE, 'Transferencia', 1);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227202906_AddMetodoPagoSeeding') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-27 17:29:05'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251227202906_AddMetodoPagoSeeding') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251227202906_AddMetodoPagoSeeding', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228192403_AddNegocioSlug') THEN

    ALTER TABLE `Negocios` ADD `Slug` varchar(100) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'temp-slug';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228192403_AddNegocioSlug') THEN


                    UPDATE Negocios 
                    SET Slug = CONCAT('negocio-', Id)
                    WHERE Slug = 'temp-slug' OR Slug = '';
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228192403_AddNegocioSlug') THEN

    UPDATE `Negocios` SET `FechaRegistro` = TIMESTAMP '2025-12-28 16:24:02', `Slug` = 'pizzeria-don-pepe'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228192403_AddNegocioSlug') THEN

    CREATE UNIQUE INDEX `IX_Negocios_Slug` ON `Negocios` (`Slug`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228192403_AddNegocioSlug') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251228192403_AddNegocioSlug', '8.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

