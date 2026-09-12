-- =========================================================================================
-- MIGRATION SCRIPT: TẠO BẢNG VOUCHERS & BỔ SUNG KHÓA LIÊN KẾT TRÊN ORDERS
-- Project: SEP490_G52_CSMS
-- Database: CSMSSystem
-- Date: 2026-09-09
-- Description:
--   1. Tạo bảng riêng [vouchers] để quản lý mã giảm giá theo chi nhánh:
--      - voucher_id (PK), voucher_code, branch_id, discount_percent, quantity, used_count
--      - start_date, end_date, description, is_active, created_by, created_at
--   2. Bổ sung các cột [voucher_id], [voucher_code] vào bảng [orders]
--   3. Cấu hình khóa ngoại ON DELETE NO ACTION (Không delete cascade, không gán NULL)
-- =========================================================================================

USE [CSMSSystem];
GO

SET NOCOUNT ON;
PRINT N'--> Bắt đầu quá trình Migration: Tạo bảng vouchers...';
GO

-- =========================================================================================
-- BƯỚC 1: TẠO BẢNG [vouchers] NẾU CHƯA TỒN TẠI
-- =========================================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'vouchers')
BEGIN
    PRINT N'1. Đang tạo bảng [vouchers]...';
    
    CREATE TABLE [vouchers] (
        [voucher_id] INT IDENTITY(1,1) NOT NULL,
        [voucher_code] NVARCHAR(50) NOT NULL,                                                      -- Mã voucher (VD: SUMMER20, CSMS10)
        [branch_id] NVARCHAR(20) NULL,                                                             -- Chi nhánh phát hành (NULL: toàn hệ thống)
        [discount_percent] DECIMAL(5, 2) NOT NULL,                                                  -- Tỷ lệ giảm giá % (1 - 100)
        [quantity] INT NOT NULL,                                                                    -- Tổng số lượng voucher phát hành
        [used_count] INT NOT NULL CONSTRAINT [DF_vouchers_used_count] DEFAULT 0,                    -- Số lượng đã sử dụng
        [start_date] DATETIME2 NOT NULL,                                                            -- Thời gian bắt đầu áp dụng
        [end_date] DATETIME2 NOT NULL,                                                              -- Thời gian kết thúc áp dụng
        [description] NVARCHAR(500) NULL,                                                           -- Thông tin thêm / mô tả khuyến mãi
        [is_active] BIT NOT NULL CONSTRAINT [DF_vouchers_is_active] DEFAULT 1,                      -- Trạng thái: 1 = Đang hoạt động, 0 = Tạm dừng
        [created_by] INT NULL,                                                                      -- Người tạo (EmployeeId)
        [created_at] DATETIME2 NOT NULL CONSTRAINT [DF_vouchers_created_at] DEFAULT GETDATE(),      -- Thời điểm tạo
        
        CONSTRAINT [PK_vouchers] PRIMARY KEY CLUSTERED ([voucher_id] ASC),
        CONSTRAINT [FK_vouchers_branches] FOREIGN KEY ([branch_id]) REFERENCES [branches] ([branch_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_vouchers_employees] FOREIGN KEY ([created_by]) REFERENCES [employees] ([employee_id]) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX [IX_vouchers_voucher_code] ON [vouchers] ([voucher_code] ASC);
    CREATE NONCLUSTERED INDEX [IX_vouchers_branch_id] ON [vouchers] ([branch_id] ASC);
    CREATE NONCLUSTERED INDEX [IX_vouchers_dates] ON [vouchers] ([start_date] ASC, [end_date] ASC);

    PRINT N'   Đã tạo thành công bảng [vouchers] và các chỉ mục liên quan.';
END
ELSE
BEGIN
    PRINT N'1. Bảng [vouchers] đã tồn tại sẵn, bỏ qua bước tạo bảng.';
END
GO

-- =========================================================================================
-- BƯỚC 2: BỔ SUNG CỘT [voucher_id], [voucher_code] VÀO BẢNG [orders]
-- =========================================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'voucher_id')
BEGIN
    PRINT N'2. Đang thêm cột [voucher_id] vào bảng [orders]...';
    ALTER TABLE [orders] ADD [voucher_id] INT NULL;

    ALTER TABLE [orders] ADD CONSTRAINT [FK_orders_vouchers] 
        FOREIGN KEY ([voucher_id]) REFERENCES [vouchers] ([voucher_id]) ON DELETE NO ACTION;
    PRINT N'   Đã thêm cột [voucher_id] và khóa ngoại FK_orders_vouchers.';
END
ELSE
BEGIN
    PRINT N'2. Cột [voucher_id] đã tồn tại trên bảng [orders].';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'voucher_code')
BEGIN
    PRINT N'3. Đang thêm cột [voucher_code] vào bảng [orders]...';
    ALTER TABLE [orders] ADD [voucher_code] NVARCHAR(50) NULL;
    PRINT N'   Đã thêm cột [voucher_code] vào bảng [orders].';
END
ELSE
BEGIN
    PRINT N'3. Cột [voucher_code] đã tồn tại trên bảng [orders].';
END
GO

-- =========================================================================================
-- BƯỚC 3: TẠO SAMPLE VOUCHERS CHO MỤC ĐÍCH KIỂM THỬ (NẾU CHƯA CÓ)
-- =========================================================================================
DECLARE @DefaultBranchId NVARCHAR(20);
SELECT TOP 1 @DefaultBranchId = branch_id FROM branches;

IF NOT EXISTS (SELECT 1 FROM [vouchers] WHERE [voucher_code] = 'WELCOME10')
BEGIN
    PRINT N'4. Thêm voucher mẫu [WELCOME10] (-10%)...';
    INSERT INTO [vouchers] ([voucher_code], [branch_id], [discount_percent], [quantity], [used_count], [start_date], [end_date], [description], [is_active], [created_at])
    VALUES ('WELCOME10', @DefaultBranchId, 10.00, 100, 0, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, 30, GETDATE()), N'Giảm 10% cho khách hàng mới', 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM [vouchers] WHERE [voucher_code] = 'SUMMER20')
BEGIN
    PRINT N'5. Thêm voucher mẫu [SUMMER20] (-20%)...';
    INSERT INTO [vouchers] ([voucher_code], [branch_id], [discount_percent], [quantity], [used_count], [start_date], [end_date], [description], [is_active], [created_at])
    VALUES ('SUMMER20', @DefaultBranchId, 20.00, 50, 0, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, 15, GETDATE()), N'Khuyến mãi mùa hè giảm 20% toàn bộ đơn hàng', 1, GETDATE());
END
GO

PRINT N'--> Migration hoàn tất thành công!';
GO
