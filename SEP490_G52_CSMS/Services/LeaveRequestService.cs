using System;
using System.Linq;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories;

namespace SEP490_G52_CSMS.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repository;

        public LeaveRequestService(ILeaveRequestRepository repository)
        {
            _repository = repository;
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
                    SubmittedAt = item.SubmittedAt.ToString("dd/MM/yyyy"),
                    Status = displayStatus
                });
            }

            return model;
        }

        public async Task<OperationResult> CreateLeaveRequestAsync(LeaveRequestCreateViewModel model)
        {
            // Business rule BR-01: Must submit at least 1 day in advance
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
                SubmittedAt = DateTime.Now
            };

            await _repository.AddLeaveRequestAsync(request);
            return OperationResult.Ok("Gửi đơn xin nghỉ phép thành công.");
        }

        public async Task<OperationResult> CancelLeaveRequestAsync(int applicationId, int currentEmployeeId)
        {
            var request = await _repository.GetLeaveRequestByIdAsync(applicationId);
            if (request == null || request.EmployeeId != currentEmployeeId)
            {
                return OperationResult.Fail("Không tìm thấy đơn xin nghỉ.");
            }

            if (request.Status != "Pending")
            {
                return OperationResult.Fail("Chỉ có thể hủy đơn khi trạng thái là 'Đã gửi'.");
            }

            request.Status = "Canceled";
            await _repository.UpdateLeaveRequestAsync(request);

            return OperationResult.Ok("Hủy đơn thành công.");
        }
    }
}
