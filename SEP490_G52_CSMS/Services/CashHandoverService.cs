using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories;

namespace SEP490_G52_CSMS.Services
{
    public class CashHandoverService : ICashHandoverService
    {
        private readonly ICashHandoverRepository _cashHandoverRepository;

        public CashHandoverService(ICashHandoverRepository cashHandoverRepository)
        {
            _cashHandoverRepository = cashHandoverRepository;
        }

        // =========================================================
        //  MỞ CA (UC11)
        // =========================================================

        public async Task<OpenShiftViewModel?> GetOpenShiftModelAsync(int cashierId)
        {
            var today = DateTime.Today;

            // Kiểm tra đã có ca Active hôm nay chưa
            var existingActive = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today);
            if (existingActive != null)
            {
                // Ca đã được mở — trả null để Controller biết chuyển sang tab Giao ca
                return null;
            }

            // Lấy lịch trực hôm nay của thu ngân (trả về ca hiện tại hoặc ca sắp tới trong ngày)
            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(cashierId, today);
            
            // Lấy tên thu ngân thực hiện mở ca
            var cashierEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(cashierId);

            // Lấy thông tin ca gần nhất đã đóng của chi nhánh
            var branchId = roster?.BranchId ?? cashierEmployee?.BranchId ?? string.Empty;
            var lastHandover = string.IsNullOrEmpty(branchId)
                ? null
                : await _cashHandoverRepository.GetLastClosedHandoverForBranchAsync(branchId);

            bool isTimeToOpen = false;
            if (roster != null && roster.FixedShift != null)
            {
                // Kiểm tra xem đã đến giờ mở ca chưa (cho phép mở sớm 30 phút và không cho mở nếu đã hết ca)
                var currentTime = DateTime.Now.TimeOfDay;
                var thirtyMinutes = TimeSpan.FromMinutes(30);
                isTimeToOpen = currentTime >= roster.FixedShift.StartTime.Subtract(thirtyMinutes) && 
                               currentTime <= roster.FixedShift.EndTime;
            }

            var model = new OpenShiftViewModel
            {
                CashierId = cashierId,
                BranchId = branchId,
                ShiftId = roster?.ShiftId ?? 0,
                ShiftName = roster?.FixedShift?.ShiftName ?? "(Chưa có ca trực)",
                ShiftTimeRange = roster?.FixedShift != null
                    ? $"{roster.FixedShift.StartTime:hh\\:mm} – {roster.FixedShift.EndTime:hh\\:mm}"
                    : "-",
                HandoverDate = today,
                CashierName = cashierEmployee?.FullName
                              ?? CashHandoverConstants.UnassignedCashierLabel,
                PreviousCashierName = lastHandover?.OutgoingCashier?.FullName
                                      ?? CashHandoverConstants.UnassignedCashierLabel,
                PreviousShiftName = lastHandover?.FixedShift?.ShiftName ?? "-",
                PreviousHandoverDate = lastHandover?.HandoverDate.ToString("dd/MM/yyyy") ?? "-",
                PreviousInitialCash = lastHandover != null
                    ? lastHandover.InitialCash.ToString("N0") + " ₫"
                    : "-",
                PreviousApproverName = lastHandover?.IncomingCashier?.FullName ?? "-",
                InitialCash = 0,
                IsTimeToOpen = isTimeToOpen
            };

            return model;
        }

