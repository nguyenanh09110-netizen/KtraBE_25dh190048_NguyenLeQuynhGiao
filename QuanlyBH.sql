IF DB_ID('QuanlyBH') IS NULL
CREATE DATABASE QuanlyBH;
GO
USE QuanlyBH;
GO

-- 1. Bảng User: quản lý cả tài khoản của Khách hàng và Admin (Phân quyền: A: admin, C: customer)
CREATE TABLE [dbo].[User] (
    [Username]     NVARCHAR (255) NOT NULL,
    [Password]     NVARCHAR (100) NOT NULL,
    [UserRole]     NCHAR (1)      NOT NULL,
    PRIMARY KEY CLUSTERED ([Username] ASC)
);
GO

-- 2. Bảng Customer: lưu thông tin khách hàng
CREATE TABLE [dbo].[Customer] (
    [CustomerID]       INT            IDENTITY (1, 1) NOT NULL,
    [CustomerName]     NVARCHAR (MAX) NOT NULL,
    [CustomerPhone]    NVARCHAR (15)  NOT NULL,
    [CustomerEmail]    NVARCHAR (MAX) NULL,
    [CustomerAddress]  NVARCHAR (MAX) NULL,
    [Username]         NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([CustomerID] ASC),
    CONSTRAINT [FK_User_Customer] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username])
);
GO

-- 3. Bảng Category: lưu thông tin danh mục sản phẩm
CREATE TABLE [dbo].[Category] (
    [CategoryID]       INT            IDENTITY (1, 1) NOT NULL,
    [CategoryName]     NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
GO

-- 4. Bảng Product: lưu thông tin sản phẩm
CREATE TABLE [dbo].[Product] (
    [ProductID]         INT             IDENTITY (1, 1) NOT NULL,
    [CategoryID]        INT             NOT NULL,
    [ProductName]       NVARCHAR (MAX)  NOT NULL,
    [ProductDecription] NVARCHAR (MAX)  NOT NULL,
    [ProductPrice]      DECIMAL (18, 2) NOT NULL,
    [ProductImage]      NVARCHAR (MAX)  NULL,
    [Quantity]          INT             DEFAULT (0) NOT NULL, -- Bổ sung trường tồn kho phục vụ checklist
    [Sold]              INT             DEFAULT (0) NULL,     -- Bổ sung trường đã bán
    PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
);
GO

-- 5. Bảng Order: lưu thông tin đơn hàng của khách hàng
CREATE TABLE [dbo].[Order] (
    [OrderID]          INT             IDENTITY (1, 1) NOT NULL,
    [CustomerID]       INT             NOT NULL,
    [OrderDate]        DATE            NOT NULL,
    [TotalAmount]      DECIMAL (18, 2) NOT NULL,
    [PaymentStatus]    NVARCHAR (MAX)  NULL,
    [AddressDelivery]  NVARCHAR (MAX)  NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderID] ASC),
    FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
);
GO

-- 6. Bảng OrderDetail: lưu thông tin chi tiết các sản phẩm có trong đơn hàng
CREATE TABLE [dbo].[OrderDetail] (
    [ID]        INT             IDENTITY (1, 1) NOT NULL,
    [ProductID] INT             NOT NULL,
    [OrderID]   INT             NOT NULL,
    [Quantity]  INT             NOT NULL,
    [UnitPrice] DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product] ([ProductID]),
    FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order] ([OrderID])
);
GO

-- Thêm dữ liệu mẫu tài khoản Admin vào bảng User
INSERT INTO [dbo].[User] ([Username], [Password], [UserRole]) 
VALUES ('admin', '123456', 'A');
GO

-- Thêm vài danh mục mẫu
INSERT INTO [dbo].[Category] ([CategoryName]) 
VALUES (N'Áo thun'), 
       (N'Quần Jean');
GO
-- Thêm dữ liệu mẫu vào bảng Product
INSERT INTO [dbo].[Product] ([ProductName], [ProductDecription], [ProductPrice], [ProductImage], [Quantity], [Sold], [CategoryID])
VALUES 
(N'Áo Thun Form Rộng', N'Áo thun cotton cao cấp thoáng mát', 150000, 'ao_thun.jpg', 50, 0, 1),
(N'Quần Jean Nam', N'Quần jean phong cách Hàn Quốc', 250000, 'quan_jean.jpg', 30, 0, 2);
GO

DELETE FROM [dbo].[Product];
DBCC CHECKIDENT ('Product', RESEED, 0);
GO

INSERT INTO [dbo].[Product] ([ProductName], [ProductDecription], [ProductPrice], [ProductImage], [Quantity], [Sold], [CategoryID])
VALUES 
(N'Áo Thun Form Rộng', N'Áo thun cotton cao cấp thoáng mát', 150000, 'ao_thun.jpg', 50, 0, 1),
(N'Quần Jean Nam', N'Quần jean phong cách Hàn Quốc', 250000, 'quan_jean.jpg', 30, 0, 1),
(N'Ví Da Bò', N'Ví da thật 100% cao cấp', 350000, 'vi_da.jpg', 20, 0, 2);
GO

DELETE FROM [dbo].[Product];
DELETE FROM [dbo].[Category];
GO

DBCC CHECKIDENT ('Product', RESEED, 0);
DBCC CHECKIDENT ('Category', RESEED, 0);
GO

INSERT INTO [dbo].[Category] ([CategoryName])
VALUES 
(N'Áo thun'),       
(N'Quần Jean'),     
(N'Ví da');         
GO

INSERT INTO [dbo].[Product] ([ProductName], [ProductDecription], [ProductPrice], [ProductImage], [Quantity], [Sold], [CategoryID])
VALUES 
(N'Áo Thun Form Rộng', N'Áo thun cotton cao cấp thoáng mát', 150000, 'ao_thun.jpg', 50, 0, 1),
(N'Quần Jean Nam', N'Quần jean phong cách Hàn Quốc', 250000, 'quan_jean.jpg', 30, 0, 2),
(N'Ví Da Bò', N'Ví da thật 100% cao cấp', 350000, 'vi_da.jpg', 20, 0, 3);
GO

ALTER TABLE [User]
ALTER COLUMN [UserRole] NVARCHAR(20) NOT NULL;

SELECT Username, Password, UserRole FROM [dbo].[User];

ALTER TABLE [Order]
ADD OrderStatus NVARCHAR(50) NULL;