using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly CSMSAppDbContext _context;

        public LeaveRequestRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeaveApplication>> GetLeaveRequestsAsync(int employeeId, DateTime? fromDate, DateTime? toDate, string status)
        {
            var query = _context.LeaveApplications
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId);

            if (fromDate.HasValue)
                query = query.Where(x => x.SubmittedAt.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.SubmittedAt.Date <= toDate.Value.Date);

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                if (status == "Đã gửi")
                    query = query.Where(x => x.Status == "Pending");
                else if (status == "Đã duyệt")
                    query = query.Where(x => x.Status == "Approved");
                else if (status == "Từ chối")
                    query = query.Where(x => x.Status == "Rejected");
                else if (status == "Đã hủy")
                    query = query.Where(x => x.Status == "Canceled");
            }

            return await query.OrderByDescending(x => x.SubmittedAt).ToListAsync();
        }

        public async Task<LeaveApplication?> GetLeaveRequestByIdAsync(int applicationId)
        {
            return await _context.LeaveApplications.FirstOrDefaultAsync(x => x.ApplicationId == applicationId);
        }

        public async Task AddLeaveRequestAsync(LeaveApplication leaveApplication)
        {
            _context.LeaveApplications.Add(leaveApplication);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLeaveRequestAsync(LeaveApplication leaveApplication)
        {
            _context.LeaveApplications.Update(leaveApplication);
            await _context.SaveChangesAsync();
        }
    }
}
