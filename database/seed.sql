-- =============================================================
-- LogisticsDB – Seed Data
-- Run after schema.sql
-- =============================================================

USE LogisticsDB;
GO

-- Clear existing data (for re-seeding)
DELETE FROM Transports;
GO

-- ---------------------------------------------------------------
-- Insert sample transport records
-- ---------------------------------------------------------------
INSERT INTO Transports (Carrier, RouteFrom, RouteTo, DepartureTime, ArrivalTime, Delay, Cost, CargoType, WeightTons, Season, WeatherCondition)
VALUES
-- DHL records
('DHL', 'Beograd', 'Novi Sad',   DATEADD(DAY, -1,  GETDATE()), DATEADD(DAY, -1,  DATEADD(HOUR,  2, GETDATE())), 0, 120,  'Electronics',  5.0,  'Summer', 'Clear'),
('DHL', 'Beograd', 'Niš',        DATEADD(DAY, -3,  GETDATE()), DATEADD(DAY, -3,  DATEADD(HOUR,  4, GETDATE())), 1, 200,  'Chemicals',    8.0,  'Summer', 'Rain'),
('DHL', 'Beograd', 'Subotica',   DATEADD(DAY, -5,  GETDATE()), DATEADD(DAY, -5,  DATEADD(HOUR,  3, GETDATE())), 1, 150,  'Food',         10.0, 'Summer', 'Clear'),
('DHL', 'Novi Sad','Niš',        DATEADD(DAY, -7,  GETDATE()), DATEADD(DAY, -7,  DATEADD(HOUR,  5, GETDATE())), 0, 180,  'General',      12.0, 'Summer', 'Clear'),
('DHL', 'Beograd', 'Pančevo',    DATEADD(DAY, -2,  GETDATE()), DATEADD(DAY, -2,  DATEADD(HOUR,  1, GETDATE())), 0, 50,   'Electronics',   3.0, 'Summer', 'Clear'),
-- FedEx records
('FedEx','Beograd','Novi Sad',   DATEADD(DAY, -2,  GETDATE()), DATEADD(DAY, -2,  DATEADD(HOUR,  2, GETDATE())), 0, 130,  'Pharmaceuticals', 4.0, 'Summer', 'Clear'),
('FedEx','Beograd','Niš',        DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -10, DATEADD(HOUR,  3, GETDATE())), 0, 200,  'General',      7.0,  'Spring', 'Clear'),
('FedEx','Niš',    'Leskovac',   DATEADD(DAY, -4,  GETDATE()), DATEADD(DAY, -4,  DATEADD(HOUR,  1, GETDATE())), 1, 80,   'Food',         6.0,  'Summer', 'Rain'),
('FedEx','Subotica','Novi Sad',  DATEADD(DAY, -6,  GETDATE()), DATEADD(DAY, -6,  DATEADD(HOUR,  2, GETDATE())), 1, 100,  'Chemicals',    9.0,  'Summer', 'Fog'),
('FedEx','Beograd','Zrenjanin',  DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -15, DATEADD(HOUR,  2, GETDATE())), 0, 90,   'General',      5.0,  'Spring', 'Clear'),
-- DB Cargo records
('DB Cargo','Beograd','Novi Sad',DATEADD(DAY, -3,  GETDATE()), DATEADD(DAY, -3,  DATEADD(HOUR,  2, GETDATE())), 0, 110,  'Machinery',    15.0, 'Summer', 'Clear'),
('DB Cargo','Beograd','Niš',     DATEADD(DAY, -8,  GETDATE()), DATEADD(DAY, -8,  DATEADD(HOUR,  4, GETDATE())), 1, 210,  'Machinery',    20.0, 'Spring', 'Snow'),
('DB Cargo','Novi Sad','Subotica',DATEADD(DAY,-12, GETDATE()), DATEADD(DAY, -12, DATEADD(HOUR,  2, GETDATE())), 1, 95,   'General',      8.0,  'Winter', 'Snow'),
('DB Cargo','Beograd','Šabac',   DATEADD(DAY, -4,  GETDATE()), DATEADD(DAY, -4,  DATEADD(HOUR,  1, GETDATE())), 0, 65,   'Food',         7.0,  'Summer', 'Clear'),
('DB Cargo','Beograd','Pančevo', DATEADD(DAY, -1,  GETDATE()), DATEADD(DAY, -1,  DATEADD(HOUR,  1, GETDATE())), 0, 40,   'Electronics',   2.0, 'Summer', 'Clear'),
-- Older records (> 30 days)
('DHL', 'Beograd', 'Niš',        DATEADD(DAY, -35, GETDATE()), DATEADD(DAY, -35, DATEADD(HOUR, 3, GETDATE())),  1, 195,  'Chemicals',    9.0,  'Spring', 'Rain'),
('FedEx','Niš',    'Vranje',      DATEADD(DAY, -40, GETDATE()), DATEADD(DAY, -40, DATEADD(HOUR, 2, GETDATE())),  0, 90,   'General',      5.0,  'Spring', 'Clear'),
('DB Cargo','Beograd','Novi Sad', DATEADD(DAY, -45, GETDATE()), DATEADD(DAY, -45, DATEADD(HOUR, 2, GETDATE())),  1, 115,  'Machinery',    18.0, 'Winter', 'Snow');
GO

PRINT 'Seed data inserted successfully.';
GO
