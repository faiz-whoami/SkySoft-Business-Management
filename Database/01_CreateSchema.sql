-- ============================================================
-- SkySoft Business Management
-- 01_CreateSchema.sql
-- Creates the SkySoftDB database, tables, constraints, and indexes.
-- Safe to re-run: it drops and recreates the database.
-- ============================================================

USE [master];
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'SkySoftDB')
BEGIN
    ALTER DATABASE [SkySoftDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [SkySoftDB];
END
GO

CREATE DATABASE [SkySoftDB];
GO

USE [SkySoftDB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
GO

-- ------------------------------------------------------------
-- Lookup tables
-- ------------------------------------------------------------
CREATE TABLE dbo.Role (
    RoleId       INT IDENTITY(1,1) NOT NULL,
    RoleName     NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Role PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Role_RoleName UNIQUE (RoleName)
);

CREATE TABLE dbo.TransactionType (
    TransactionTypeId INT IDENTITY(1,1) NOT NULL,
    Name               NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_TransactionType PRIMARY KEY (TransactionTypeId),
    CONSTRAINT UQ_TransactionType_Name UNIQUE (Name)
);

CREATE TABLE dbo.TransactionStatus (
    TransactionStatusId INT IDENTITY(1,1) NOT NULL,
    Name                 NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_TransactionStatus PRIMARY KEY (TransactionStatusId),
    CONSTRAINT UQ_TransactionStatus_Name UNIQUE (Name)
);

-- ------------------------------------------------------------
-- Security
-- ------------------------------------------------------------
CREATE TABLE dbo.EndUser (
    EndUserId     INT IDENTITY(1,1) NOT NULL,
    Username      NVARCHAR(50) NOT NULL,
    PasswordHash  NVARCHAR(256) NOT NULL,
    FullName      NVARCHAR(150) NOT NULL,
    RoleId        INT NOT NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_EndUser_IsActive DEFAULT (1),
    CreatedDate   DATETIME2 NOT NULL CONSTRAINT DF_EndUser_CreatedDate DEFAULT (SYSDATETIME()),
    ModifiedDate  DATETIME2 NOT NULL CONSTRAINT DF_EndUser_ModifiedDate DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_EndUser PRIMARY KEY (EndUserId),
    CONSTRAINT UQ_EndUser_Username UNIQUE (Username),
    CONSTRAINT FK_EndUser_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId)
);

-- ------------------------------------------------------------
-- Master data
-- ------------------------------------------------------------
CREATE TABLE dbo.Customer (
    CustomerId    INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NULL,
    Phone         NVARCHAR(30) NULL,
    Address       NVARCHAR(250) NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Customer_IsActive DEFAULT (1),
    CreatedDate   DATETIME2 NOT NULL CONSTRAINT DF_Customer_CreatedDate DEFAULT (SYSDATETIME()),
    ModifiedDate  DATETIME2 NOT NULL CONSTRAINT DF_Customer_ModifiedDate DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Customer PRIMARY KEY (CustomerId)
);

CREATE TABLE dbo.Supplier (
    SupplierId    INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NULL,
    Phone         NVARCHAR(30) NULL,
    Address       NVARCHAR(250) NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Supplier_IsActive DEFAULT (1),
    CreatedDate   DATETIME2 NOT NULL CONSTRAINT DF_Supplier_CreatedDate DEFAULT (SYSDATETIME()),
    ModifiedDate  DATETIME2 NOT NULL CONSTRAINT DF_Supplier_ModifiedDate DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Supplier PRIMARY KEY (SupplierId)
);

CREATE TABLE dbo.Vendor (
    VendorId      INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(150) NOT NULL,
    Email         NVARCHAR(150) NULL,
    Phone         NVARCHAR(30) NULL,
    Address       NVARCHAR(250) NULL,
    ServiceType   NVARCHAR(100) NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Vendor_IsActive DEFAULT (1),
    CreatedDate   DATETIME2 NOT NULL CONSTRAINT DF_Vendor_CreatedDate DEFAULT (SYSDATETIME()),
    ModifiedDate  DATETIME2 NOT NULL CONSTRAINT DF_Vendor_ModifiedDate DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Vendor PRIMARY KEY (VendorId)
);

CREATE TABLE dbo.Product (
    ProductId     INT IDENTITY(1,1) NOT NULL,
    SKU           NVARCHAR(50) NULL,
    ProductName   NVARCHAR(150) NOT NULL,
    UnitPrice     DECIMAL(10,2) NOT NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Product_IsActive DEFAULT (1),
    CreatedDate   DATETIME2 NOT NULL CONSTRAINT DF_Product_CreatedDate DEFAULT (SYSDATETIME()),
    ModifiedDate  DATETIME2 NOT NULL CONSTRAINT DF_Product_ModifiedDate DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Product PRIMARY KEY (ProductId),
    CONSTRAINT CK_Product_UnitPrice CHECK (UnitPrice >= 0)
);

-- ------------------------------------------------------------
-- Transactions
-- Sale    (type 1) must have a Customer only
-- Purchase (type 2) must have a Supplier only
-- Service  (type 3) must have a Vendor only
-- ------------------------------------------------------------
CREATE TABLE dbo.[Transaction] (
    TransactionId        INT IDENTITY(1,1) NOT NULL,
    TransactionTypeId    INT NOT NULL,
    TransactionStatusId  INT NOT NULL,
    CustomerId           INT NULL,
    SupplierId            INT NULL,
    VendorId              INT NULL,
    CreatedByUserId       INT NOT NULL,
    TransactionDate       DATETIME2 NOT NULL CONSTRAINT DF_Transaction_TransactionDate DEFAULT (SYSDATETIME()),
    TotalAmount           DECIMAL(12,2) NOT NULL CONSTRAINT DF_Transaction_TotalAmount DEFAULT (0),
    CreatedDate           DATETIME2 NOT NULL CONSTRAINT DF_Transaction_CreatedDate DEFAULT (SYSDATETIME()),
    ModifiedDate          DATETIME2 NOT NULL CONSTRAINT DF_Transaction_ModifiedDate DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Transaction PRIMARY KEY (TransactionId),
    CONSTRAINT FK_Transaction_TransactionType FOREIGN KEY (TransactionTypeId) REFERENCES dbo.TransactionType (TransactionTypeId),
    CONSTRAINT FK_Transaction_TransactionStatus FOREIGN KEY (TransactionStatusId) REFERENCES dbo.TransactionStatus (TransactionStatusId),
    CONSTRAINT FK_Transaction_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (CustomerId),
    CONSTRAINT FK_Transaction_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Supplier (SupplierId),
    CONSTRAINT FK_Transaction_Vendor FOREIGN KEY (VendorId) REFERENCES dbo.Vendor (VendorId),
    CONSTRAINT FK_Transaction_EndUser FOREIGN KEY (CreatedByUserId) REFERENCES dbo.EndUser (EndUserId),
    CONSTRAINT CK_Transaction_PartyMatch CHECK (
        (TransactionTypeId = 1 AND CustomerId IS NOT NULL AND SupplierId IS NULL AND VendorId IS NULL) OR
        (TransactionTypeId = 2 AND SupplierId IS NOT NULL AND CustomerId IS NULL AND VendorId IS NULL) OR
        (TransactionTypeId = 3 AND VendorId IS NOT NULL AND CustomerId IS NULL AND SupplierId IS NULL)
    )
);

CREATE TABLE dbo.TransactionItem (
    TransactionItemId  INT IDENTITY(1,1) NOT NULL,
    TransactionId       INT NOT NULL,
    ProductId            INT NULL,
    Description           NVARCHAR(200) NOT NULL,
    Quantity              INT NOT NULL,
    UnitPrice             DECIMAL(10,2) NOT NULL,
    LineTotal AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT PK_TransactionItem PRIMARY KEY (TransactionItemId),
    CONSTRAINT FK_TransactionItem_Transaction FOREIGN KEY (TransactionId) REFERENCES dbo.[Transaction] (TransactionId) ON DELETE CASCADE,
    CONSTRAINT FK_TransactionItem_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product (ProductId),
    CONSTRAINT CK_TransactionItem_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_TransactionItem_UnitPrice CHECK (UnitPrice >= 0)
);
GO

-- ------------------------------------------------------------
-- Indexes
-- ------------------------------------------------------------
CREATE UNIQUE NONCLUSTERED INDEX UQ_Product_SKU
    ON dbo.Product (SKU)
    WHERE SKU IS NOT NULL AND SKU <> N'';

CREATE NONCLUSTERED INDEX IX_Transaction_TransactionDate ON dbo.[Transaction] (TransactionDate DESC);
CREATE NONCLUSTERED INDEX IX_Transaction_CustomerId ON dbo.[Transaction] (CustomerId) WHERE CustomerId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Transaction_SupplierId ON dbo.[Transaction] (SupplierId) WHERE SupplierId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Transaction_VendorId ON dbo.[Transaction] (VendorId) WHERE VendorId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_TransactionItem_TransactionId ON dbo.TransactionItem (TransactionId);
CREATE NONCLUSTERED INDEX IX_Customer_Name ON dbo.Customer (Name);
CREATE NONCLUSTERED INDEX IX_Supplier_Name ON dbo.Supplier (Name);
CREATE NONCLUSTERED INDEX IX_Vendor_Name ON dbo.Vendor (Name);
CREATE NONCLUSTERED INDEX IX_Product_Name ON dbo.Product (ProductName);
GO
