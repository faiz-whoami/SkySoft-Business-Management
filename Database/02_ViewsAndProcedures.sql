-- ============================================================
-- SkySoft Business Management
-- 02_ViewsAndProcedures.sql
-- Reporting views and stored procedures.
-- ============================================================

USE [SkySoftDB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_PartyDirectory AS
SELECT 'Customer' AS PartyType, CustomerId AS PartyId, Name, Email, Phone, Address, IsActive FROM dbo.Customer
UNION ALL
SELECT 'Supplier', SupplierId, Name, Email, Phone, Address, IsActive FROM dbo.Supplier
UNION ALL
SELECT 'Vendor', VendorId, Name, Email, Phone, Address, IsActive FROM dbo.Vendor;
GO

CREATE OR ALTER VIEW dbo.vw_MonthlyTransactionSummary AS
SELECT
    YEAR(t.TransactionDate)  AS TxnYear,
    MONTH(t.TransactionDate) AS TxnMonth,
    tt.Name                  AS TransactionType,
    COUNT(*)                 AS TransactionCount,
    SUM(t.TotalAmount)       AS TotalAmount
FROM dbo.[Transaction] t
JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
GROUP BY YEAR(t.TransactionDate), MONTH(t.TransactionDate), tt.Name;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTransactionWithItems
    @TransactionTypeId  INT,
    @CustomerId          INT = NULL,
    @SupplierId          INT = NULL,
    @VendorId            INT = NULL,
    @CreatedByUserId     INT,
    @ItemsJson           NVARCHAR(MAX),
    @TransactionDate     DATETIME2 = NULL,
    @NewTransactionId    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PendingStatusId INT =
            (SELECT TransactionStatusId FROM dbo.TransactionStatus WHERE Name = N'Pending');

        IF @PendingStatusId IS NULL
            THROW 51001, 'Pending transaction status is missing.', 1;

        DECLARE @Items TABLE (
            ProductId   INT NULL,
            Description NVARCHAR(200),
            Quantity    INT,
            UnitPrice   DECIMAL(10,2)
        );

        INSERT INTO @Items (ProductId, Description, Quantity, UnitPrice)
        SELECT ProductId, Description, Quantity, UnitPrice
        FROM OPENJSON(@ItemsJson)
        WITH (
            ProductId   INT           '$.ProductId',
            Description NVARCHAR(200) '$.Description',
            Quantity    INT           '$.Quantity',
            UnitPrice   DECIMAL(10,2) '$.UnitPrice'
        );

        IF NOT EXISTS (SELECT 1 FROM @Items)
            THROW 51000, 'A transaction must contain at least one line item.', 1;

        DECLARE @Total DECIMAL(12,2);
        SELECT @Total = SUM(Quantity * UnitPrice) FROM @Items;

        INSERT INTO dbo.[Transaction]
            (TransactionTypeId, TransactionStatusId, CustomerId, SupplierId, VendorId, CreatedByUserId, TransactionDate, TotalAmount)
        VALUES
            (@TransactionTypeId, @PendingStatusId, @CustomerId, @SupplierId, @VendorId, @CreatedByUserId, ISNULL(@TransactionDate, SYSDATETIME()), @Total);

        SET @NewTransactionId = SCOPE_IDENTITY();

        INSERT INTO dbo.TransactionItem (TransactionId, ProductId, Description, Quantity, UnitPrice)
        SELECT @NewTransactionId, ProductId, Description, Quantity, UnitPrice FROM @Items;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Report_PartyDirectory
    @PartyType NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PartyType, PartyId, Name, Email, Phone, Address
    FROM dbo.vw_PartyDirectory
    WHERE (@PartyType IS NULL OR PartyType = @PartyType)
      AND IsActive = 1
    ORDER BY PartyType, Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Report_TransactionDetail
    @TransactionId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT t.TransactionId,
           tt.Name AS TransactionType,
           ts.Name AS TransactionStatus,
           t.TransactionDate,
           COALESCE(c.Name, s.Name, v.Name) AS PartyName,
           t.TotalAmount,
           u.FullName AS RecordedBy
    FROM dbo.[Transaction] t
    JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
    JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
    JOIN dbo.EndUser u ON u.EndUserId = t.CreatedByUserId
    LEFT JOIN dbo.Customer c ON c.CustomerId = t.CustomerId
    LEFT JOIN dbo.Supplier s ON s.SupplierId = t.SupplierId
    LEFT JOIN dbo.Vendor v ON v.VendorId = t.VendorId
    WHERE t.TransactionId = @TransactionId;

    SELECT Description, Quantity, UnitPrice, LineTotal
    FROM dbo.TransactionItem
    WHERE TransactionId = @TransactionId
    ORDER BY TransactionItemId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Report_MonthlySummary
    @Year INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TxnYear, TxnMonth, TransactionType, TransactionCount, TotalAmount
    FROM dbo.vw_MonthlyTransactionSummary
    WHERE (@Year IS NULL OR TxnYear = @Year)
    ORDER BY TxnYear, TxnMonth, TransactionType;
END;
GO
