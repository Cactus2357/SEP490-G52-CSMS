using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Repositories.Interfaces;

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
                if (status == "Đã gửi" || status == "Chờ duyệt")
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

        // --- Manager UC47 & UC48 Methods ---

        public async Task<List<LeaveApplication>> GetManagerLeaveRequestsAsync(string branchId, string? searchName, DateTime? fromDate, DateTime? toDate, string? status)
        {
            var query = _context.LeaveApplications
                .Include(l => l.Employee)
                .AsNoTracking()
                .Where(l => l.Employee != null && l.Employee.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var term = searchName.Trim();
                if (_context.Database.IsSqlServer())
                {
                    query = query.Where(l => (l.Employee != null && (
                        EF.Functions.Collate(l.Employee.FullName, "SQL_Latin1_General_CP1_CI_AI").Contains(term) ||
                        EF.Functions.Collate(l.Employee.Username, "SQL_Latin1_General_CP1_CI_AI").Contains(term)
                    )) || (l.Reason != null && EF.Functions.Collate(l.Reason, "SQL_Latin1_General_CP1_CI_AI").Contains(term))
                       || (l.LeaveShifts != null && EF.Functions.Collate(l.LeaveShifts, "SQL_Latin1_General_CP1_CI_AI").Contains(term)));
                }
                else
                {
                    var lowerTerm = term.ToLower();
                    query = query.Where(l => (l.Employee != null && (
                        (l.Employee.FullName != null && l.Employee.FullName.ToLower().Contains(lowerTerm)) ||
                        (l.Employee.Username != null && l.Employee.Username.ToLower().Contains(lowerTerm))
                    )) || (l.Reason != null && l.Reason.ToLower().Contains(lowerTerm))
                       || (l.LeaveShifts != null && l.LeaveShifts.ToLower().Contains(lowerTerm)));
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.StartDate.Date >= fromDate.Value.Date || l.SubmittedAt.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.StartDate.Date <= toDate.Value.Date || l.SubmittedAt.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả")
            {
                if (status == "Chờ duyệt" || status == "Pending" || status == "Đã gửi")
                    query = query.Where(l => l.Status == "Pending");
                else if (status == "Đã duyệt" || status == "Approved")
                    query = query.Where(l => l.Status == "Approved");
                else if (status == "Từ chối" || status == "Rejected")
                    query = query.Where(l => l.Status == "Rejected");
                else if (status == "Đã hủy" || status == "Canceled")
                    query = query.Where(l => l.Status == "Canceled");
            }

            return await query.OrderByDescending(l => l.SubmittedAt).ToListAsync();
        }

        public async Task<LeaveApplication?> GetLeaveRequestDetailByIdAsync(int applicationId)
        {
            return await _context.LeaveApplications
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.ApplicationId == applicationId);
        }

        public async Task<int> GetApprovedLeavesCountInMonthAsync(int employeeId, int year, int month)
        {
            return await _context.LeaveApplications
                .Where(l => l.EmployeeId == employeeId && l.Status == "Approved" && l.StartDate.Year == year && l.StartDate.Month == month)
                .CountAsync();
        }

        public async Task<int> GetPendingLeavesCountInMonthAsync(int employeeId, int year, int month)
        {
            return await _context.LeaveApplications
                .Where(l => l.EmployeeId == employeeId && l.Status == "Pending" && l.StartDate.Year == year && l.StartDate.Month == month)
                .CountAsync();
        }

        public async Task<int> GetAssignedShiftsCountInMonthAsync(int employeeId, int year, int month)
        {
            return await _context.WeeklyRosterGrids
                .Where(w => w.EmployeeId == employeeId && w.AssignmentDate.Year == year && w.AssignmentDate.Month == month)
                .CountAsync();
        }

        public async Task<int> GetOtherCoWorkersAssignedCountAsync(string branchId, DateTime date, int excludeEmployeeId)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.Employee)
                .Where(w => w.Employee != null && w.Employee.BranchId == branchId && w.EmployeeId != excludeEmployeeId && w.AssignmentDate.Date == date.Date)
                .CountAsync();
        }

        public async Task UnassignEmployeeRosterAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var rosters = await _context.WeeklyRosterGrids
                .Where(w => w.EmployeeId == employeeId && w.AssignmentDate.Date >= startDate.Date && w.AssignmentDate.Date <= endDate.Date)
                .ToListAsync();

            if (rosters.Any())
            {
                _context.WeeklyRosterGrids.RemoveRange(rosters);
                await _context.SaveChangesAsync();
            }
        }
    }
}
