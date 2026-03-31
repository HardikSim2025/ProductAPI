-- ProductDB Database Creation Script
-- This script creates the Products table for the Product API

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductDB')
BEGIN
    CREATE DATABASE ProductDB;
END
GO

USE ProductDB;
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Category NVARCHAR(100) NULL,
        Stock INT NOT NULL,
        CreatedAt DATETIME DEFAULT GETUTCDATE()
    );
    
    -- Create indexes for common queries
    CREATE INDEX IX_Products_Category ON Products(Category);
    CREATE INDEX IX_Products_Name ON Products(Name);
    
    PRINT 'Products table created successfully.';
END
ELSE
BEGIN
    PRINT 'Products table already exists.';
END
GO

-- Add check constraints for validation
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Products_Price')
BEGIN
    ALTER TABLE Products
    ADD CONSTRAINT CK_Products_Price CHECK (Price > 0);
    PRINT 'Price constraint added.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Products_Stock')
BEGIN
    ALTER TABLE Products
    ADD CONSTRAINT CK_Products_Stock CHECK (Stock >= 0);
    PRINT 'Stock constraint added.';
END
GO
