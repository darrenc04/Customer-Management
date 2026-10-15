USE CustomerDB;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_GetAll
AS
BEGIN
	SET NOCOUNT ON;

	SELECT CustomerID, Username, Age, Contact, Email, CreatedBy, CreatedDate, LastUpdate
	FROM dbo.Customer
	ORDER BY CustomerID;
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

CREATE OR ALTER PROCEDURE dbo.usp_Customer_GetById
	@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT CustomerID, Username, Age, Contact, Email, CreatedBy, CreatedDate, LastUpdate
	FROM dbo.Customer
	WHERE CustomerID = @CustomerID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Insert
	@Username	NVARCHAR(50),
	@Age		INT,
	@Contact	NVARCHAR(12),
	@Email		NVARCHAR(50),
	@CreatedBy	NVARCHAR(128)
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
	@CustomerID	INT,
	@Username	NVARCHAR(50),
	@Age		INT,
	@Contact	NVARCHAR(12),
	@Email		NVARCHAR(50)
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
		Email = @Email,
		LastUpdate = SYSDATETIME()
	WHERE CustomerID = @CustomerID;

	IF @@ROWCOUNT = 0
		THROW 50002, 'Customer not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Delete
	@CustomerID	INT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM dbo.Customer
	WHERE CustomerID = @CustomerID;

	IF @@ROWCOUNT = 0
		THROW 50002, 'Customer not found.', 1;
END
GO
