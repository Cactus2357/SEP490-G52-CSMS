using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager")]
    public class ShiftHandoverController : Controller
    {
        private readonly CSMSAppDbContext _context;

        public ShiftHandoverController(CSMSAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? date = null,
            int? shiftId = null,
            string? status = null,
            string? handoverType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var loggedInBranchId = User.GetBranchId() ?? "";
            var start = (fromDate ?? date ?? DateTime.Today).Date;
            var end = (toDate ?? date ?? DateTime.Today).Date;
            if (end < start)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            var allShifts = await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var rangeHandovers = await _context.CashHandovers
                .Include(h => h.OutgoingCashier)
                .Include(h => h.IncomingCashier)
                .Include(h => h.FixedShift)
                .Where(h => h.BranchId == loggedInBranchId && h.HandoverDate.Date >= start && h.HandoverDate.Date <= end)
                .OrderByDescending(h => h.HandoverDate)
                .ThenByDescending(h => h.OpenedAt)
                .ToListAsync();

            var allRows = new List<HandoverRowVM>();
            HandoverRowVM? openingInfo = null;
            HandoverRowVM? closingInfo = null;

            // Find opening/closing info for the displayed period
            var chronologicalHandovers = rangeHandovers.OrderBy(h => h.OpenedAt).ToList();
            foreach (var h in chronologicalHandovers)
            {
                if (h.HandoverType == CashHandoverConstants.HandoverTypeFirstShift && openingInfo == null)
                {
                    var shiftIndex = allShifts.FindIndex(s => s.ShiftId == h.ShiftId);
                    FixedShift? next = (shiftIndex >= 0 && shiftIndex < allShifts.Count - 1) ? allShifts[shiftIndex + 1] : null;
                    openingInfo = new HandoverRowVM { Handover = h, NextShift = next };
                }
                if (h.HandoverType == CashHandoverConstants.HandoverTypeLastShift)
                {
                    var shiftIndex = allShifts.FindIndex(s => s.ShiftId == h.ShiftId);
                    FixedShift? next = (shiftIndex >= 0 && shiftIndex < allShifts.Count - 1) ? allShifts[shiftIndex + 1] : null;
                    closingInfo = new HandoverRowVM { Handover = h, NextShift = next };
                }
            }

            foreach (var h in rangeHandovers)
            {
                var shiftIndex = allShifts.FindIndex(s => s.ShiftId == h.ShiftId);
                FixedShift? next = (shiftIndex >= 0 && shiftIndex < allShifts.Count - 1)
                    ? allShifts[shiftIndex + 1]
                    : null;

                var row = new HandoverRowVM { Handover = h, NextShift = next };
                allRows.Add(row);
            }

            if (openingInfo == null && allRows.Any())
            {
                openingInfo = allRows.Last(); // earliest in chronological
            }
            if (closingInfo == null && allRows.Any())
            {
                closingInfo = allRows.First(); // latest
            }

            var totalCashRevenueToday = rangeHandovers.Sum(h => h.MachineCashRevenue);
            var totalBankRevenueToday = rangeHandovers.Sum(h => h.BankTransferRevenue);
            var totalRevenueToday = totalCashRevenueToday + totalBankRevenueToday;
            var totalVarianceToday = rangeHandovers.Where(h => h.Status == CashHandoverConstants.ClosedStatus).Sum(h => h.ActualCash - h.TheoreticalCash);
            var handedOverCount = rangeHandovers.Count(h => h.Status == CashHandoverConstants.ClosedStatus);
            var emergencyCount = rangeHandovers.Count(h => h.HandoverType == CashHandoverConstants.HandoverTypeEmergency);

            IEnumerable<HandoverRowVM> filteredRows = allRows;

            if (shiftId.HasValue)
            {
                filteredRows = filteredRows.Where(r => r.Handover.ShiftId == shiftId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredRows = status == "Lệch tiền"
                    ? filteredRows.Where(r => r.Handover.ActualCash != r.Handover.TheoreticalCash)
                    : filteredRows.Where(r => r.Handover.ActualCash == r.Handover.TheoreticalCash);
            }

            if (!string.IsNullOrWhiteSpace(handoverType))
            {
                filteredRows = filteredRows.Where(r => r.Handover.HandoverType == handoverType);
            }

            var vm = new ShiftHandoverListViewModel
            {
                FromDate = start,
                ToDate = end,
                SelectedDate = start,
                AllShifts = allShifts,
                SelectedShiftId = shiftId,
                SelectedStatus = status,
                SelectedHandoverType = handoverType,
                Rows = filteredRows.ToList(),
                OpeningInfo = openingInfo,
                ClosingInfo = closingInfo,
                TotalRevenueToday = totalRevenueToday,
                TotalCashRevenueToday = totalCashRevenueToday,
                TotalBankRevenueToday = totalBankRevenueToday,
                HandedOverCount = handedOverCount,
                EmergencyCount = emergencyCount,
                TotalShiftsConfigured = allShifts.Count,
                TotalVarianceToday = totalVarianceToday
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DetailPartial(int id)
        {
            var loggedInBranchId = User.GetBranchId() ?? "";

            var handover = await _context.CashHandovers
                .Include(h => h.OutgoingCashier)
                .Include(h => h.IncomingCashier)
                .Include(h => h.FixedShift)
                .FirstOrDefaultAsync(h => h.HandoverId == id);

            if (handover == null)
            {
                return NotFound();
            }

            if (handover.BranchId != loggedInBranchId)
            {
                return Forbid();
            }

            var allShifts = await _context.FixedShifts.OrderBy(s => s.StartTime).ToListAsync();
            var shiftIndex = allShifts.FindIndex(s => s.ShiftId == handover.ShiftId);
            FixedShift? next = (shiftIndex >= 0 && shiftIndex < allShifts.Count - 1)
                ? allShifts[shiftIndex + 1]
                : null;

            var vm = new HandoverRowVM { Handover = handover, NextShift = next };
            return PartialView("_HandoverDetail", vm);
        }
    }

    public class ShiftHandoverListViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<FixedShift> AllShifts { get; set; } = new();
        public int? SelectedShiftId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SelectedHandoverType { get; set; }

        public List<HandoverRowVM> Rows { get; set; } = new();
        public HandoverRowVM? OpeningInfo { get; set; }
        public HandoverRowVM? ClosingInfo { get; set; }

        public decimal TotalRevenueToday { get; set; }
        public decimal TotalCashRevenueToday { get; set; }
        public decimal TotalBankRevenueToday { get; set; }
        public int HandedOverCount { get; set; }
        public int EmergencyCount { get; set; }
        public int TotalShiftsConfigured { get; set; }
        public decimal TotalVarianceToday { get; set; }
    }

    public class HandoverRowVM
    {
        public CashHandover Handover { get; set; } = null!;
        public FixedShift? NextShift { get; set; }

        public string HandoverTypeLabel => Handover.HandoverType switch
        {
            "FirstShift" => "Mở ca đầu ngày",
            "MidShift" => "Giao ca thường",
            "Emergency" => "Bàn giao đột xuất",
            "LastShift" => "Đóng ca cuối ngày",
            _ => "Giao ca"
        };

        public string HandoverTypeBadgeClass => Handover.HandoverType switch
        {
            "FirstShift" => "bg-info text-dark",
            "MidShift" => "bg-primary",
            "Emergency" => "bg-danger",
            "LastShift" => "bg-dark",
            _ => "bg-secondary"
        };
    }
}
