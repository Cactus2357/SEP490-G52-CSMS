-- =========================================================================================
-- MIGRATION SCRIPT: TÁCH BẢNG PAYMENTS KHỎI ORDERS & DI TRÚ DỮ LIỆU
-- Project: SEP490_G52_CSMS
-- Database: CSMSSystem
-- Date: 2026-09-06
-- Description:
--   1. Tạo bảng mới [payments] để quản lý độc lập từng giao dịch thanh toán / hoàn tiền.
--   2. Di trú toàn bộ dữ liệu lịch sử từ [orders] sang [payments] (Tiền mặt, Chuyển khoản, Hoàn tiền).
--   3. Xóa các Default Constraints và drop các cột thanh toán cũ khỏi bảng [orders].
-- =========================================================================================

USE [CSMSSystem];
GO

SET NOCOUNT ON;
PRINT N'--> Bắt đầu quá trình Migration: Tách bảng payments và chuyển đổi dữ liệu...';
GO

-- =========================================================================================
-- BƯỚC 1: TẠO BẢNG [payments] NẾU CHƯA TỒN TẠI
-- =========================================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payments')
BEGIN
    PRINT N'1. Tạo bảng [payments]...';
    
    CREATE TABLE [payments] (
        [payment_id] NVARCHAR(50) NOT NULL,
        [order_id] NVARCHAR(50) NOT NULL,
        [branch_id] NVARCHAR(20) NULL,
        [cashier_id] INT NULL,
        [payment_type] NVARCHAR(20) NOT NULL CONSTRAINT [DF_payments_payment_type] DEFAULT 'Payment', -- 'Payment', 'Refund'
        [payment_method] NVARCHAR(50) NOT NULL,                                                     -- 'Cash', 'BankTransfer', 'CreditCard', 'Momo'
        [amount] DECIMAL(18, 2) NOT NULL,
        [status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_payments_status] DEFAULT 'Success',            -- 'Success', 'Pending', 'Failed'
        [transaction_code] NVARCHAR(100) NULL,                                                      -- Mã GD ngân hàng / SePay ref
        [customer_cash] DECIMAL(18, 2) NULL,                                                        -- Tiền khách đưa (nếu là tiền mặt)
        [change_amount] DECIMAL(18, 2) NULL,                                                        -- Tiền thừa trả khách
        [notes] NVARCHAR(255) NULL,                                                                 -- Ghi chú / Lý do hoàn tiền
        [created_at] DATETIME2 NOT NULL CONSTRAINT [DF_payments_created_at] DEFAULT GETDATE(),
        
        CONSTRAINT [PK_payments] PRIMARY KEY CLUSTERED ([payment_id] ASC),
        CONSTRAINT [FK_payments_orders] FOREIGN KEY ([order_id]) REFERENCES [orders] ([order_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_payments_branches] FOREIGN KEY ([branch_id]) REFERENCES [branches] ([branch_id]),
        CONSTRAINT [FK_payments_employees] FOREIGN KEY ([cashier_id]) REFERENCES [employees] ([employee_id])
    );

    CREATE NONCLUSTERED INDEX [IX_payments_order_id] ON [payments] ([order_id] ASC);
    CREATE NONCLUSTERED INDEX [IX_payments_created_at] ON [payments] ([created_at] ASC);
    CREATE NONCLUSTERED INDEX [IX_payments_branch_id] ON [payments] ([branch_id] ASC);

    PRINT N'   Đã tạo thành công bảng [payments] và các chỉ mục liên quan.';
END
ELSE
BEGIN
    PRINT N'1. Bảng [payments] đã tồn tại sẵn, bỏ qua bước tạo bảng.';
END
GO

-- =========================================================================================
-- BƯỚC 2: DI TRÚ DỮ LIỆU (DATA TRANSFER) TỪ [orders] SANG [payments]
-- =========================================================================================
PRINT N'2. Di trú dữ liệu lịch sử thanh toán từ [orders] sang [payments]...';

-- 2.1. Đơn hàng có thu Tiền mặt cụ thể (cash_amount > 0)
INSERT INTO [payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at])
SELECT 
    'PAY-' + o.[order_id] + '-CSH',
    o.[order_id],
    o.[branch_id],
    o.[cashier_id],
    'Payment',
    'Cash',
    o.[cash_amount],
    'Success',
    NULL,
    o.[cash_amount],
    0,
    N'Di trú từ trường cash_amount cũ của đơn hàng',
    o.[created_at]
FROM [orders] o
WHERE o.[cash_amount] > 0
  AND NOT EXISTS (SELECT 1 FROM [payments] p WHERE p.[payment_id] = 'PAY-' + o.[order_id] + '-CSH');

-- 2.2. Đơn hàng có thu Chuyển khoản ngân hàng cụ thể (bank_amount > 0)
INSERT INTO [payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at])
SELECT 
    'PAY-' + o.[order_id] + '-BNK',
    o.[order_id],
    o.[branch_id],
    o.[cashier_id],
    'Payment',
    'BankTransfer',
    o.[bank_amount],
    'Success',
    o.[bank_transaction_code],
    NULL,
    NULL,
    N'Di trú từ trường bank_amount cũ của đơn hàng',
    o.[created_at]
FROM [orders] o
WHERE o.[bank_amount] > 0
  AND NOT EXISTS (SELECT 1 FROM [payments] p WHERE p.[payment_id] = 'PAY-' + o.[order_id] + '-BNK');

