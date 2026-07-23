using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Repositories
{
    public class ShiftChangeRepository : IShiftChangeRepository
    {
        private readonly CSMSAppDbContext _context;

        public ShiftChangeRepository(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftChangeRequest>> GetRequestsAsync(int employeeId, DateTime? fromDate, DateTime? toDate, string status)
        {
            var query = _context.ShiftChangeRequests.AsQueryable();

            query = query.Where(r => r.RequestingEmployeeId == employeeId);

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.SubmittedAt.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(r => r.SubmittedAt.Date <= toDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                string statusFilter = status switch
                {
                    "Đã gửi" => "Submitted",
                    "Đã duyệt" => "Approved",
                    "Từ chối" => "Rejected",
                    "Đã hủy" => "Canceled",
                    _ => status
                };
                query = query.Where(r => r.Status == statusFilter);
            }

            return await query.OrderByDescending(r => r.SubmittedAt).ToListAsync();
        }

        public async Task<ShiftChangeRequest?> GetRequestByIdAsync(int requestId, int employeeId)
        {
            return await _context.ShiftChangeRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.RequestingEmployeeId == employeeId);
        }

        public async Task AddRequestAsync(ShiftChangeRequest request)
        {
            _context.ShiftChangeRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequestAsync(ShiftChangeRequest request)
        {
            _context.ShiftChangeRequests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}
