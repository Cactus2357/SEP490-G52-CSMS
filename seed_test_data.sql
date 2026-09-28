-- ============================================================
-- SEED DATA cho module Ket Tien & Giao Ca (CSMS)
-- ============================================================
USE CSMSSystem;
GO

-- 1. BRANCH
IF NOT EXISTS (SELECT 1 FROM branches WHERE branch_id = 'CN001')
BEGIN
    INSERT INTO branches (branch_id, branch_name, address, opening_time, closing_time, status, phone_number)
    VALUES ('CN001', N'Chi nhanh Quan 1', N'123 Nguyen Hue, Q.1, TP.HCM',
            '06:00:00', '22:00:00', 'Active', '0901234567');
    PRINT 'OK: Inserted branch CN001';
END
ELSE
    PRINT 'SKIP: Branch CN001 exists';
GO

-- 2. FIXED SHIFTS
IF NOT EXISTS (SELECT 1 FROM fixed_shifts WHERE shift_id = 1)
BEGIN
    SET IDENTITY_INSERT fixed_shifts ON;
    INSERT INTO fixed_shifts (shift_id, shift_name, start_time, end_time) VALUES
        (1, N'Ca 1', '06:00:00', '10:00:00'),
        (2, N'Ca 2', '10:00:00', '14:00:00'),
        (3, N'Ca 3', '14:00:00', '18:00:00'),
        (4, N'Ca 4', '18:00:00', '22:00:00');
    SET IDENTITY_INSERT fixed_shifts OFF;
    PRINT 'OK: Inserted 4 fixed shifts';
END
ELSE
    PRINT 'SKIP: Fixed shifts exist';
GO

-- 3. EMPLOYEES — Thu ngan A
IF NOT EXISTS (SELECT 1 FROM employees WHERE username = 'nguyenvana')
BEGIN
    INSERT INTO employees (username, password, full_name, date_of_birth, address,
                           phone_number, email, role, employment_type, citizen_id,
                           status, failed_login_attempts, branch_id)
    VALUES ('nguyenvana', '123456', N'Nguyen Van A', '1995-01-15',
            N'45 Le Loi, Q.1', '0912345678', 'nguyenvana@csms.vn',
            'Cashier', 'Full-time', '079095001234',
            'Active', 0, 'CN001');
    PRINT 'OK: Inserted employee: Nguyen Van A';
END
ELSE
    PRINT 'SKIP: Employee nguyenvana exists';
GO

-- Thu ngan B
IF NOT EXISTS (SELECT 1 FROM employees WHERE username = 'nguyenhuub')
BEGIN
    INSERT INTO employees (username, password, full_name, date_of_birth, address,
                           phone_number, email, role, employment_type, citizen_id,
                           status, failed_login_attempts, branch_id)
    VALUES ('nguyenhuub', '123456', N'Nguyen Huu B', '1997-06-20',
            N'78 Pasteur, Q.3', '0923456789', 'nguyenhuub@csms.vn',
            'Cashier', 'Full-time', '079097005678',
            'Active', 0, 'CN001');
    PRINT 'OK: Inserted employee: Nguyen Huu B';
END
ELSE
    PRINT 'SKIP: Employee nguyenhuub exists';
GO

-- 4. WEEKLY ROSTER GRID — Lich truc HOM NAY
DECLARE @today  DATE = CAST(GETDATE() AS DATE);
DECLARE @empA   INT  = (SELECT employee_id FROM employees WHERE username = 'nguyenvana');
DECLARE @empB   INT  = (SELECT employee_id FROM employees WHERE username = 'nguyenhuub');

IF @empA IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM weekly_roster_grids WHERE employee_id = @empA AND CAST(assignment_date AS DATE) = @today)
BEGIN
    INSERT INTO weekly_roster_grids (branch_id, assignment_date, shift_id, employee_id)
    VALUES ('CN001', @today, 1, @empA);
    PRINT 'OK: Roster Nguyen Van A - Ca 1 - today';
END

IF @empB IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM weekly_roster_grids WHERE employee_id = @empB AND CAST(assignment_date AS DATE) = @today)
BEGIN
    INSERT INTO weekly_roster_grids (branch_id, assignment_date, shift_id, employee_id)
    VALUES ('CN001', @today, 2, @empB);
    PRINT 'OK: Roster Nguyen Huu B - Ca 2 - today';
END
GO

-- 5. CASH HANDOVER — 1 ca da dong HOM QUA (lam "thong tin ca truoc")
DECLARE @yesterday DATE = CAST(DATEADD(DAY, -1, GETDATE()) AS DATE);
DECLARE @empA2 INT = (SELECT employee_id FROM employees WHERE username = 'nguyenvana');
DECLARE @empB2 INT = (SELECT employee_id FROM employees WHERE username = 'nguyenhuub');

IF @empA2 IS NOT NULL AND @empB2 IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM cash_handovers
       WHERE branch_id = 'CN001' AND CAST(handover_date AS DATE) = @yesterday AND status = 'Closed')
BEGIN
    INSERT INTO cash_handovers (
        branch_id, handover_date, shift_id,
        outgoing_cashier_id, incoming_cashier_id,
        initial_cash, machine_cash_revenue, bank_transfer_revenue,
        theoretical_cash, actual_cash,
        is_password_confirmed, status, notes,
        opened_at, closed_at
    )
    VALUES (
        'CN001', @yesterday, 4,
        @empB2, @empA2,
        3000000, 5000000, 1500000,
        8000000, 8000000,
        1, 'Closed', NULL,
        DATEADD(HOUR, 18, CAST(@yesterday AS DATETIME)),
        DATEADD(HOUR, 22, CAST(@yesterday AS DATETIME))
    );
    PRINT 'OK: Inserted closed handover yesterday (Ca 4)';
END
ELSE
    PRINT 'SKIP or ERROR: Check if employees inserted correctly';
GO

-- 6. KET QUA
SELECT 'employees (CN001)' AS tbl, COUNT(*) AS cnt FROM employees WHERE branch_id = 'CN001'
UNION ALL
SELECT 'fixed_shifts', COUNT(*) FROM fixed_shifts
UNION ALL
SELECT 'roster today', COUNT(*) FROM weekly_roster_grids WHERE CAST(assignment_date AS DATE) = CAST(GETDATE() AS DATE)
UNION ALL
SELECT 'cash_handovers (CN001)', COUNT(*) FROM cash_handovers WHERE branch_id = 'CN001';

SELECT employee_id, username, full_name, role FROM employees WHERE branch_id = 'CN001';
GO
