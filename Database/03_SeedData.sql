-- ============================================================
-- SkySoft Business Management
-- 03_SeedData.sql
-- Sample data for screens and Crystal Reports.
-- Passwords are PBKDF2 hashes (iterations.salt.hash):
--   sjenkins    / Admin@123
--   mrodriguez  / Sales@123
-- ============================================================

USE [SkySoftDB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

INSERT INTO dbo.Role (RoleName) VALUES (N'Admin'), (N'Sales Staff');

INSERT INTO dbo.TransactionType (Name) VALUES (N'Sale'), (N'Purchase'), (N'Service');

INSERT INTO dbo.TransactionStatus (Name) VALUES (N'Pending'), (N'Completed'), (N'Cancelled');

INSERT INTO dbo.EndUser (Username, PasswordHash, FullName, RoleId) VALUES
    (N'sjenkins',   N'10000.GLlea6kVq1V/9NfM6SWF5A==.6QfHrZAMyCB7dQ4aUYIWC97Epx1EhGQyhN5rUvuF4FY=', N'Sarah Jenkins',  1),
    (N'mrodriguez', N'10000./7KepWsPhNweNW7GMN5SuA==.2OlHMHpgXei70wTRjVbxNOO/3Psh9IK/jVJ4bdLSNn8=', N'Mark Rodriguez', 2);

INSERT INTO dbo.Customer (Name, Email, Phone, Address) VALUES
    (N'Acme Industrial Corp',    N'jdoe@acmeind.com',         N'(555) 019-2831', N'100 Industrial Pkwy, Chicago, IL'),
    (N'Midwest Manufacturing',   N'rvance@midwestmfg.com',    N'(555) 018-3311', N'42 Assembly Way, Detroit, MI'),
    (N'Lakeside Contractors',    N'billing@lakesidecon.com',  N'(555) 017-4422', N'8 Harbor Blvd, Milwaukee, WI'),
    (N'Prairie Engineering LLC', N'ap@prairieeng.com',        N'(555) 016-5533', N'250 Grain St, Omaha, NE'),
    (N'Northline Hardware',      N'orders@northlinehw.com',   N'(555) 015-6644', N'19 Retail Row, Minneapolis, MN'),
    (N'Summit Fabrication',      N'purchasing@summitfab.com', N'(555) 014-7755', N'77 Weld Ave, Indianapolis, IN');

INSERT INTO dbo.Supplier (Name, Email, Phone, Address) VALUES
    (N'Global Tooling Solutions', N'erostova@globaltools.com',   N'(555) 088-1122', N'12 Toolmakers Lane, Cleveland, OH'),
    (N'Titan Hydraulics Inc',     N'mvance@titanhydraulics.com', N'(555) 077-4433', N'90 Press Way, Pittsburgh, PA'),
    (N'Apex Steel Supply',        N'sales@apexsteel.com',        N'(555) 076-5544', N'3 Mill Rd, Gary, IN'),
    (N'Northern Bearings Co',     N'orders@northernbearings.com',N'(555) 075-6655', N'44 Raceway, Toledo, OH');

INSERT INTO dbo.Vendor (Name, Email, Phone, Address, ServiceType) VALUES
    (N'SwiftLine Freight Co.',    N'dispatch@swiftline.com',  N'(555) 066-7788', N'5 Depot Rd, Columbus, OH',   N'Freight & Delivery'),
    (N'Precision Field Services', N'ops@precisionfield.com',  N'(555) 055-3344', N'77 Service Ave, Toledo, OH', N'On-site Installation & Maintenance'),
    (N'Harbor Crane Hire',        N'hire@harborcrane.com',    N'(555) 044-2211', N'1 Dockside, Cleveland, OH',  N'Equipment Rental');

INSERT INTO dbo.Product (SKU, ProductName, UnitPrice) VALUES
    (N'SKU-1092', N'Pneumatic Industrial Drill X1', 150.00),
    (N'SKU-4011', N'Carbide Drill Bit Set (12-Pc)',   45.00),
    (N'SKU-8820', N'Hydraulic Press Station 20T',   2450.00),
    (N'SKU-3019', N'Heavy Duty Roller Bearing',       85.00),
    (N'SKU-2204', N'Angle Grinder 7-Inch',           129.00),
    (N'SKU-5510', N'Safety Harness Kit',             96.50),
    (N'SKU-7741', N'Welding Helmet Auto-Dark',      210.00),
    (N'SKU-1188', N'Impact Socket Set (21-Pc)',      74.00);

DECLARE @NewId INT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 1, @CustomerId = 1, @CreatedByUserId = 2,
    @TransactionDate = '2026-06-12',
    @ItemsJson = N'[{"ProductId":1,"Description":"Pneumatic Industrial Drill X1","Quantity":2,"UnitPrice":150.00},{"ProductId":2,"Description":"Carbide Drill Bit Set (12-Pc)","Quantity":5,"UnitPrice":45.00}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 1, @CustomerId = 2, @CreatedByUserId = 2,
    @TransactionDate = '2026-07-03',
    @ItemsJson = N'[{"ProductId":3,"Description":"Hydraulic Press Station 20T","Quantity":1,"UnitPrice":2450.00}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 1, @CustomerId = 3, @CreatedByUserId = 2,
    @TransactionDate = '2026-08-18',
    @ItemsJson = N'[{"ProductId":5,"Description":"Angle Grinder 7-Inch","Quantity":4,"UnitPrice":129.00},{"ProductId":6,"Description":"Safety Harness Kit","Quantity":6,"UnitPrice":96.50}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 1, @CustomerId = 5, @CreatedByUserId = 1,
    @TransactionDate = '2026-09-01',
    @ItemsJson = N'[{"ProductId":7,"Description":"Welding Helmet Auto-Dark","Quantity":3,"UnitPrice":210.00},{"ProductId":8,"Description":"Impact Socket Set (21-Pc)","Quantity":2,"UnitPrice":74.00}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 2, @SupplierId = 1, @CreatedByUserId = 1,
    @TransactionDate = '2026-06-20',
    @ItemsJson = N'[{"ProductId":2,"Description":"Carbide Drill Bit Set (12-Pc)","Quantity":20,"UnitPrice":35.00}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 2, @SupplierId = 3, @CreatedByUserId = 1,
    @TransactionDate = '2026-08-05',
    @ItemsJson = N'[{"ProductId":4,"Description":"Heavy Duty Roller Bearing","Quantity":40,"UnitPrice":62.00}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 3, @VendorId = 1, @CreatedByUserId = 2,
    @TransactionDate = '2026-07-22',
    @ItemsJson = N'[{"ProductId":null,"Description":"Freight delivery - Chicago route","Quantity":1,"UnitPrice":220.00}]',
    @NewTransactionId = @NewId OUTPUT;

EXEC dbo.sp_CreateTransactionWithItems
    @TransactionTypeId = 3, @VendorId = 2, @CreatedByUserId = 1,
    @TransactionDate = '2026-09-02',
    @ItemsJson = N'[{"ProductId":null,"Description":"On-site press installation","Quantity":1,"UnitPrice":875.00}]',
    @NewTransactionId = @NewId OUTPUT;
GO
