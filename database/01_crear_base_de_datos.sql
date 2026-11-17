/* ============================================================================
   Sistema de Reservas de Hotel - Caso de uso: Generar reserva de habitación
   Script de creación de la base de datos (SQL Server 2019 o superior)
   Generado a partir de la migración de EF Core "Inicial" (idempotente: se puede
   ejecutar más de una vez). Incluye datos de ejemplo (clientes y habitaciones).
   Ejecutar con SSMS o:  sqlcmd -S localhost -E -C -i 01_crear_base_de_datos.sql
   ============================================================================ */
IF DB_ID(N'ReservaHotelDB') IS NULL
    CREATE DATABASE [ReservaHotelDB];
GO
USE [ReservaHotelDB];
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Codigo] int NOT NULL IDENTITY,
        [Nombres] nvarchar(80) NOT NULL,
        [Apellidos] nvarchar(80) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Codigo])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    CREATE TABLE [Habitaciones] (
        [Numero] int NOT NULL,
        [Tipo] nvarchar(30) NOT NULL,
        [CostoPorDia] decimal(10,2) NOT NULL,
        [Estado] nvarchar(15) NOT NULL,
        CONSTRAINT [PK_Habitaciones] PRIMARY KEY ([Numero]),
        CONSTRAINT [CK_Habitaciones_Costo] CHECK ([CostoPorDia] > 0),
        CONSTRAINT [CK_Habitaciones_Estado] CHECK ([Estado] IN (N'Disponible', N'Reservado', N'Ocupado'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    CREATE TABLE [Reservas] (
        [NumeroReserva] int NOT NULL,
        [CodigoCliente] int NOT NULL,
        [FechaLlegada] date NOT NULL,
        [FechaSalida] date NOT NULL,
        [CantidadDias] int NOT NULL,
        [Subtotal] decimal(12,2) NOT NULL,
        [Igv] decimal(12,2) NOT NULL,
        [MontoTotal] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_Reservas] PRIMARY KEY ([NumeroReserva]),
        CONSTRAINT [CK_Reservas_Dias] CHECK ([CantidadDias] > 0),
        CONSTRAINT [CK_Reservas_Fechas] CHECK ([FechaSalida] > [FechaLlegada]),
        CONSTRAINT [FK_Reservas_Clientes_CodigoCliente] FOREIGN KEY ([CodigoCliente]) REFERENCES [Clientes] ([Codigo]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    CREATE TABLE [DetallesReserva] (
        [NumeroReserva] int NOT NULL,
        [NumeroHabitacion] int NOT NULL,
        [NombreHuesped] nvarchar(120) NOT NULL,
        [CostoDia] decimal(10,2) NOT NULL,
        [Dias] int NOT NULL,
        [MontoPagar] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_DetallesReserva] PRIMARY KEY ([NumeroReserva], [NumeroHabitacion]),
        CONSTRAINT [CK_DetallesReserva_Dias] CHECK ([Dias] > 0),
        CONSTRAINT [FK_DetallesReserva_Habitaciones_NumeroHabitacion] FOREIGN KEY ([NumeroHabitacion]) REFERENCES [Habitaciones] ([Numero]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetallesReserva_Reservas_NumeroReserva] FOREIGN KEY ([NumeroReserva]) REFERENCES [Reservas] ([NumeroReserva]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Codigo', N'Apellidos', N'Nombres') AND [object_id] = OBJECT_ID(N'[Clientes]'))
        SET IDENTITY_INSERT [Clientes] ON;
    EXEC(N'INSERT INTO [Clientes] ([Codigo], [Apellidos], [Nombres])
    VALUES (1, N''Ramírez Soto'', N''Carlos Alberto''),
    (2, N''Quispe Huamán'', N''María Fernanda''),
    (3, N''Torres Vargas'', N''Luis Miguel''),
    (4, N''Flores Medina'', N''Ana Lucía''),
    (5, N''Castillo Rojas'', N''José Antonio'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Codigo', N'Apellidos', N'Nombres') AND [object_id] = OBJECT_ID(N'[Clientes]'))
        SET IDENTITY_INSERT [Clientes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Numero', N'CostoPorDia', N'Estado', N'Tipo') AND [object_id] = OBJECT_ID(N'[Habitaciones]'))
        SET IDENTITY_INSERT [Habitaciones] ON;
    EXEC(N'INSERT INTO [Habitaciones] ([Numero], [CostoPorDia], [Estado], [Tipo])
    VALUES (101, 80.0, N''Disponible'', N''Simple''),
    (102, 80.0, N''Disponible'', N''Simple''),
    (103, 80.0, N''Ocupado'', N''Simple''),
    (201, 120.0, N''Disponible'', N''Doble''),
    (202, 120.0, N''Disponible'', N''Doble''),
    (203, 120.0, N''Reservado'', N''Doble''),
    (301, 150.0, N''Disponible'', N''Matrimonial''),
    (302, 150.0, N''Disponible'', N''Matrimonial''),
    (401, 250.0, N''Disponible'', N''Suite''),
    (402, 250.0, N''Ocupado'', N''Suite'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Numero', N'CostoPorDia', N'Estado', N'Tipo') AND [object_id] = OBJECT_ID(N'[Habitaciones]'))
        SET IDENTITY_INSERT [Habitaciones] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetallesReserva_NumeroHabitacion] ON [DetallesReserva] ([NumeroHabitacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    CREATE INDEX [IX_Reservas_CodigoCliente] ON [Reservas] ([CodigoCliente]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117010216_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117010216_Inicial', N'10.0.15');
END;

COMMIT;
GO

