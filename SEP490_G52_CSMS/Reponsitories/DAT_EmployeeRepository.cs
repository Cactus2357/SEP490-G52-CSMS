using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Repositories
{
    public class DAT_EmployeeRepository : IDAT_EmployeeRepository
    {
        private readonly CSMSAppDbContext _context;

        public DAT_EmployeeRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Branch)
                .OrderByDescending(e => e.EmployeeId)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int employeeId)
        {
            return await _context.Employees
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<Employee?> GetByUsernameAsync(string username)
        {
            return await _context.Employees
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.Username == username);
        }

        public async Task<bool> ExistsUsernameAsync(string username)
        {
            return await _context.Employees.AnyAsync(e => e.Username == username);
        }

        public async Task<bool> ExistsEmailAsync(string email)
        {
            return await _context.Employees.AnyAsync(e => e.Email == email);
        }

        public async Task<bool> ExistsCitizenIdAsync(string citizenId)
        {
            return await _context.Employees.AnyAsync(e => e.CitizenId == citizenId);
        }

        public async Task<bool> ExistsPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Employees.AnyAsync(e => e.PhoneNumber == phoneNumber);
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
        {
            return await _context.Branches
                .Where(b => b.Status == "Active")
                .ToListAsync();
        }

        public async Task<bool> IsManagerOfBranchAsync(int managerId, string branchId)
        {
            return await _context.BranchManagers.AnyAsync(bm => bm.ManagerId == managerId && bm.BranchId == branchId);
        }

        public async Task<bool> ExistsCitizenIdExcludeSelfAsync(string citizenId, int excludeEmployeeId)
        {
            return await _context.Employees.AnyAsync(e => e.CitizenId == citizenId && e.EmployeeId != excludeEmployeeId);
        }

        public async Task<List<WeeklyRosterGrid>> GetRostersWithAttendanceAndHandoverAsync(int employeeId)
        {
            return await _context.WeeklyRosterGrids
                .Include(w => w.FixedShift)
                .Include(w => w.AttendanceLogs)
                .Where(w => w.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<bool> HasCashHandoverAsync(int employeeId, int shiftId, DateTime date)
        {
            return await _context.CashHandovers.AnyAsync(ch =>
                ch.OutgoingCashierId == employeeId &&
                ch.ShiftId == shiftId &&
                ch.HandoverDate.Date == date.Date);
        }
    }
}
