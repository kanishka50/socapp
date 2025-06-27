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
CREATE TABLE [DistributorOrders] (
    [Id] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(50) NOT NULL,
    [OrderType] int NOT NULL,
    [ManufacturerId] int NULL,
    [SellerId] int NULL,
    [CustomerId] int NULL,
    [CustomerOrderNumber] nvarchar(max) NULL,
    [CustomerName] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [ExpectedDeliveryDate] datetime2 NULL,
    [ActualDeliveryDate] datetime2 NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ShippingAddress] nvarchar(500) NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DistributorOrders] PRIMARY KEY ([Id])
);

CREATE TABLE [DistributorProducts] (
    [Id] int NOT NULL IDENTITY,
    [ManufacturerProductId] int NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [SKU] nvarchar(50) NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SellingPrice] decimal(18,2) NOT NULL,
    [CurrentStock] int NOT NULL,
    [ReservedStock] int NOT NULL,
    [MinStockLevel] int NOT NULL,
    [ReorderPoint] int NOT NULL,
    [ReorderQuantity] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DistributorProducts] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(256) NOT NULL,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
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

CREATE TABLE [DistributorInventoryTransactions] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [TransactionType] nvarchar(20) NOT NULL,
    [Quantity] int NOT NULL,
    [Reference] nvarchar(max) NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    [UnitCost] decimal(18,2) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DistributorInventoryTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DistributorInventoryTransactions_DistributorProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [DistributorProducts] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DistributorOrderItems] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [QuantityReceived] int NULL,
    [Notes] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_DistributorOrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DistributorOrderItems_DistributorOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [DistributorOrders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DistributorOrderItems_DistributorProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [DistributorProducts] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'CurrentStock', N'IsActive', N'ManufacturerProductId', N'MinStockLevel', N'ProductName', N'PurchasePrice', N'ReorderPoint', N'ReorderQuantity', N'ReservedStock', N'SKU', N'SellingPrice', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[DistributorProducts]'))
    SET IDENTITY_INSERT [DistributorProducts] ON;
INSERT INTO [DistributorProducts] ([Id], [CreatedAt], [CreatedBy], [CurrentStock], [IsActive], [ManufacturerProductId], [MinStockLevel], [ProductName], [PurchasePrice], [ReorderPoint], [ReorderQuantity], [ReservedStock], [SKU], [SellingPrice], [UpdatedAt], [UpdatedBy])
VALUES (1, '2024-01-01T00:00:00.0000000', NULL, 25, CAST(1 AS bit), 1, 10, N'Luxury Wool Blanket - Queen Size', 199.99, 15, 30, 0, N'LWB-Q-001', 259.99, NULL, NULL),
(2, '2024-01-01T00:00:00.0000000', NULL, 40, CAST(1 AS bit), 2, 15, N'Cotton Comfort Blanket - King Size', 149.99, 20, 40, 5, N'CCB-K-001', 199.99, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'CurrentStock', N'IsActive', N'ManufacturerProductId', N'MinStockLevel', N'ProductName', N'PurchasePrice', N'ReorderPoint', N'ReorderQuantity', N'ReservedStock', N'SKU', N'SellingPrice', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[DistributorProducts]'))
    SET IDENTITY_INSERT [DistributorProducts] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompanyName', N'CreatedAt', N'CreatedBy', N'Email', N'FirstName', N'IsActive', N'LastName', N'PasswordHash', N'Phone', N'Role', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [CompanyName], [CreatedAt], [CreatedBy], [Email], [FirstName], [IsActive], [LastName], [PasswordHash], [Phone], [Role], [UpdatedAt], [UpdatedBy])
VALUES (1, N'Central Distribution Ltd', '2024-01-01T00:00:00.0000000', NULL, N'distributor@cozycomfort.com', N'David', CAST(1 AS bit), N'Distributor', N'$2a$11$rQf8Fx8Kz2Wn7vJ4RjPmE.9lK3qS2hF1GxN5pT7dM8aC6bV9eWxYi', NULL, 3, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompanyName', N'CreatedAt', N'CreatedBy', N'Email', N'FirstName', N'IsActive', N'LastName', N'PasswordHash', N'Phone', N'Role', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

CREATE INDEX [IX_DistributorInventoryTransactions_ProductId] ON [DistributorInventoryTransactions] ([ProductId]);

CREATE INDEX [IX_DistributorOrderItems_OrderId] ON [DistributorOrderItems] ([OrderId]);

CREATE INDEX [IX_DistributorOrderItems_ProductId] ON [DistributorOrderItems] ([ProductId]);

CREATE UNIQUE INDEX [IX_DistributorOrders_OrderNumber] ON [DistributorOrders] ([OrderNumber]);

CREATE UNIQUE INDEX [IX_DistributorProducts_SKU] ON [DistributorProducts] ([SKU]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624154537_AddStaticSeedData', N'9.0.6');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UpdatedBy');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Users] ALTER COLUMN [UpdatedBy] nvarchar(100) NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Phone');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Users] ALTER COLUMN [Phone] nvarchar(20) NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'LastName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Users] ALTER COLUMN [LastName] nvarchar(100) NOT NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'IsActive');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Users] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'FirstName');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Users] ALTER COLUMN [FirstName] nvarchar(100) NOT NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'CreatedBy');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Users] ALTER COLUMN [CreatedBy] nvarchar(100) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'CompanyName');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Users] ALTER COLUMN [CompanyName] nvarchar(200) NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorProducts]') AND [c].[name] = N'UpdatedBy');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [DistributorProducts] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [DistributorProducts] ALTER COLUMN [UpdatedBy] nvarchar(100) NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorProducts]') AND [c].[name] = N'IsActive');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [DistributorProducts] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [DistributorProducts] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorProducts]') AND [c].[name] = N'CreatedBy');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [DistributorProducts] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [DistributorProducts] ALTER COLUMN [CreatedBy] nvarchar(100) NULL;

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrders]') AND [c].[name] = N'UpdatedBy');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrders] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [DistributorOrders] ALTER COLUMN [UpdatedBy] nvarchar(100) NULL;

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrders]') AND [c].[name] = N'Notes');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrders] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [DistributorOrders] ALTER COLUMN [Notes] nvarchar(1000) NOT NULL;

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrders]') AND [c].[name] = N'IsActive');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrders] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [DistributorOrders] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrders]') AND [c].[name] = N'CustomerOrderNumber');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrders] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [DistributorOrders] ALTER COLUMN [CustomerOrderNumber] nvarchar(100) NULL;

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrders]') AND [c].[name] = N'CustomerName');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrders] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [DistributorOrders] ALTER COLUMN [CustomerName] nvarchar(200) NOT NULL;

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrders]') AND [c].[name] = N'CreatedBy');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrders] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [DistributorOrders] ALTER COLUMN [CreatedBy] nvarchar(100) NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrderItems]') AND [c].[name] = N'UpdatedBy');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrderItems] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [DistributorOrderItems] ALTER COLUMN [UpdatedBy] nvarchar(100) NULL;

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrderItems]') AND [c].[name] = N'Notes');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrderItems] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [DistributorOrderItems] ALTER COLUMN [Notes] nvarchar(500) NOT NULL;

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrderItems]') AND [c].[name] = N'IsActive');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrderItems] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [DistributorOrderItems] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorOrderItems]') AND [c].[name] = N'CreatedBy');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [DistributorOrderItems] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [DistributorOrderItems] ALTER COLUMN [CreatedBy] nvarchar(100) NULL;

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorInventoryTransactions]') AND [c].[name] = N'UpdatedBy');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [DistributorInventoryTransactions] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [DistributorInventoryTransactions] ALTER COLUMN [UpdatedBy] nvarchar(100) NULL;

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorInventoryTransactions]') AND [c].[name] = N'Reference');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [DistributorInventoryTransactions] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [DistributorInventoryTransactions] ALTER COLUMN [Reference] nvarchar(100) NOT NULL;

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorInventoryTransactions]') AND [c].[name] = N'Notes');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [DistributorInventoryTransactions] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [DistributorInventoryTransactions] ALTER COLUMN [Notes] nvarchar(500) NOT NULL;

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorInventoryTransactions]') AND [c].[name] = N'IsActive');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [DistributorInventoryTransactions] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [DistributorInventoryTransactions] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorInventoryTransactions]') AND [c].[name] = N'CreatedBy');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [DistributorInventoryTransactions] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [DistributorInventoryTransactions] ALTER COLUMN [CreatedBy] nvarchar(100) NULL;

UPDATE [Users] SET [Phone] = N'1234567890'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624155920_InitialCreateComplete', N'9.0.6');

UPDATE [Users] SET [PasswordHash] = N'$2a$11$QJk7fvgHt5KnrHcE9dGhF.8MJYl4ZzPu1CeQq2AvBnIiYHXM4QNYi'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624161337_FixedDistributorDatabase', N'9.0.6');

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DistributorProducts]') AND [c].[name] = N'ManufacturerProductId');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [DistributorProducts] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [DistributorProducts] ALTER COLUMN [ManufacturerProductId] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624223014_CheckDistributorUpdates', N'9.0.6');

COMMIT;
GO

