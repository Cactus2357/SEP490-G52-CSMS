using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Attendance;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager,Cashier")]
    public class ShiftHandoverController : Controller
    {
        private readonly CSMSAppDbContext _context;

        public ShiftHandoverController(CSMSAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date, int? shiftId, string? status)
        {
            var loggedInBranchId = User.GetBranchId() ?? "";
            var targetDate = (date ?? DateTime.Today).Date;

            var allShifts = await _context.FixedShifts
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var dayHandovers = await _context.CashHandovers
                .Include(h => h.OutgoingCashier)
                .Include(h => h.IncomingCashier)
                .Include(h => h.FixedShift)
                .Where(h => h.BranchId == loggedInBranchId && h.HandoverDate.Date == targetDate)
                .ToListAsync();

            var transitionRows = new List<HandoverRowVM>();
            HandoverRowVM? openingInfo = null;
            HandoverRowVM? closingInfo = null;

            var orderedHandovers = dayHandovers.OrderBy(h => h.FixedShift?.StartTime ?? TimeSpan.Zero).ToList();
            if (orderedHandovers.Any())
            {
                var firstHandover = orderedHandovers.First();
                var firstShiftIndex = allShifts.FindIndex(s => s.ShiftId == firstHandover.ShiftId);
                FixedShift? nextForFirst = (firstShiftIndex >= 0 && firstShiftIndex < allShifts.Count - 1)
                    ? allShifts[firstShiftIndex + 1]
                    : null;
                openingInfo = new HandoverRowVM { Handover = firstHandover, NextShift = nextForFirst };
            }

            foreach (var h in orderedHandovers)
            {
                var shiftIndex = allShifts.FindIndex(s => s.ShiftId == h.ShiftId);
                FixedShift? next = (shiftIndex >= 0 && shiftIndex < allShifts.Count - 1)
                    ? allShifts[shiftIndex + 1]
                    : null;

                var row = new HandoverRowVM { Handover = h, NextShift = next };

                if (next != null)
                {
                    transitionRows.Add(row);
                }
                else
                {
                    closingInfo = row;
                }
            }

            var totalRevenueToday = dayHandovers.Sum(h => h.MachineCashRevenue + (h.TheoreticalCash - h.InitialCash));
            var totalVarianceToday = dayHandovers.Sum(h => h.ActualCash - h.TheoreticalCash);
            var handedOverCount = dayHandovers.Count(h => h.IsPasswordConfirmed);

            IEnumerable<HandoverRowVM> filteredRows = transitionRows;

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

            var vm = new ShiftHandoverListViewModel
            {
                SelectedDate = targetDate,
                AllShifts = allShifts,
                SelectedShiftId = shiftId,
                SelectedStatus = status,
                Rows = filteredRows.ToList(),
                OpeningInfo = openingInfo,
                ClosingInfo = closingInfo,
                TotalRevenueToday = totalRevenueToday,
                HandedOverCount = handedOverCount,
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
        public DateTime SelectedDate { get; set; }
        public List<FixedShift> AllShifts { get; set; } = new();
        public int? SelectedShiftId { get; set; }
        public string? SelectedStatus { get; set; }

        public List<HandoverRowVM> Rows { get; set; } = new();
        public HandoverRowVM? OpeningInfo { get; set; }
        public HandoverRowVM? ClosingInfo { get; set; }

        public decimal TotalRevenueToday { get; set; }
        public int HandedOverCount { get; set; }
        public int TotalShiftsConfigured { get; set; }
        public decimal TotalVarianceToday { get; set; }
    }

    public class HandoverRowVM
    {
        public CashHandover Handover { get; set; } = null!;
        public FixedShift? NextShift { get; set; }
    }
}
