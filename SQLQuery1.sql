Create database Fuddo;

use Fuddo;
-- Người dùng (có thể là admin hoặc khách)
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User' -- User hoặc Admin
);

-- Danh mục sản phẩm (Snack, Kẹo, Bánh, Nước uống...)
CREATE TABLE Category (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL
);

-- Sản phẩm
CREATE TABLE Product (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10, 2) NOT NULL,
    QuantityInStock INT NOT NULL DEFAULT 0,
    CategoryId INT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Category(Id)
);

-- Ảnh sản phẩm (1 sản phẩm có nhiều ảnh)
CREATE TABLE ProductImage (
    Id INT PRIMARY KEY IDENTITY,
    ImageUrl NVARCHAR(300) NOT NULL,
    ProductId INT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Product(Id)
);

-- Đơn hàng
CREATE TABLE [Order] (
    Id INT PRIMARY KEY IDENTITY,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](Id)
);

-- Chi tiết đơn hàng
CREATE TABLE OrderDetail (
    Id INT PRIMARY KEY IDENTITY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES [Order](Id),
    FOREIGN KEY (ProductId) REFERENCES Product(Id)
);
