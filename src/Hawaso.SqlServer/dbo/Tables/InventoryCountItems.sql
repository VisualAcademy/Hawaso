CREATE TABLE [dbo].[InventoryCountItems]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_InventoryCountItems] PRIMARY KEY,
    [InventoryCountSessionId] INT NOT NULL,
    [InventoryStockId] INT NOT NULL,
    [InventoryItemId] INT NOT NULL,
    [WarehouseLocationId] INT NULL,
    [StockStatusSnapshot] NVARCHAR(50) NULL,
    [TrackingType] NVARCHAR(30) NULL,
    [SerialNumber] NVARCHAR(255) NULL,
    [LotNumber] NVARCHAR(255) NULL,
    [AssetTag] NVARCHAR(255) NULL,
    [ExpectedQuantity] DECIMAL(18,4) NOT NULL,
    [CountedQuantity] DECIMAL(18,4) NULL,
    [CountStatus] NVARCHAR(30) NOT NULL CONSTRAINT [DF_InventoryCountItems_CountStatus] DEFAULT(N'Pending'),
    [Notes] NVARCHAR(2000) NULL,
    [CountedBy] NVARCHAR(255) NULL,
    [CountedDate] DATETIME2(0) NULL,
    [AdjustedBy] NVARCHAR(255) NULL,
    [AdjustedDate] DATETIME2(0) NULL,
    [AdjustmentTransactionId] BIGINT NULL,
    CONSTRAINT [FK_InventoryCountItems_Sessions] FOREIGN KEY ([InventoryCountSessionId]) REFERENCES [dbo].[InventoryCountSessions]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryCountItems_Stocks] FOREIGN KEY ([InventoryStockId]) REFERENCES [dbo].[InventoryStocks]([Id]),
    CONSTRAINT [FK_InventoryCountItems_Items] FOREIGN KEY ([InventoryItemId]) REFERENCES [dbo].[InventoryItems]([Id]),
    CONSTRAINT [FK_InventoryCountItems_Locations] FOREIGN KEY ([WarehouseLocationId]) REFERENCES [dbo].[WarehouseLocations]([Id])
);
GO
CREATE UNIQUE INDEX [UX_InventoryCountItems_Session_Stock] ON [dbo].[InventoryCountItems]([InventoryCountSessionId], [InventoryStockId]);
GO
CREATE INDEX [IX_InventoryCountItems_Session_Status] ON [dbo].[InventoryCountItems]([InventoryCountSessionId], [CountStatus]);
GO
