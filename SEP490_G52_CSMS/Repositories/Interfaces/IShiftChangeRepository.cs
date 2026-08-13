using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IShiftChangeRepository
    {
        Task<List<ShiftChangeRequest>> GetRequestsAsync(int employeeId, DateTime? fromDate, DateTime? toDate, string status);
        Task<ShiftChangeRequest?> GetRequestByIdAsync(int requestId, int employeeId);
        Task AddRequestAsync(ShiftChangeRequest request);
        Task UpdateRequestAsync(ShiftChangeRequest request);
    }
}
