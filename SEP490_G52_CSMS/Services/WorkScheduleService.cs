using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly IWorkScheduleRepository _repository;

        public WorkScheduleService(IWorkScheduleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SelectEmployeeScheduleViewModel>> GetEmployeesForSelectionAsync(string branchId)
        {
            var employees = await _repository.GetEmployeesByBranchAsync(branchId);
            return employees.Select(e => new SelectEmployeeScheduleViewModel
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = e.FullName ?? e.Username ?? "Unknown",
                Username = e.Username ?? "Unknown",
                Role = e.Role ?? "Employee"
            }).ToList();
        }

        public async Task<WorkScheduleViewModel?> GetEmployeeScheduleAsync(int employeeId, DateTime dateInWeek)
        {
            var employee = await _repository.GetEmployeeByIdAsync(employeeId);
            if (employee == null) return null;

            // Xác định ngày Thứ 2 (StartOfWeek) và Chủ Nhật (EndOfWeek)
            int diff = (7 + (dateInWeek.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = dateInWeek.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6).Date;

            var shifts = await _repository.GetFixedShiftsAsync();
            var rosters = await _repository.GetWeeklyRostersAsync(employeeId, startOfWeek, endOfWeek);

            var model = new WorkScheduleViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.FullName ?? employee.Username ?? "Unknown",
                BranchId = employee.BranchId ?? "CN001",
                WeekStartDate = startOfWeek,
                DateRangeText = $"{startOfWeek:dd/MM/yyyy} - {endOfWeek:dd/MM/yyyy}",
                DayHeaders = new List<string> { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" }
            };

            foreach (var shift in shifts)
            {
                var row = new WorkScheduleRowViewModel
                {
                    ShiftId = shift.ShiftId,
                    ShiftName = shift.ShiftName ?? "",
                    ShiftTimeRange = $"{shift.StartTime:hh\\:mm} - {shift.EndTime:hh\\:mm}"
                };

                for (int i = 0; i < 7; i++)
                {
                    var currentDate = startOfWeek.AddDays(i);
                    var roster = rosters.FirstOrDefault(r => r.AssignmentDate.Date == currentDate.Date && r.ShiftId == shift.ShiftId);

                    var cell = new WorkScheduleCellViewModel
                    {
                        Date = currentDate,
                        HasShift = roster != null
                    };

                    if (roster != null)
                    {
                        var log = roster.AttendanceLogs.FirstOrDefault();
                        if (log != null)
                        {
                            if (log.OverallStatus == "Present")
                            {
                                cell.AttendanceStatus = "Đã điểm danh";
                            }
                            else if (log.OverallStatus == "Absent")
                            {
                                cell.AttendanceStatus = "Vắng mặt";
                            }
                            else
                            {
                                cell.AttendanceStatus = "Chưa điểm danh";
                            }
                        }
                        else
                        {
                            // Chưa có log điểm danh
                            if (currentDate.Date > DateTime.Today)
                            {
                                cell.AttendanceStatus = "Chưa điểm danh"; // Tương lai
                            }
                            else if (currentDate.Date == DateTime.Today && shift.StartTime > DateTime.Now.TimeOfDay)
                            {
                                cell.AttendanceStatus = "Chưa điểm danh"; // Chưa tới giờ
                            }
                            else if (currentDate.Date < DateTime.Today)
                            {
                                cell.AttendanceStatus = "Vắng mặt"; // Đã qua nhưng không có log
                            }
                            else
                            {
                                cell.AttendanceStatus = "Chưa điểm danh";
                            }
                        }
                    }

                    row.Cells.Add(cell);
                }
                model.Rows.Add(row);
            }

            return model;
        }
    }
}