        public async Task<OperationResult> OpenShiftAsync(OpenShiftViewModel model)
        {
            var today = DateTime.Today;

            // Kiểm tra đã mở ca hôm nay chưa (tránh duplicate)
            var existing = await _cashHandoverRepository.GetActiveHandoverAsync(model.CashierId, today);
            if (existing != null)
            {
                return OperationResult.Fail("Ca làm việc hôm nay đã được mở. Vui lòng chuyển sang Giao ca.");
            }

            // Kiểm tra lại trên server xem đã đúng giờ mở ca chưa
            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(model.CashierId, today);
            if (roster == null)
            {
                return OperationResult.Fail("Bạn không có lịch trực vào thời gian này.");
            }
            
            var currentTime = DateTime.Now.TimeOfDay;
            var thirtyMinutes = TimeSpan.FromMinutes(30);
            if (currentTime < roster.FixedShift.StartTime.Subtract(thirtyMinutes))
            {
                return OperationResult.Fail("Chưa đến giờ mở ca. Bạn chỉ có thể mở ca trước 30 phút so với giờ bắt đầu ca trực.");
            }

            // Tạo bản ghi CashHandover mới với trạng thái Active
            var handover = new CashHandover
            {
                BranchId = model.BranchId,
                HandoverDate = today,
                ShiftId = model.ShiftId,
                OutgoingCashierId = model.CashierId,
                IncomingCashierId = model.CashierId,   // Tạm thời = chính mình, cập nhật khi giao ca
                InitialCash = model.InitialCash,
                MachineCashRevenue = 0,
                BankTransferRevenue = 0,               // Cập nhật sau từ module Bán hàng
                TheoreticalCash = model.InitialCash,   // Lúc mở ca TheoreticalCash = InitialCash
                ActualCash = 0,
                IsPasswordConfirmed = false,
                Status = CashHandoverConstants.ActiveStatus,
                OpenedAt = DateTime.Now,
            };

            await _cashHandoverRepository.AddHandoverAsync(handover);
            return OperationResult.Ok($"Đã mở ca thành công. Tiền đầu ca: {model.InitialCash:N0} đ");
        }

        // =========================================================
        //  GIAO CA & ĐÓNG CA
        // =========================================================

        public async Task<HandoverViewModel?> GetHandoverModelAsync(int cashierId)
        {
            var today = DateTime.Today;

            // Phải có ca đang Active mới được giao
            var activeHandover = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today);
            if (activeHandover == null)
            {
                return null;
            }

            // Lấy danh sách thu ngân cùng chi nhánh để chọn người nhận ca
            var cashiers = await _cashHandoverRepository.GetCashiersInBranchAsync(activeHandover.BranchId);

            // Tìm người nhận ca tiếp theo
            var currentTime = DateTime.Now.TimeOfDay;
            var nextCashierId = await _cashHandoverRepository.GetNextCashierForHandoverAsync(activeHandover.BranchId, today, currentTime);

