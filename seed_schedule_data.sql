-- ============================================================
-- SEED DATA: Work Schedule & Attendance
-- ============================================================
USE CSMSSystem;
GO

DECLARE @today DATE = CAST(GETDATE() AS DATE);
DECLARE @empA INT = (SELECT employee_id FROM employees WHERE username = 'nguyenvana');
DECLARE @empB INT = (SELECT employee_id FROM employees WHERE username = 'nguyenhuub');
DECLARE @empC INT = (SELECT employee_id FROM employees WHERE username = 'tranthic');
DECLARE @empD INT = (SELECT employee_id FROM employees WHERE username = 'levanD');

-- Xóa dữ liệu cũ của tuần này (nếu có để chạy lại script nhiều lần)
DELETE FROM attendance_logs;
DELETE FROM weekly_roster_grids;

-- Hàm tính ngày Thứ 2 của tuần hiện tại
DECLARE @diff INT = (7 + (DATEPART(dw, @today) - 2)) % 7;
DECLARE @monday DATE = DATEADD(DAY, -@diff, @today);

-- Biến chạy vòng lặp từ T2 đến CN
DECLARE @currentDate DATE = @monday;
DECLARE @i INT = 0;

WHILE @i < 7
BEGIN
    -- Nguyễn Văn A: Làm Ca 1 tất cả các ngày
    INSERT INTO weekly_roster_grids (branch_id, assignment_date, shift_id, employee_id)
    VALUES ('CN001', @currentDate, 1, @empA);
    
    DECLARE @rosterA INT = SCOPE_IDENTITY();
    
    -- Giả lập điểm danh cho Nguyễn Văn A (T2-T4 Đã điểm danh, T5 vắng, T6-CN chưa)
    IF @i < 3
        INSERT INTO attendance_logs (roster_id, employee_id, check_in_time, check_in_status, check_out_time, check_out_status, overall_status)
        VALUES (@rosterA, @empA, DATEADD(HOUR, 6, CAST(@currentDate AS DATETIME)), 'OnTime', DATEADD(HOUR, 10, CAST(@currentDate AS DATETIME)), 'OnTime', 'Present');
    ELSE IF @i = 3
        INSERT INTO attendance_logs (roster_id, employee_id, check_in_status, check_out_status, overall_status)
        VALUES (@rosterA, @empA, 'Absent', 'Absent', 'Absent');

    -- Nguyễn Hữu B: Làm Ca 2 (thứ 2,4,6)
    IF @i % 2 = 0
    BEGIN
        INSERT INTO weekly_roster_grids (branch_id, assignment_date, shift_id, employee_id)
        VALUES ('CN001', @currentDate, 2, @empB);
    END

    -- Trần Thị C: Làm Ca 3 (thứ 3,5,7)
    IF @i % 2 = 1
    BEGIN
        INSERT INTO weekly_roster_grids (branch_id, assignment_date, shift_id, employee_id)
        VALUES ('CN001', @currentDate, 3, @empC);
    END

    SET @currentDate = DATEADD(DAY, 1, @currentDate);
    SET @i = @i + 1;
END
GO