-- 2.3. Đơn hàng [Paid] cũ (trước khi có 2 cột cash_amount và bank_amount, lúc này cash_amount = 0 và bank_amount = 0)
INSERT INTO [payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at])
SELECT 
    'PAY-' + o.[order_id] + '-LEG',
    o.[order_id],
    o.[branch_id],
    o.[cashier_id],
    'Payment',
    CASE 
        WHEN o.[payment_method] LIKE '%Momo%' THEN 'Momo'
        WHEN o.[payment_method] LIKE '%Credit%' OR o.[payment_method] LIKE '%Card%' THEN 'CreditCard'
        WHEN o.[payment_method] LIKE '%Bank%' OR o.[payment_method] LIKE '%CK%' THEN 'BankTransfer'
        ELSE 'Cash'
    END,
    o.[total_amount],
    'Success',
    o.[bank_transaction_code],
    CASE 
        WHEN o.[payment_method] LIKE '%Cash%' OR o.[payment_method] IS NULL THEN o.[total_amount]
        ELSE NULL
    END,
    0,
    N'Di trú từ đơn hàng đã hoàn tất cũ (legacy Paid)',
    o.[created_at]
FROM [orders] o
WHERE o.[payment_status] = 'Paid'
  AND o.[cash_amount] = 0
  AND o.[bank_amount] = 0
  AND o.[total_amount] > 0
  AND NOT EXISTS (SELECT 1 FROM [payments] p WHERE p.[payment_id] = 'PAY-' + o.[order_id] + '-LEG');

-- 2.4. Đơn hàng có phát sinh Hoàn tiền (refund_amount > 0)
INSERT INTO [payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at])
SELECT 
    'REF-' + o.[order_id] + '-01',
    o.[order_id],
    o.[branch_id],
    o.[cashier_id],
    'Refund',
    ISNULL(o.[refund_method], 'Cash'),
    o.[refund_amount],
    'Success',
    NULL,
    NULL,
    NULL,
    ISNULL(o.[refund_reason], N'Hoàn tiền đơn hàng'),
    ISNULL(o.[refunded_at], o.[created_at])
FROM [orders] o
WHERE o.[refund_amount] > 0
  AND NOT EXISTS (SELECT 1 FROM [payments] p WHERE p.[payment_id] = 'REF-' + o.[order_id] + '-01');

PRINT N'   Đã hoàn thành di trú dữ liệu sang [payments].';
GO

-- =========================================================================================
-- BƯỚC 3: XÓA CÁC RÀNG BUỘC MẶC ĐỊNH & XÓA CỘT CŨ KHỎI [orders]
-- =========================================================================================
PRINT N'3. Xóa các trường dữ liệu thanh toán cũ khỏi [orders]...';

-- 3.1. Xóa các Default Constraints trên các cột sắp drop (tránh lỗi ALTER TABLE DROP COLUMN)
DECLARE @dropConstraintsSql NVARCHAR(MAX) = N'';

SELECT @dropConstraintsSql += N'ALTER TABLE [orders] DROP CONSTRAINT [' + d.name + N']; '
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
WHERE d.parent_object_id = OBJECT_ID('orders')
  AND c.name IN (
    'cash_amount',
    'bank_amount',
    'bank_transaction_code',
    'refund_amount',
    'refund_reason',
    'refund_method',
    'refunded_at'
  );

IF LEN(@dropConstraintsSql) > 0
BEGIN
    PRINT N'   Đang xóa các Default Constraints: ' + @dropConstraintsSql;
    EXEC sp_executesql @dropConstraintsSql;
END

-- 3.2. Drop các cột thanh toán cũ khỏi bảng [orders]
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'cash_amount')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [cash_amount];
    PRINT N'   - Đã xóa cột cash_amount';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'bank_amount')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [bank_amount];
    PRINT N'   - Đã xóa cột bank_amount';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'bank_transaction_code')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [bank_transaction_code];
    PRINT N'   - Đã xóa cột bank_transaction_code';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'refund_amount')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [refund_amount];
    PRINT N'   - Đã xóa cột refund_amount';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'refund_reason')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [refund_reason];
    PRINT N'   - Đã xóa cột refund_reason';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'refund_method')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [refund_method];
    PRINT N'   - Đã xóa cột refund_method';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'refunded_at')
BEGIN
    ALTER TABLE [orders] DROP COLUMN [refunded_at];
    PRINT N'   - Đã xóa cột refunded_at';
END
GO

-- =========================================================================================
-- BƯỚC 4: ĐẢM BẢO CÁC RÀNG BUỘC FK CỦA ORDERS VÀ PAYMENTS LÀ NO ACTION (KHÔNG CASCADE, KHÔNG NULL)
-- =========================================================================================
PRINT N'4. Cấu hình ràng buộc khóa ngoại: Không Delete Cascade, Không đổi sang NULL...';

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_payments_orders' AND delete_referential_action_desc = 'CASCADE')
BEGIN
    ALTER TABLE [payments] DROP CONSTRAINT [FK_payments_orders];
    ALTER TABLE [payments] ADD CONSTRAINT [FK_payments_orders] FOREIGN KEY ([order_id]) REFERENCES [orders] ([order_id]) ON DELETE NO ACTION;
    PRINT N'   - Đã chuyển FK_payments_orders thành ON DELETE NO ACTION';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_orders_employees_cashier_id' AND delete_referential_action_desc = 'CASCADE')
BEGIN
    ALTER TABLE [orders] DROP CONSTRAINT [FK_orders_employees_cashier_id];
    ALTER TABLE [orders] ADD CONSTRAINT [FK_orders_employees_cashier_id] FOREIGN KEY ([cashier_id]) REFERENCES [employees] ([employee_id]) ON DELETE NO ACTION;
    PRINT N'   - Đã chuyển FK_orders_employees_cashier_id thành ON DELETE NO ACTION';
END
GO

PRINT N'--> Migration hoàn tất thành công 100%!';
GO
