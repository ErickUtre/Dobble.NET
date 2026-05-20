-- ============================================================
-- Script de creacion de la base de datos del juego Dobble
-- Ejecutar en SQL Server Management Studio o sqlcmd
-- ============================================================

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DobbleBD')
BEGIN
    CREATE DATABASE DobbleBD;
END
GO

USE DobbleBD;
GO

-- ============================================================
-- Tabla Cuenta (credenciales de acceso)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cuenta')
BEGIN
    CREATE TABLE dbo.Cuenta (
        idCuenta      INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombreUsuario VARCHAR(15)   NOT NULL,
        correo        VARCHAR(60)   NOT NULL,
        contraseña    VARCHAR(255)  NOT NULL
    );
END
GO

-- ============================================================
-- Tabla Usuario (perfil del jugador)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuario')
BEGIN
    CREATE TABLE dbo.Usuario (
        idCuenta  INT              NOT NULL PRIMARY KEY,
        foto      VARBINARY(MAX)   NULL,
        puntaje   INT              NULL,
        estado    BIT              NULL,
        CONSTRAINT FK__Usuario__idCuent__3D5E1FD2
            FOREIGN KEY (idCuenta) REFERENCES dbo.Cuenta(idCuenta)
            ON DELETE CASCADE
    );
END
GO

-- ============================================================
-- Tabla Amistad (relaciones entre jugadores)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Amistad')
BEGIN
    CREATE TABLE dbo.Amistad (
        idAmistad         INT  IDENTITY(1,1) NOT NULL PRIMARY KEY,
        estadoSolicitud   BIT  NULL,
        UsuarioPrincipalId INT NOT NULL,
        UsuarioAmigoId    INT  NOT NULL,
        CONSTRAINT FK__Amistad__Usuario__412EB0B6
            FOREIGN KEY (UsuarioPrincipalId) REFERENCES dbo.Usuario(idCuenta)
            ON DELETE CASCADE,
        CONSTRAINT FK__Amistad__Usuario__4222D4EF
            FOREIGN KEY (UsuarioAmigoId) REFERENCES dbo.Usuario(idCuenta)
    );
END
GO

PRINT 'Base de datos DobbleBD creada correctamente.';
GO
