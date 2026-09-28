using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly INotificationService _notificationService;

        public LeaveRequestService(ILeaveRequestRepository repository, IEmployeeRepository employeeRepository, INotificationService notificationService)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _notificationService = notificationService;
        }

        public async Task<LeaveRequestListViewModel> GetLeaveRequestListAsync(int employeeId, string employeeName, DateTime? fromDate, DateTime? toDate, string status)
        {
            var data = await _repository.GetLeaveRequestsAsync(employeeId, fromDate, toDate, status);

            var model = new LeaveRequestListViewModel
            {
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                TotalCount = data.Count
            };

            foreach (var item in data)
            {
                var displayStatus = item.Status switch
                {
                    "Pending" => "Đã gửi",
                    "Approved" => "Đã duyệt",
                    "Rejected" => "Từ chối",
                    "Canceled" => "Đã hủy",
                    _ => item.Status
                };

                model.Items.Add(new LeaveRequestItemViewModel
                {
                    ApplicationId = item.ApplicationId,
                    SubmittedAt = item.SubmittedAt.ToVietnamTimeString("HH:mm - dd/MM/yyyy"),
                    Status = displayStatus,
                    StartDate = item.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = item.EndDate.ToString("dd/MM/yyyy"),
                    LeaveShifts = string.IsNullOrWhiteSpace(item.LeaveShifts) ? "Tất cả ca" : item.LeaveShifts,
                    Reason = item.Reason ?? ""
                });
            }

            return model;
        }

        public async Task<OperationResult> CreateLeaveRequestAsync(LeaveRequestCreateViewModel model)
        {
            if (model.StartDate.Date <= DateTime.Today)
            {
                return OperationResult.Fail("Xin nghỉ phải nộp trước ít nhất 1 ngày. Trường hợp khẩn cấp vui lòng liên hệ quản lý.");
            }

            if (model.EndDate < model.StartDate)
            {
                return OperationResult.Fail("Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");
            }

            var request = new LeaveApplication
            {
                EmployeeId = model.EmployeeId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Reason = model.Reason,
                LeaveShifts = model.LeaveShifts,
                Status = "Pending",
                SubmittedAt = DateTime.UtcNow
            };

            await _repository.AddLeaveRequestAsync(request);

            var emp = await _employeeRepository.GetByIdAsync(model.EmployeeId);
            // Raise notification event for Branch Manager
            await _notificationService.SendAsync(new NotificationEvent(
                Title: "Đơn xin nghỉ phép mới",
                Message: $"Nhân viên {model.EmployeeName} đã nộp đơn xin nghỉ phép ngày {model.StartDate:dd/MM/yyyy}.",
                RecipientRole: "BranchManager",
                ResourceUrl: "/LeaveRequest/ManagerIndex",
                BranchId: emp?.BranchId
            ));

            return OperationResult.Ok("Gửi đơn xin nghỉ phép thành công.");
        }

        public async Task<OperationResult> CancelLeaveRequestAsync(int applicationId, int currentEmployeeId)
        {
            var request = await _repository.GetLeaveRequestByIdAsync(applicationId);
            if (request == null || request.EmployeeId != currentEmployeeId)
            {
                return OperationResult.Fail("Không tìm thấy đơn xin nghỉ.");
            }

            if (request.Status != "Pending" && request.Status != "Submitted" && request.Status != "Chờ duyệt" && request.Status != "Đã gửi")
            {
                return OperationResult.Fail("Chỉ có thể hủy đơn khi trạng thái là 'Đã gửi'.");
            }

            request.Status = "Canceled";
            await _repository.UpdateLeaveRequestAsync(request);

            return OperationResult.Ok("Hủy đơn thành công.");
        }

        // --- Branch Manager Methods (UC47 & UC48) ---

        public async Task<ManagerLeaveRequestListViewModel> GetManagerLeaveRequestsAsync(string branchId, string branchName, string? searchName, DateTime? fromDate, DateTime? toDate, string status)
        {
            // BR03: Default date inputs span current month
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var now = DateTime.Today;
                fromDate = new DateTime(now.Year, now.Month, 1);
                toDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            }

            // BR02: Range boundary check
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                // Swap or normalize if invalid range
                var temp = fromDate;
                fromDate = toDate;
                toDate = temp;
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                status = "Tất cả";
            }

            var data = await _repository.GetManagerLeaveRequestsAsync(branchId, searchName, fromDate, toDate, status);

            var model = new ManagerLeaveRequestListViewModel
            {
                BranchId = branchId,
                BranchName = branchName,
                SearchName = searchName ?? "",
                FromDate = fromDate,
                ToDate = toDate,
                Status = status
            };

            foreach (var item in data)
            {
                var empName = item.Employee?.FullName ?? $"NV#{item.EmployeeId}";
                var initials = GetInitials(empName);
                var shiftName = string.IsNullOrWhiteSpace(item.LeaveShifts) ? "Ca sáng" : item.LeaveShifts;
                var reason = string.IsNullOrWhiteSpace(item.Reason) ? "Nghỉ cá nhân" : item.Reason;
                var dateStr = item.StartDate.ToString("dd/MM");
                var fullDateStr = item.StartDate.ToString("dd/MM/yyyy");

                var displayStatus = item.Status switch
                {
                    "Pending" => "Chờ duyệt",
                    "Approved" => "Đã duyệt",
                    "Rejected" => "Từ chối",
                    "Canceled" => "Đã hủy",
                    _ => item.Status
                };

                var badgeClass = displayStatus switch
                {
                    "Chờ duyệt" => "bg-warning text-dark border border-warning-subtle",
                    "Đã duyệt" => "bg-success text-white border border-success-subtle",
                    "Từ chối" => "bg-danger text-white border border-danger-subtle",
                    _ => "bg-secondary text-white border border-secondary-subtle"
                };

                var summary = $"{empName} - {dateStr} - {shiftName} - {reason}";

                model.Items.Add(new ManagerLeaveRequestItemViewModel
                {
                    ApplicationId = item.ApplicationId,
                    EmployeeId = item.EmployeeId,
                    EmployeeFullName = empName,
                    EmployeeInitials = initials,
                    ShiftName = shiftName,
                    RequestDateStr = dateStr,
                    FullDateStr = fullDateStr,
                    Reason = reason,
                    Status = displayStatus,
                    StatusBadgeClass = badgeClass,
                    SummaryText = summary
                });
            }

            return model;
        }

        public async Task<LeaveRequestDetailViewModel?> GetLeaveRequestDetailAsync(int applicationId, string branchId)
        {
            var request = await _repository.GetLeaveRequestDetailByIdAsync(applicationId);
            if (request == null || request.Employee == null || request.Employee.BranchId != branchId)
            {
                return null;
            }

            var emp = request.Employee;
            var initials = GetInitials(emp.FullName);
            var shiftDetails = string.IsNullOrWhiteSpace(request.LeaveShifts) ? "Ca sáng - 07:00-14:00" : request.LeaveShifts;
            var dateStr = request.StartDate.ToString("dd/MM/yyyy");
            var submittedStr = request.SubmittedAt.ToVietnamTimeString("HH:mm - dd/MM/yyyy");

            var displayStatus = request.Status switch
            {
                "Pending" => "Chờ duyệt",
                "Approved" => "Đã duyệt",
                "Rejected" => "Từ chối",
                "Canceled" => "Đã hủy",
                _ => request.Status
            };

            var badgeClass = displayStatus switch
            {
                "Chờ duyệt" => "bg-warning text-dark border border-warning-subtle",
                "Đã duyệt" => "bg-success text-white border border-success-subtle",
                "Từ chối" => "bg-danger text-white border border-danger-subtle",
                _ => "bg-secondary text-white border border-secondary-subtle"
            };

            // Monthly stats
            var reqYear = request.StartDate.Year;
            var reqMonth = request.StartDate.Month;
            int takenMonth = await _repository.GetApprovedLeavesCountInMonthAsync(emp.EmployeeId, reqYear, reqMonth);
            int pendingMonth = await _repository.GetPendingLeavesCountInMonthAsync(emp.EmployeeId, reqYear, reqMonth);
            int totalShiftsMonth = await _repository.GetAssignedShiftsCountInMonthAsync(emp.EmployeeId, reqYear, reqMonth);
            if (totalShiftsMonth == 0) totalShiftsMonth = 22; // Fallback demo metric

            // Check shift substitute warning (Item 10 Banner)
            int coWorkers = await _repository.GetOtherCoWorkersAssignedCountAsync(branchId, request.StartDate, emp.EmployeeId);
            bool hasWarning = coWorkers <= 0;
            string warningMsg = hasWarning ? $"Ca sáng {request.StartDate:dd/MM} chưa có người thay thế." : string.Empty;

            return new LeaveRequestDetailViewModel
            {
                ApplicationId = request.ApplicationId,
                EmployeeId = emp.EmployeeId,
                EmployeeFullName = emp.FullName ?? emp.Username ?? $"NV#{emp.EmployeeId}",
                EmployeeUsername = string.IsNullOrWhiteSpace(emp.Username) ? $"@nv{emp.EmployeeId}" : (emp.Username.StartsWith("@") ? emp.Username : $"@{emp.Username}"),
                EmployeeRole = emp.Role ?? "Nhân viên",
                EmployeeInitials = initials,
                Status = displayStatus,
                StatusBadgeClass = badgeClass,
                RequestDateStr = dateStr,
                ShiftDetails = shiftDetails,
                Reason = request.Reason ?? "Nghỉ cá nhân",
                SubmittedAtStr = submittedStr,
                CurrentMonth = reqMonth,
                TakenShiftsMonth = takenMonth,
                PendingShiftsMonth = pendingMonth,
                TotalShiftsMonth = totalShiftsMonth,
                HasShiftWarning = hasWarning,
                ShiftWarningMessage = warningMsg
            };
        }

        public async Task<OperationResult> ApproveLeaveRequestAsync(int applicationId, string branchId, int managerId, string managerRole)
        {
            var request = await _repository.GetLeaveRequestDetailByIdAsync(applicationId);
            if (request == null || request.Employee == null || request.Employee.BranchId != branchId)
            {
                return OperationResult.Fail("Không tìm thấy đơn xin nghỉ phép hoặc không có quyền thao tác.");
            }

            // BR02: Action choices are only interactable if status is Pending / Chờ duyệt
            if (request.Status != "Pending")
            {
                return OperationResult.Fail("Đơn hàng này đã được xử lý trước đó.");
            }

            request.Status = "Approved";
            request.ApprovedBranchId = branchId;
            request.ApprovedManagerId = managerId;

            await _repository.UpdateLeaveRequestAsync(request);

            // BR03: Automatically unassign the employee from that shift in Work Schedule
            await _repository.UnassignEmployeeRosterAsync(request.EmployeeId, request.StartDate, request.EndDate);

            // Raise notification event for Employee
            await _notificationService.SendAsync(new NotificationEvent(
                Title: "Đơn xin nghỉ phép đã được duyệt",
                Message: $"Đơn xin nghỉ phép ngày {request.StartDate:dd/MM/yyyy} của bạn đã được phê duyệt.",
                RecipientUserId: request.EmployeeId,
                ResourceUrl: "/LeaveRequest",
                BranchId: request.Employee?.BranchId ?? branchId
            ));

            return OperationResult.Ok("Phê duyệt đơn xin nghỉ phép thành công.");
        }

        public async Task<OperationResult> RejectLeaveRequestAsync(int applicationId, string branchId, int managerId, string managerRole, string? reason)
        {
            var request = await _repository.GetLeaveRequestDetailByIdAsync(applicationId);
            if (request == null || request.Employee == null || request.Employee.BranchId != branchId)
            {
                return OperationResult.Fail("Không tìm thấy đơn xin nghỉ phép hoặc không có quyền thao tác.");
            }

            if (request.Status != "Pending")
            {
                return OperationResult.Fail("Đơn hàng này đã được xử lý trước đó.");
            }

            request.Status = "Rejected";
            request.ApprovedBranchId = branchId;
            request.ApprovedManagerId = managerId;

            await _repository.UpdateLeaveRequestAsync(request);

            // Raise notification event for Employee
            await _notificationService.SendAsync(new NotificationEvent(
                Title: "Đơn xin nghỉ phép bị từ chối",
                Message: $"Đơn xin nghỉ phép ngày {request.StartDate:dd/MM/yyyy} của bạn đã bị từ chối.",
                RecipientUserId: request.EmployeeId,
                ResourceUrl: "/LeaveRequest",
                BranchId: request.Employee?.BranchId ?? branchId
            ));

            return OperationResult.Ok("Đã từ chối đơn xin nghỉ phép.");
        }

        private static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "NV";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            if (parts.Length == 2) return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            // For 3+ words (e.g. Nguyễn Minh Anh -> NA, Trần Quốc Bảo -> QB or TB)
            return (parts[parts.Length - 2][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }
    }
}