            return new HandoverViewModel
            {
                HandoverId = activeHandover.HandoverId,
                OutgoingCashierId = cashierId,
                BranchId = activeHandover.BranchId,
                OutgoingCashierName = activeHandover.OutgoingCashier?.FullName
                                      ?? CashHandoverConstants.UnassignedCashierLabel,
                ShiftName = activeHandover.FixedShift?.ShiftName ?? "-",
                InitialCash = activeHandover.InitialCash,
                BankTransferRevenue = activeHandover.BankTransferRevenue,  // Doanh thu CK (không vào két)
                MachineCashRevenue = activeHandover.MachineCashRevenue,
                IncomingCashierId = nextCashierId, // Tự động chọn người nhận ca tiếp theo
                IncomingCashiers = cashiers.Select(c => new CashierOption
                {
                    CashierId = c.EmployeeId,
                    CashierName = c.FullName ?? c.Username ?? CashHandoverConstants.UnassignedCashierLabel
                }).ToList(),
            };
        }

        public async Task<OperationResult> CloseShiftAsync(HandoverViewModel model)
        {
            // 1. Lấy bản ghi CashHandover đang Active
            var handover = await _cashHandoverRepository.GetHandoverByIdAsync(model.HandoverId);
            if (handover == null || handover.Status != CashHandoverConstants.ActiveStatus)
            {
                return OperationResult.Fail("Không tìm thấy ca đang mở. Vui lòng kiểm tra lại.");
            }

            // 2. Xác minh mật khẩu người nhận ca (BR-02)
            if (!model.IncomingCashierId.HasValue || model.IncomingCashierId <= 0)
            {
                return OperationResult.Fail("Vui lòng chọn người nhận ca.");
            }

            var incomingEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.IncomingCashierId.Value);
            if (incomingEmployee == null)
            {
                return OperationResult.Fail("Không tìm thấy thông tin nhân viên nhận ca.");
            }

            // So sánh mật khẩu bằng DAT_PasswordHasher
            if (!DAT_PasswordHasher.VerifyPassword(model.IncomingPassword, incomingEmployee.Password ?? ""))
            {
                return OperationResult.Fail("Mật khẩu xác nhận không chính xác. Vui lòng thử lại.");
            }

            // 3. Tính toán nghiệp vụ (BR-01): TheoreticalCash = InitialCash + MachineCashRevenue
            var theoretical = handover.InitialCash + handover.MachineCashRevenue;

            // 4. Cập nhật bản ghi CashHandover → đóng ca
            handover.IncomingCashierId = model.IncomingCashierId.Value;
            handover.ActualCash = model.ActualCash;
            handover.TheoreticalCash = theoretical;
            handover.Notes = model.Notes;
            handover.IsPasswordConfirmed = true;
            handover.Status = CashHandoverConstants.ClosedStatus;
            handover.ClosedAt = DateTime.Now;

            await _cashHandoverRepository.UpdateHandoverAsync(handover);

            var discrepancy = model.ActualCash - theoretical;
            var discrepancyText = discrepancy >= 0
                ? $"+{discrepancy:N0} đ"
                : $"{discrepancy:N0} đ";

            return OperationResult.Ok($"Giao ca thành công. Chênh lệch: {discrepancyText}");
        }

        // =========================================================
        //  LỊCH SỬ GIAO CA
        // =========================================================

        public async Task<HandoverHistoryViewModel> GetHistoryAsync(string branchId, int pageIndex)
        {
            var pageSize = CashHandoverConstants.HistoryPageSize;
            var items = await _cashHandoverRepository.GetHandoverHistoryAsync(branchId, pageIndex, pageSize);
            var total = await _cashHandoverRepository.GetHandoverCountAsync(branchId);

            return new HandoverHistoryViewModel
            {
                BranchId = branchId,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = total,
                Items = items.Select(ch =>
                {
                    var theoretical = ch.InitialCash + ch.MachineCashRevenue;
                    var discrepancy = ch.ActualCash - theoretical;
                    return new HandoverHistoryItemViewModel
                    {
                        HandoverId = ch.HandoverId,
                        HandoverDate = ch.HandoverDate.ToString("dd/MM/yyyy"),
                        ShiftName = ch.FixedShift?.ShiftName ?? "-",
                        OutgoingCashierName = ch.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                        IncomingCashierName = ch.IncomingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                        InitialCash = ch.InitialCash.ToString("N0") + " đ",
                        MachineCashRevenue = ch.MachineCashRevenue.ToString("N0") + " đ",
                        TheoreticalCash = theoretical.ToString("N0") + " đ",
                        ActualCash = ch.ActualCash.ToString("N0") + " đ",
                        Discrepancy = (discrepancy >= 0 ? "+" : "") + discrepancy.ToString("N0") + " đ",
                        Status = ch.Status,
                        OpenedAt = ch.OpenedAt.ToString("HH:mm dd/MM/yyyy"),
                        ClosedAt = ch.ClosedAt?.ToString("HH:mm dd/MM/yyyy") ?? "-",
                        Notes = ch.Notes,
                        SessionType = ch.Status == CashHandoverConstants.ActiveStatus ? "Mở ca" : "Giao ca",
                    };
                }).ToList()
            };
        }

        // =========================================================
        //  CHỌN THU NGÂN
        // =========================================================

        public async Task<List<CashierSelectItemViewModel>> GetCashiersAsync(string branchId)
        {
            var today = DateTime.Today;
            var cashiers = await _cashHandoverRepository.GetCashiersInBranchAsync(branchId);

            var result = new List<CashierSelectItemViewModel>();
            foreach (var emp in cashiers)
            {
                var active = await _cashHandoverRepository.GetActiveHandoverAsync(emp.EmployeeId, today);
                result.Add(new CashierSelectItemViewModel
                {
                    CashierId      = emp.EmployeeId,
                    CashierName    = emp.FullName ?? emp.Username ?? "–",
                    Username       = emp.Username ?? "–",
                    BranchId       = branchId,
                    HasActiveShift = active != null,
                    ActiveShiftName = active?.FixedShift?.ShiftName ?? string.Empty,
                });
            }
            return result;
        }

        public async Task<int?> GetCurrentCashierIdAsync(string branchId)
        {
            return await _cashHandoverRepository.GetCurrentCashierIdAsync(branchId, DateTime.Today, DateTime.Now.TimeOfDay);
        }
    }
}
