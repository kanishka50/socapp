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
CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(max) NOT NULL,
    [CustomerName] nvarchar(max) NOT NULL,
    [CustomerEmail] nvarchar(max) NOT NULL,
    [CustomerPhone] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [Tax] decimal(18,2) NOT NULL,
    [ShippingCost] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ShippingAddress] nvarchar(max) NOT NULL,
    [BillingAddress] nvarchar(max) NOT NULL,
    [PaymentMethod] nvarchar(max) NOT NULL,
    [IsPaid] bit NOT NULL,
    [PaidAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [DistributorProductId] int NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [SKU] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SellingPrice] decimal(18,2) NOT NULL,
    [CurrentStock] int NOT NULL,
    [DisplayStock] int NOT NULL,
    [IsAvailable] bit NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(450) NOT NULL,
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

CREATE TABLE [OrderItems] (
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
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Category', N'CreatedAt', N'CreatedBy', N'CurrentStock', N'Description', N'DisplayStock', N'DistributorProductId', N'ImageUrl', N'IsActive', N'IsAvailable', N'ProductName', N'PurchasePrice', N'SKU', N'SellingPrice', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id], [Category], [CreatedAt], [CreatedBy], [CurrentStock], [Description], [DisplayStock], [DistributorProductId], [ImageUrl], [IsActive], [IsAvailable], [ProductName], [PurchasePrice], [SKU], [SellingPrice], [UpdatedAt], [UpdatedBy])
VALUES (1, N'Chairs', '2025-06-24T05:36:35.3917002Z', NULL, 25, N'Comfortable office chair for retail', 25, 1, N'/images/office-chair-retail.jpg', CAST(1 AS bit), CAST(1 AS bit), N'Office Chair - Retail', 250.0, N'CHAIR-RETAIL-001', 399.99, NULL, NULL),
(2, N'Desks', '2025-06-24T05:36:35.3917014Z', NULL, 15, N'Adjustable standing desk for retail', 15, 2, N'/images/standing-desk-retail.jpg', CAST(1 AS bit), CAST(1 AS bit), N'Standing Desk - Retail', 500.0, N'DESK-RETAIL-001', 799.99, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Category', N'CreatedAt', N'CreatedBy', N'CurrentStock', N'Description', N'DisplayStock', N'DistributorProductId', N'ImageUrl', N'IsActive', N'IsAvailable', N'ProductName', N'PurchasePrice', N'SKU', N'SellingPrice', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompanyName', N'CreatedAt', N'CreatedBy', N'Email', N'FirstName', N'IsActive', N'LastName', N'PasswordHash', N'Phone', N'Role', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [CompanyName], [CreatedAt], [CreatedBy], [Email], [FirstName], [IsActive], [LastName], [PasswordHash], [Phone], [Role], [UpdatedAt], [UpdatedBy])
VALUES (1, NULL, '2025-06-24T05:36:35.1974471Z', NULL, N'admin@seller.com', N'Admin', CAST(1 AS bit), N'Seller', N'$2a$11$0NGK9YqKL/Klw1xBvq89cOChPHLy3g4MzbFS8VBWrkOopNgnaRdn.', NULL, 1, NULL, NULL),
(2, NULL, '2025-06-24T05:36:35.3907973Z', NULL, N'seller@cozycomfort.com', N'Bob', CAST(1 AS bit), N'Seller', N'$2a$11$IWdMv4yl.Z7LqzF69Dgy8u1BWKvpsdfQlmwFdB3M11ppIdhDB42fq', NULL, 4, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompanyName', N'CreatedAt', N'CreatedBy', N'Email', N'FirstName', N'IsActive', N'LastName', N'PasswordHash', N'Phone', N'Role', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624053635_InitialCreate', N'9.0.6');

ALTER TABLE [OrderItems] DROP CONSTRAINT [FK_OrderItems_Orders_OrderId];

ALTER TABLE [OrderItems] DROP CONSTRAINT [FK_OrderItems_Products_ProductId];

ALTER TABLE [Products] DROP CONSTRAINT [PK_Products];

ALTER TABLE [Orders] DROP CONSTRAINT [PK_Orders];

ALTER TABLE [OrderItems] DROP CONSTRAINT [PK_OrderItems];

EXEC sp_rename N'[Products]', N'SellerProducts', 'OBJECT';

EXEC sp_rename N'[Orders]', N'CustomerOrders', 'OBJECT';

EXEC sp_rename N'[OrderItems]', N'CustomerOrderItems', 'OBJECT';

EXEC sp_rename N'[CustomerOrderItems].[IX_OrderItems_ProductId]', N'IX_CustomerOrderItems_ProductId', 'INDEX';

EXEC sp_rename N'[CustomerOrderItems].[IX_OrderItems_OrderId]', N'IX_CustomerOrderItems_OrderId', 'INDEX';

ALTER TABLE [SellerProducts] ADD CONSTRAINT [PK_SellerProducts] PRIMARY KEY ([Id]);

ALTER TABLE [CustomerOrders] ADD CONSTRAINT [PK_CustomerOrders] PRIMARY KEY ([Id]);

ALTER TABLE [CustomerOrderItems] ADD CONSTRAINT [PK_CustomerOrderItems] PRIMARY KEY ([Id]);

CREATE TABLE [SellerDistributorOrders] (
    [Id] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(50) NOT NULL,
    [DistributorId] int NOT NULL,
    [DistributorOrderNumber] nvarchar(50) NOT NULL,
    [Status] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [ExpectedDeliveryDate] datetime2 NULL,
    [ActualDeliveryDate] datetime2 NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ShippingAddress] nvarchar(500) NOT NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SellerDistributorOrders] PRIMARY KEY ([Id])
);

