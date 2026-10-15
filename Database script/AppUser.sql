USE CustomerDB
GO

CREATE TABLE dbo.AppUser
(
	UserID			INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Username		NVARCHAR(50) NOT NULL UNIQUE,
	PasswordHash	NVARCHAR(200) NOT NULL,
	IsActive		BIT NOT NULL DEFAULT 1,
	CreatedDate		DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE OR ALTER PROCEDURE dbo.usp_AppUser_GetByUsername
	@Username NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT UserID, Username, PasswordHash, IsActive
	FROM dbo.AppUser
	WHERE Username = @Username;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_AppUser_Insert
	@Username NVARCHAR(50),
	@PasswordHash NVARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT 1 FROM dbo.AppUser WHERE Username = @Username)
	THROW 50003, 'Login username already exists.', 1;

	INSERT INTO dbo.AppUser(Username, PasswordHash)
	VALUES (@Username, @PasswordHash);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_AppUser_Delete
	@Username	NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM dbo.AppUser
	WHERE Username = @Username;

	IF @@ROWCOUNT = 0
		THROW 50004, 'User not found.', 1;
END
GO