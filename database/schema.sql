-- =============================================================
-- LogisticsDB – Schema
-- Run this script in SSMS against your SQL Server instance
-- =============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'LogisticsDB')
BEGIN
    CREATE DATABASE LogisticsDB;
END
GO

USE LogisticsDB;
GO

-- ---------------------------------------------------------------
-- Table: Transports
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transports')
BEGIN
    CREATE TABLE Transports (
        Id               INT             PRIMARY KEY IDENTITY(1,1),
        Carrier          NVARCHAR(100)   NOT NULL,
        RouteFrom        NVARCHAR(100)   NOT NULL,
        RouteTo          NVARCHAR(100)   NOT NULL,
        DepartureTime    DATETIME        NOT NULL,
        ArrivalTime      DATETIME        NOT NULL,
        Delay            BIT             NOT NULL DEFAULT 0,
        Cost             FLOAT           NOT NULL,
        CargoType        NVARCHAR(100)   NOT NULL DEFAULT 'General',
        WeightTons       FLOAT           NOT NULL DEFAULT 1.0,
        Season           NVARCHAR(20)    NOT NULL DEFAULT 'Summer',
        WeatherCondition NVARCHAR(50)    NOT NULL DEFAULT 'Clear'
    );
END
GO
