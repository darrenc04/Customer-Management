-- 1. Create Database
IF DB_ID('CustomerDB') IS NULL
BEGIN
    CREATE DATABASE CustomerDB;
END
GO

USE CustomerDB;
GO

-- 2. Create Customer Table
IF OBJECT_ID('dbo.Customer', 'U') IS NOT NULL
    DROP TABLE dbo.Customer;
GO

CREATE TABLE dbo.Customer
(
    CustomerID   INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)      NOT NULL UNIQUE,
    Age          INT               NOT NULL,
    Contact      NVARCHAR(12)      NOT NULL,
    Email        NVARCHAR(50)      NOT NULL,
    CreatedBy    NVARCHAR(128)     NOT NULL DEFAULT N'admin',
    CreatedDate  DATETIME2(0)      NOT NULL DEFAULT SYSDATETIME()
);
GO

-- 3. Stored Procedures
CREATE OR ALTER PROCEDURE dbo.usp_Customer_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CustomerID, Username, Age, Contact, Email, CreatedBy, CreatedDate
    FROM dbo.Customer
    ORDER BY CustomerID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_GetById
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CustomerID, Username, Age, Contact, Email, CreatedBy, CreatedDate
    FROM dbo.Customer
    WHERE CustomerID = @CustomerID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Search
	@SearchText NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Pattern NVARCHAR(220) = 
		N'%' + REPLACE(REPLACE(REPLACE(@SearchText, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
	
	SELECT CustomerID, Username, Age, Contact, Email, CreatedBy, CreatedDate, LastUpdate
	FROM dbo.Customer
	WHERE Username LIKE @Pattern
		OR Email   LIKE @Pattern
	ORDER BY CustomerID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Insert
    @Username    NVARCHAR(50),
    @Age         INT,
    @Contact     NVARCHAR(12),
    @Email       NVARCHAR(50),
    @CreatedBy   NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Customer WHERE Username = @Username)
        THROW 50001, 'Username already exists.', 1;

    INSERT INTO dbo.Customer (Username, Age, Contact, Email, CreatedBy)
    VALUES (@Username, @Age, @Contact, @Email, @CreatedBy);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS CustomerID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Update
    @CustomerID INT,
    @Username   NVARCHAR(50),
    @Age        INT,
    @Contact    NVARCHAR(12),
    @Email      NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Customer WHERE Username = @Username AND CustomerID <> @CustomerID)
        THROW 50001, 'Username already exists.', 1;

    UPDATE dbo.Customer
    SET 
        Username = @Username,
        Age = @Age,
        Contact = @Contact,
        Email = @Email
    WHERE CustomerID = @CustomerID;

    IF @@ROWCOUNT = 0
        THROW 50002, 'Customer not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Delete
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Customer
    WHERE CustomerID = @CustomerID;

    IF @@ROWCOUNT = 0
        THROW 50002, 'Customer not found.', 1;
END
GO

-- 4. Insert Sample Data
IF NOT EXISTS (SELECT 1 FROM dbo.Customer)
BEGIN
    INSERT INTO dbo.Customer (Username, Age, Contact, Email, CreatedBy)
    VALUES
        (N'user1', 22, N'0123456789', N'user1@gmail.com', N'admin'),
        (N'user2', 23, N'0123456781', N'user2@gmail.com', N'admin'),
        (N'user3', 24, N'0123456782', N'user3@gmail.com', N'admin'),
        (N'user4', 25, N'0123456783', N'user4@gmail.com', N'admin'),
        (N'user5', 26, N'0123456784', N'user5@gmail.com', N'admin');
END
GO