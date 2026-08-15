using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class ShiftChangeService : IShiftChangeService
    {
        private readonly IShiftChangeRepository _repository;

        public ShiftChangeService(IShiftChangeRepository repository)
        {
            _repository = repository;
        }

        public async Task<ShiftChangeListViewModel> GetShiftChangeListAsync(int employeeId, string employeeName, string roleName, DateTime? fromDate, DateTime? toDate, string status)
        {
            var requests = await _repository.GetRequestsAsync(employeeId, fromDate, toDate, status);

            var model = new ShiftChangeListViewModel
            {
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                RoleName = roleName,
                FromDate = fromDate,
                ToDate = toDate,
                Status = string.IsNullOrEmpty(status) ? "Tất cả" : status,
                Items = requests.Select(r => new ShiftChangeItemViewModel
                {
                    RequestId = r.RequestId,
                    SubmittedAt = r.SubmittedAt.ToString("dd/MM/yyyy"),
                    Status = MapStatusToVietnamese(r.Status),
                    Aspiration = r.Aspiration ?? "",
                    Reason = r.Reason ?? "",
                    CanCancel = r.Status == "Submitted" || r.Status == "Pending" // Chỉ cho phép hủy khi đang "Đã gửi"
                }).ToList()
            };

            return model;
        }

        public async Task<OperationResult> CreateShiftChangeAsync(ShiftChangeCreateViewModel model)
        {
            try
            {
                var request = new ShiftChangeRequest
                {
                    RequestingEmployeeId = model.EmployeeId,
                    Aspiration = model.Aspiration,
                    Reason = model.Reason,
                    SubmittedAt = DateTime.Now,
                    Status = "Submitted"
                };

                await _repository.AddRequestAsync(request);
                return OperationResult.Ok("Gửi đơn đổi ca thành công.");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        public async Task<OperationResult> CancelShiftChangeAsync(int requestId, int currentEmployeeId)
        {
            try
            {
                var request = await _repository.GetRequestByIdAsync(requestId, currentEmployeeId);
                if (request == null)
                {
                    return OperationResult.Fail("Không tìm thấy đơn hoặc bạn không có quyền hủy đơn này.");
                }

                if (request.Status != "Submitted")
                {
                    return OperationResult.Fail("Chỉ có thể hủy đơn đang ở trạng thái 'Đã gửi'.");
                }

                request.Status = "Canceled";
                await _repository.UpdateRequestAsync(request);

                return OperationResult.Ok("Hủy đơn thành công.");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        private string MapStatusToVietnamese(string status)
        {
            return status switch
            {
                "Submitted" => "Đã gửi",
                "Approved" => "Đã duyệt",
                "Rejected" => "Từ chối",
                "Canceled" => "Đã hủy",
                _ => status
            };
        }
    }
}
