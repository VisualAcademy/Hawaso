CREATE TABLE [dbo].[Warehouses]
(
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Warehouses] PRIMARY KEY,
    [Code] NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [AddressLine1] NVARCHAR(255) NULL,
    [AddressLine2] NVARCHAR(255) NULL,
    [City] NVARCHAR(100) NULL,
    [StateProvince] NVARCHAR(100) NULL,
    [PostalCode] NVARCHAR(30) NULL,
    [Country] NVARCHAR(100) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Warehouses_IsActive] DEFAULT(1),
    [CreatedBy] NVARCHAR(255) NULL,
    [Created] DATETIME2(0) NULL CONSTRAINT [DF_Warehouses_Created] DEFAULT(SYSDATETIME()),
    [ModifiedBy] NVARCHAR(255) NULL,
    [Modified] DATETIME2(0) NULL
);
GO
CREATE UNIQUE INDEX [UX_Warehouses_Code] ON [dbo].[Warehouses]([Code]);
GO
