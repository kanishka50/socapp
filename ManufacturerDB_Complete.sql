IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [ManufacturerOrders] (
    [Id] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(50) NOT NULL,
    [DistributorId] int NOT NULL,
    [DistributorName] nvarchar(200) NOT NULL,
    [DistributorOrderNumber] nvarchar(50) NOT NULL,
    [Status] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Notes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ManufacturerOrders] PRIMARY KEY ([Id])
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [CurrentStock] int NOT NULL,
    [ReservedStock] int NOT NULL,
    [ProductionCapacityPerDay] int NOT NULL,
    [ManufacturingCost] decimal(18,2) NOT NULL,
    [LeadTimeDays] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Material] nvarchar(max) NOT NULL,
    [Size] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [SKU] nvarchar(50) NOT NULL,
    [MinStockLevel] int NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [Category] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(256) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [CompanyName] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [InventoryTransactions] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [TransactionType] nvarchar(20) NOT NULL,
    [Quantity] int NOT NULL,
    [Reference] nvarchar(100) NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryTransactions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ManufacturerOrderItems] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ManufacturerOrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ManufacturerOrderItems_ManufacturerOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ManufacturerOrders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ManufacturerOrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProductMaterials] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [MaterialName] nvarchar(200) NOT NULL,
    [MaterialType] nvarchar(max) NOT NULL,
    [QuantityRequired] decimal(18,2) NOT NULL,
    [Unit] nvarchar(20) NOT NULL,
    [CostPerUnit] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ProductMaterials] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductMaterials_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Category', N'CreatedAt', N'CreatedBy', N'CurrentStock', N'Description', N'ImageUrl', N'IsActive', N'LeadTimeDays', N'ManufacturingCost', N'Material', N'MinStockLevel', N'Name', N'Price', N'ProductionCapacityPerDay', N'ReservedStock', N'SKU', N'Size', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id], [Category], [CreatedAt], [CreatedBy], [CurrentStock], [Description], [ImageUrl], [IsActive], [LeadTimeDays], [ManufacturingCost], [Material], [MinStockLevel], [Name], [Price], [ProductionCapacityPerDay], [ReservedStock], [SKU], [Size], [UpdatedAt], [UpdatedBy])
VALUES (1, N'Wool Blankets', '2024-01-01T00:00:00.0000000Z', NULL, 50, N'Premium 100% Merino wool blanket, perfect for cold nights', N'/images/luxury-wool-queen.jpg', CAST(1 AS bit), 3, 75.0, N'100% Merino Wool', 20, N'Luxury Wool Blanket - Queen Size', 199.99, 10, 0, N'LWB-Q-001', N'Queen (90x90 inches)', NULL, NULL),
(2, N'Cotton Blankets', '2024-01-01T00:00:00.0000000Z', NULL, 75, N'Soft and breathable 100% organic cotton blanket', N'/images/cotton-comfort-king.jpg', CAST(1 AS bit), 2, 55.0, N'100% Organic Cotton', 25, N'Cotton Comfort Blanket - King Size', 149.99, 15, 5, N'CCB-K-001', N'King (108x90 inches)', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Category', N'CreatedAt', N'CreatedBy', N'CurrentStock', N'Description', N'ImageUrl', N'IsActive', N'LeadTimeDays', N'ManufacturingCost', N'Material', N'MinStockLevel', N'Name', N'Price', N'ProductionCapacityPerDay', N'ReservedStock', N'SKU', N'Size', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompanyName', N'CreatedAt', N'CreatedBy', N'Email', N'FirstName', N'IsActive', N'LastName', N'PasswordHash', N'Phone', N'Role', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [CompanyName], [CreatedAt], [CreatedBy], [Email], [FirstName], [IsActive], [LastName], [PasswordHash], [Phone], [Role], [UpdatedAt], [UpdatedBy])
VALUES (1, NULL, '2024-01-01T00:00:00.0000000Z', NULL, N'admin@cozycomfort.com', N'System', CAST(1 AS bit), N'Administrator', N'$2a$11$ZGsLwskCPFNMmT8lEV2ELetqSMN5XC1nBR1eEI8FmGN5dQdB2Ibvy', NULL, 1, NULL, NULL),
(2, N'Cozy Comfort Manufacturing', '2024-01-01T00:00:00.0000000Z', NULL, N'manufacturer@cozycomfort.com', N'John', CAST(1 AS bit), N'Manufacturer', N'$2a$11$Q3p1VQDLzq.VQDlzq.VQDlzq.VQ3p1VQDLzq.VQDlzq.VQ3p1VQ', NULL, 2, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompanyName', N'CreatedAt', N'CreatedBy', N'Email', N'FirstName', N'IsActive', N'LastName', N'PasswordHash', N'Phone', N'Role', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

CREATE INDEX [IX_InventoryTransactions_ProductId] ON [InventoryTransactions] ([ProductId]);

CREATE INDEX [IX_ManufacturerOrderItems_OrderId] ON [ManufacturerOrderItems] ([OrderId]);

CREATE INDEX [IX_ManufacturerOrderItems_ProductId] ON [ManufacturerOrderItems] ([ProductId]);

CREATE UNIQUE INDEX [IX_ManufacturerOrders_OrderNumber] ON [ManufacturerOrders] ([OrderNumber]);

CREATE INDEX [IX_ProductMaterials_ProductId] ON [ProductMaterials] ([ProductId]);

CREATE UNIQUE INDEX [IX_Products_SKU] ON [Products] ([SKU]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624203354_InitialCreateWithOrders', N'9.0.6');

COMMIT;
GO

