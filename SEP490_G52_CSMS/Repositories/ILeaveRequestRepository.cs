using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Repositories
{
    public interface ILeaveRequestRepository
    {
        Task<List<LeaveApplication>> GetLeaveRequestsAsync(int employeeId, DateTime? fromDate, DateTime? toDate, string status);
        Task<LeaveApplication?> GetLeaveRequestByIdAsync(int applicationId);
        Task AddLeaveRequestAsync(LeaveApplication leaveApplication);
        Task UpdateLeaveRequestAsync(LeaveApplication leaveApplication);
    }
}
