using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly CSMSAppDbContext _context;

        public VoucherService(CSMSAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voucher>> GetBranchVouchersAsync(string branchId, string? status = null, string? search = null)
        {
            var query = _context.Vouchers
                .Include(v => v.Creator)
                .Include(v => v.Branch)
                .Where(v => string.IsNullOrEmpty(branchId) || v.BranchId == branchId || v.BranchId == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(v => v.VoucherCode.ToLower().Contains(term) || (v.Description != null && v.Description.ToLower().Contains(term)));
            }

            var list = await query.OrderByDescending(v => v.CreatedAt).ToListAsync();

            if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả")
            {
                var now = DateTime.Now;
                switch (status.ToLower())
                {
                    case "active":
                    case "đang hoạt động":
                    case "đang diễn ra":
                        list = list.Where(v => v.IsActive && now >= v.StartDate && now <= v.EndDate && v.UsedCount < v.Quantity).ToList();
                        break;
                    case "upcoming":
                    case "chưa bắt đầu":
                        list = list.Where(v => v.IsActive && now < v.StartDate).ToList();
                        break;
                    case "expired":
                    case "đã kết thúc":
                        list = list.Where(v => now > v.EndDate).ToList();
                        break;
                    case "usedup":
                    case "đã hết lượt":
                        list = list.Where(v => v.UsedCount >= v.Quantity).ToList();
                        break;
                    case "paused":
                    case "tạm dừng":
                        list = list.Where(v => !v.IsActive).ToList();
                        break;
                }
            }

            return list;
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int voucherId, string? branchId = null)
        {
            var query = _context.Vouchers
                .Include(v => v.Creator)
                .Include(v => v.Branch)
                .Where(v => v.VoucherId == voucherId);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(v => v.BranchId == branchId || v.BranchId == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string voucherCode, string? branchId = null)
        {
            if (string.IsNullOrWhiteSpace(voucherCode)) return null;
            var code = voucherCode.Trim().ToUpper();

            var query = _context.Vouchers
                .Where(v => v.VoucherCode.ToUpper() == code);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(v => v.BranchId == branchId || v.BranchId == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message, Voucher? Voucher, decimal DiscountPercent, decimal DiscountAmount)> ValidateVoucherForOrderAsync(string voucherCode, string branchId, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(voucherCode))
            {
                return (false, "Mã giảm giá không tồn tại", null, 0, 0);
            }

            var code = voucherCode.Trim().ToUpper();
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherCode.ToUpper() == code && (string.IsNullOrEmpty(branchId) || v.BranchId == branchId || v.BranchId == null));

            if (voucher == null || !voucher.IsActive)
            {
                return (false, "Mã giảm giá không tồn tại", null, 0, 0);
            }

            var now = DateTime.Now;
            if (now < voucher.StartDate || now > voucher.EndDate || voucher.UsedCount >= voucher.Quantity)
            {
                return (false, "Mã giảm giá không tồn tại", null, 0, 0);
            }

            if (subtotal <= 0)
            {
                return (false, "Giỏ hàng hiện chưa có món để áp dụng mã giảm giá.", null, 0, 0);
            }

            decimal discountAmount = Math.Round(subtotal * (voucher.DiscountPercent / 100m));
            if (discountAmount > subtotal) discountAmount = subtotal;

            return (true, $"Áp dụng voucher '{code}' thành công: Giảm {voucher.DiscountPercent:G29}% (tiết kiệm {discountAmount:N0} đ).", voucher, voucher.DiscountPercent, discountAmount);
        }

        public async Task<(bool Success, string Message)> CreateVoucherAsync(Voucher voucher, int managerId)
        {
            if (voucher == null) return (false, "Dữ liệu voucher không hợp lệ.");

            voucher.VoucherCode = (voucher.VoucherCode ?? "").Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(voucher.VoucherCode))
            {
                return (false, "Mã voucher không được để trống.");
            }

            if (voucher.VoucherCode.Length > 50)
            {
                return (false, "Mã voucher không được vượt quá 50 ký tự.");
            }

            if (voucher.DiscountPercent <= 0 || voucher.DiscountPercent > 100)
            {
                return (false, "Phần trăm giảm giá phải lớn hơn 0% và không vượt quá 100%.");
            }

            if (voucher.Quantity <= 0)
            {
                return (false, "Số lượng voucher phát hành phải lớn hơn 0.");
            }

            if (voucher.StartDate >= voucher.EndDate)
            {
                return (false, "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }

            // Kiểm tra trùng mã trong chi nhánh hoặc toàn hệ thống
            bool codeExists = await _context.Vouchers.AnyAsync(v => 
                v.VoucherCode.ToUpper() == voucher.VoucherCode && 
                (v.BranchId == voucher.BranchId || v.BranchId == null || voucher.BranchId == null));

            if (codeExists)
            {
                return (false, $"Mã voucher '{voucher.VoucherCode}' đã tồn tại trong chi nhánh hoặc toàn chuỗi. Vui lòng chọn mã khác.");
            }

            voucher.CreatedBy = managerId;
            voucher.CreatedAt = DateTime.UtcNow;
            voucher.UsedCount = 0;
            voucher.IsActive = true;

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return (true, $"Tạo mã voucher '{voucher.VoucherCode}' thành công!");
        }

        public async Task<bool> ToggleVoucherStatusAsync(int voucherId, string branchId)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherId == voucherId && (string.IsNullOrEmpty(branchId) || v.BranchId == branchId || v.BranchId == null));
            if (voucher == null) return false;

            voucher.IsActive = !voucher.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Order>> GetVoucherOrderHistoryAsync(int voucherId, string branchId)
        {
            return await _context.Orders
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Where(o => o.VoucherId == voucherId && (string.IsNullOrEmpty(branchId) || o.BranchId == branchId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IncrementVoucherUsageAsync(int voucherId)
        {
            var voucher = await _context.Vouchers.FindAsync(voucherId);
            if (voucher == null) return false;

            voucher.UsedCount += 1;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