CREATE TABLE [SellerInventoryTransactions] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [TransactionType] nvarchar(20) NOT NULL,
    [Quantity] int NOT NULL,
    [Reference] nvarchar(100) NOT NULL,
    [Notes] nvarchar(500) NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    [UnitCost] decimal(18,2) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SellerInventoryTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SellerInventoryTransactions_SellerProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [SellerProducts] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SellerDistributorOrderItems] (
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
    CONSTRAINT [PK_SellerDistributorOrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SellerDistributorOrderItems_SellerDistributorOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [SellerDistributorOrders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SellerDistributorOrderItems_SellerProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [SellerProducts] ([Id]) ON DELETE NO ACTION
);

UPDATE [SellerProducts] SET [CreatedAt] = '2024-01-01T00:00:00.0000000Z'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [SellerProducts] SET [CreatedAt] = '2024-01-01T00:00:00.0000000Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedAt] = '2024-01-01T00:00:00.0000000Z', [PasswordHash] = N'$2a$11$RZvBwJL7e8OadjLPNgW7x.Km7NkkdVNoLSbEVmNGbxSu.Pnw8dXLa'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedAt] = '2024-01-01T00:00:00.0000000Z', [PasswordHash] = N'$2a$11$5VBm7OKqiM6XGQqATcQTb.SZeh7mr8fVyVuVC.M8FQ8g1jLOSGGBu'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


CREATE INDEX [IX_SellerDistributorOrderItems_OrderId] ON [SellerDistributorOrderItems] ([OrderId]);

CREATE INDEX [IX_SellerDistributorOrderItems_ProductId] ON [SellerDistributorOrderItems] ([ProductId]);

CREATE UNIQUE INDEX [IX_SellerDistributorOrders_OrderNumber] ON [SellerDistributorOrders] ([OrderNumber]);

CREATE INDEX [IX_SellerInventoryTransactions_ProductId_TransactionDate] ON [SellerInventoryTransactions] ([ProductId], [TransactionDate]);

CREATE INDEX [IX_SellerInventoryTransactions_TransactionDate] ON [SellerInventoryTransactions] ([TransactionDate]);

ALTER TABLE [CustomerOrderItems] ADD CONSTRAINT [FK_CustomerOrderItems_CustomerOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [CustomerOrders] ([Id]) ON DELETE CASCADE;

ALTER TABLE [CustomerOrderItems] ADD CONSTRAINT [FK_CustomerOrderItems_SellerProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [SellerProducts] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250627010327_AddDistributorOrderTables', N'9.0.6');

COMMIT;
GO

