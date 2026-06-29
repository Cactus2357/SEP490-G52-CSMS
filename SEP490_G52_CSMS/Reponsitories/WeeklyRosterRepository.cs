using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Reponsitories
{
    public class WeeklyRosterRepository : IWeeklyRosterRepository
    {
        private readonly CSMSAppDbContext _context;

        public WeeklyRosterRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<WeeklyRosterGrid> rosters)
        {
            await _context.WeeklyRosterGrids.AddRangeAsync(rosters);
        }

        public async Task<bool> ExistsAsync(string branchId, DateTime assignmentDate, int shiftId)
        {
            return await _context.WeeklyRosterGrids.AnyAsync(x =>
                x.BranchId == branchId &&
                x.AssignmentDate == assignmentDate &&
                x.ShiftId == shiftId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
