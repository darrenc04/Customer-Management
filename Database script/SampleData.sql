USE CustomerDB;
GO

INSERT INTO dbo.Customer(Username, Age, Contact, Email, CreatedBy)
VALUES
	(N'user1', 22, N'0123456789', N'user1@gmail.com', N'admin'),
	(N'user2', 23, N'0123456781', N'user2@gmail.com', N'admin'),
	(N'user3', 24, N'0123456782', N'user3@gmail.com', N'admin'),
	(N'user4', 25, N'0123456783', N'user4@gmail.com', N'admin'),
	(N'user5', 26, N'0123456784', N'user5@gmail.com', N'admin')
GO

