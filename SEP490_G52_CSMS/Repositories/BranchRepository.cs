using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly CSMSAppDbContext _context;

        public BranchRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BranchNameExistsAsync(string branchName)
        {
            return await _context.Branches
                .AsNoTracking()
                .AnyAsync(b => b.BranchName.ToLower() == branchName.ToLower());
        }

        public async Task<string> GenerateNextBranchIdAsync()
        {
            var lastBranch = await _context.Branches
                .AsNoTracking()
                .Where(b => b.BranchId.StartsWith(BranchConstants.BranchIdPrefix))
                .OrderByDescending(b => b.BranchId)
                .Select(b => b.BranchId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastBranch))
            {
                return "CB001";
            }

            var digitMatch = Regex.Match(lastBranch, @"\d+");
            if (!digitMatch.Success || !int.TryParse(digitMatch.Value, out var currentNumber))
            {
                return "CB001";
            }

            return $"{BranchConstants.BranchIdPrefix}{currentNumber + 1:D3}";
        }

        public async Task<List<Branch>> GetBranchesAsync(string? searchTerm, string? statusFilter, int? managerId, int pageIndex, int pageSize)
        {
            var query = _context.Branches
                .Include(b => b.BranchManagers)
                    .ThenInclude(bm => bm.Manager)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.BranchName.Contains(searchTerm) || b.Address.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(b => b.Status == statusFilter);
            }

            if (managerId.HasValue)
            {
                query = query.Where(b => b.BranchManagers.Any(bm => bm.ManagerId == managerId.Value));
            }

            return await query
                .OrderBy(b => b.BranchId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetBranchCountAsync(string? searchTerm, string? statusFilter, int? managerId)
        {
            var query = _context.Branches.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.BranchName.Contains(searchTerm) || b.Address.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(b => b.Status == statusFilter);
            }

            if (managerId.HasValue)
            {
                query = query.Where(b => b.BranchManagers.Any(bm => bm.ManagerId == managerId.Value));
            }

            return await query.CountAsync();
        }

        public async Task<Branch?> GetBranchByIdAsync(string branchId)
        {
            return await _context.Branches
                .Include(b => b.BranchManagers)
                    .ThenInclude(bm => bm.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == branchId);
        }

        public async Task<List<Employee>> GetEligibleManagersAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(e => e.Role == BranchConstants.BranchManagerRole && e.Status == BranchConstants.DefaultStatus)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task UpdateBranchAsync(Branch branch)
        {
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBranchManagersAsync(string branchId)
        {
            var assignments = _context.BranchManagers.Where(bm => bm.BranchId == branchId);
            _context.BranchManagers.RemoveRange(assignments);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBranchAsync(string branchId)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddBranchAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
        }

        public async Task AddBranchManagerAsync(BranchManager assignment)
        {
            _context.BranchManagers.Add(assignment);
            await _context.SaveChangesAsync();
        }
    }
}
