using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace SEP490_G52_CSMS.Repositories
{
    public class WarehouseSupplyRepository : IWarehouseSupplyRepository
    {
        private readonly CSMSAppDbContext _context;

        public WarehouseSupplyRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<BranchSupplyRequest?> GetRequestByCodeAsync(string requestCode, string? branchId = null)
        {
            var query = _context.BranchSupplyRequests
                .Include(r => r.Branch)
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .Where(r => r.RequestCode == requestCode);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(r => r.BranchId == branchId);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<BranchSupplyRequest?> GetRequestByIdAsync(int requestId)
        {
            return await _context.BranchSupplyRequests
                .Include(r => r.Branch)
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task<List<BranchSupplyRequest>> GetBranchRequestsAsync(string branchId, DateTime fromDate, DateTime toDate, string status, int page, int pageSize)
        {
            var query = ApplyBranchRequestFilters(branchId, fromDate, toDate, status);
            return await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetBranchRequestsCountAsync(string branchId, DateTime fromDate, DateTime toDate, string status)
        {
            var query = ApplyBranchRequestFilters(branchId, fromDate, toDate, status);
            return await query.CountAsync();
        }

        private IQueryable<BranchSupplyRequest> ApplyBranchRequestFilters(string branchId, DateTime fromDate, DateTime toDate, string status)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            var query = _context.BranchSupplyRequests
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .Where(r => r.BranchId == branchId && r.RequestDate >= start && r.RequestDate <= end);

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(r => r.Status == status);
            }

            return query;
        }

        public async Task<List<BranchSupplyRequest>> GetCentralExportRequestsAsync(
            DateTime fromDate, DateTime toDate, string status, int page, int pageSize, string? sortBy = "expected_asc", string? priorityFilter = "all", string? branchId = null)
        {
            var query = ApplyExportRequestFilters(fromDate, toDate, status, priorityFilter, branchId);

            query = (sortBy?.ToLower()) switch
            {
                "expected_asc" => query.OrderBy(r => r.ExpectedDeliveryDate ?? DateTime.MaxValue).ThenByDescending(r => r.RequestDate),
                "expected_desc" => query.OrderByDescending(r => r.ExpectedDeliveryDate ?? DateTime.MinValue).ThenByDescending(r => r.RequestDate),
                "request_asc" => query.OrderBy(r => r.RequestDate),
                "request_desc" => query.OrderByDescending(r => r.RequestDate),
                _ => query.OrderBy(r => r.ExpectedDeliveryDate ?? DateTime.MaxValue).ThenByDescending(r => r.RequestDate)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCentralExportRequestsCountAsync(DateTime fromDate, DateTime toDate, string status, string? priorityFilter = "all", string? branchId = null)
        {
            var query = ApplyExportRequestFilters(fromDate, toDate, status, priorityFilter, branchId);
            return await query.CountAsync();
        }

        public async Task<int> GetUrgentRequestsCountAsync(DateTime fromDate, DateTime toDate, string? branchId = null)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);
            var urgentDeadline = DateTime.Today.AddDays(2);

            var query = _context.BranchSupplyRequests
                .Where(r => r.RequestDate >= start && r.RequestDate <= end
                    && (r.Status == "Chờ duyệt" || r.Status == "Đang chuẩn bị xuất")
                    && r.ExpectedDeliveryDate.HasValue
                    && r.ExpectedDeliveryDate.Value.Date <= urgentDeadline);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(r => r.BranchId == branchId);
            }

            return await query.CountAsync();
        }

        private IQueryable<BranchSupplyRequest> ApplyExportRequestFilters(DateTime fromDate, DateTime toDate, string status, string? priorityFilter = "all", string? branchId = null)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            var query = _context.BranchSupplyRequests
                .Include(r => r.Branch)
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .Where(r => r.RequestDate >= start && r.RequestDate <= end);

            if (!string.IsNullOrEmpty(status) && !string.Equals(status, "Tất cả", StringComparison.OrdinalIgnoreCase) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(r => r.BranchId == branchId);
            }

            if (priorityFilter == "urgent")
            {
                var urgentDeadline = DateTime.Today.AddDays(2);
                query = query.Where(r => (r.Status == "Chờ duyệt" || r.Status == "Đang chuẩn bị xuất")
                    && r.ExpectedDeliveryDate.HasValue
                    && r.ExpectedDeliveryDate.Value.Date <= urgentDeadline);
            }

            return query;
        }

        public async Task<string> GenerateNextRequestCodeAsync()
        {
            var existingCodes = await _context.BranchSupplyRequests
                .Select(r => r.RequestCode)
                .ToListAsync();

            int maxId = 0;
            foreach (var code in existingCodes)
            {
                var match = Regex.Match(code ?? "", @"\d+");
                if (match.Success && int.TryParse(match.Value, out int id))
                {
                    if (id > maxId) maxId = id;
                }
            }

            return $"YC-{(maxId + 1):D3}";
        }

        public async Task AddRequestAsync(BranchSupplyRequest request)
        {
            await _context.BranchSupplyRequests.AddAsync(request);
        }

        public Task UpdateRequestAsync(BranchSupplyRequest request)
        {
            _context.BranchSupplyRequests.Update(request);
            return Task.CompletedTask;
        }

        public async Task<List<BranchInventory>> GetBranchInventoriesAsync(string branchId)
        {
            return await _context.BranchInventories
                .Include(bi => bi.Material)
                .Where(bi => bi.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<BranchInventory?> GetBranchInventoryAsync(string branchId, int materialId)
        {
            return await _context.BranchInventories
                .Include(bi => bi.Material)
                .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.MaterialId == materialId);
        }

        public async Task AddBranchInventoryAsync(BranchInventory inventory)
        {
            await _context.BranchInventories.AddAsync(inventory);
        }

        public async Task<List<Material>> SearchTrackedMaterialsAsync(string branchId, string term)
        {
            return await _context.BranchInventories
                .Where(bi => bi.BranchId == branchId)
                .Select(bi => bi.Material)
                .Where(m => m != null && m.MaterialName.Contains(term))
                .Select(m => m!)
                .Take(10)
                .ToListAsync();
        }

        public async Task<Material?> GetMaterialByIdAsync(int materialId)
        {
            return await _context.Materials.FindAsync(materialId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
