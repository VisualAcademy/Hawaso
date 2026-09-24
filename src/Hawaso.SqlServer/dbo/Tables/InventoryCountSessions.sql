CREATE TABLE [dbo].[InventoryCountSessions]
(
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_InventoryCountSessions] PRIMARY KEY,
    [CountNumber] NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(255) NULL,
    [WarehouseId] INT NOT NULL,
    [WarehouseLocationId] INT NULL,
    [InventoryCategoryId] INT NULL,
    [CountType] NVARCHAR(50) NOT NULL CONSTRAINT [DF_InventoryCountSessions_CountType] DEFAULT(N'Warehouse'),
    [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_InventoryCountSessions_Status] DEFAULT(N'Open'),
    [SnapshotDate] DATETIME2(0) NOT NULL,
    [CountDate] DATETIME2(0) NOT NULL,
    [Notes] NVARCHAR(2000) NULL,
    [CreatedBy] NVARCHAR(255) NULL,
    [Created] DATETIME2(0) NULL CONSTRAINT [DF_InventoryCountSessions_Created] DEFAULT(SYSDATETIME()),
    [ModifiedBy] NVARCHAR(255) NULL,
    [Modified] DATETIME2(0) NULL,
    [CompletedBy] NVARCHAR(255) NULL,
    [CompletedDate] DATETIME2(0) NULL,
    [AdjustedBy] NVARCHAR(255) NULL,
    [AdjustedDate] DATETIME2(0) NULL,
    CONSTRAINT [FK_InventoryCountSessions_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [dbo].[Warehouses]([Id]),
    CONSTRAINT [FK_InventoryCountSessions_WarehouseLocations] FOREIGN KEY ([WarehouseLocationId]) REFERENCES [dbo].[WarehouseLocations]([Id]),
    CONSTRAINT [FK_InventoryCountSessions_InventoryCategories] FOREIGN KEY ([InventoryCategoryId]) REFERENCES [dbo].[InventoryCategories]([Id])
);
GO
CREATE UNIQUE INDEX [UX_InventoryCountSessions_CountNumber] ON [dbo].[InventoryCountSessions]([CountNumber]);
GO
CREATE INDEX [IX_InventoryCountSessions_Warehouse_Status_Date] ON [dbo].[InventoryCountSessions]([WarehouseId], [Status], [CountDate] DESC);
GO
