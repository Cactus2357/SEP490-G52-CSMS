using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

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
        //  XÁC ĐỊNH LOẠI CA HIỆN TẠI
        // =========================================================

        public async Task<string> DetermineCurrentShiftPhaseAsync(int cashierId, string branchId)
        {
            var today = DateTime.Today;
            var activeHandover = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today)
                              ?? await _cashHandoverRepository.GetCurrentActiveHandoverForBranchAsync(branchId, today);

            if (activeHandover != null)
            {
                return await _cashHandoverRepository.GetShiftPhaseAsync(activeHandover.ShiftId);
            }

            // Nếu chưa có ca nào mở hôm nay trong chi nhánh -> Luôn là FirstShift (Mở ca đầu ngày)
            return CashHandoverConstants.HandoverTypeFirstShift;
        }

        // =========================================================
        //  1. MỞ CA ĐẦU NGÀY (UC11)
        // =========================================================

        public async Task<OpenShiftViewModel?> GetOpenShiftModelAsync(int cashierId)
        {
            var today = DateTime.Today;

            var cashierEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(cashierId);
            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(cashierId, today);
            var branchId = roster?.BranchId ?? cashierEmployee?.BranchId ?? "CN001";

            // Chỉ thu ngân có lịch làm việc hôm nay mới được mở ca
            if (roster == null)
            {
                return null;
            }

            // Kiểm tra đã có ca Active hôm nay tại chi nhánh chưa
            var existingActive = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today)
                              ?? await _cashHandoverRepository.GetCurrentActiveHandoverForBranchAsync(branchId, today);
            if (existingActive != null)
            {
                return null; // Đã có ca mở -> chuyển sang Giao ca hoặc Đóng ca
            }

            var lastHandover = string.IsNullOrEmpty(branchId)
                ? null
                : await _cashHandoverRepository.GetLastClosedHandoverForBranchAsync(branchId);

            var allShifts = await _cashHandoverRepository.GetAllFixedShiftsAsync();
            var firstShift = allShifts.FirstOrDefault();

            int shiftId = roster.ShiftId;
            string shiftName = roster.FixedShift?.ShiftName ?? firstShift?.ShiftName ?? "Ca 1";
            string shiftTimeRange = roster.FixedShift != null
                ? $"{roster.FixedShift.StartTime:hh\\:mm} – {roster.FixedShift.EndTime:hh\\:mm}"
                : (firstShift != null ? $"{firstShift.StartTime:hh\\:mm} – {firstShift.EndTime:hh\\:mm}" : "-");

            bool isTimeToOpen = true;
            if (roster.FixedShift != null)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var thirtyMinutes = TimeSpan.FromMinutes(30);
                isTimeToOpen = currentTime >= roster.FixedShift.StartTime.Subtract(thirtyMinutes);
            }

            bool isFirstShift = (lastHandover == null || lastHandover.HandoverDate.Date < today.Date);
            decimal initialCash = 0;
            if (!isFirstShift && lastHandover != null)
            {
                initialCash = lastHandover.ActualCash;
            }
            else if (lastHandover != null && lastHandover.ActualCash > 0)
            {
                initialCash = lastHandover.ActualCash;
            }

            var model = new OpenShiftViewModel
            {
                CashierId = cashierId,
                BranchId = branchId,
                ShiftId = shiftId,
                ShiftName = shiftName,
                ShiftTimeRange = shiftTimeRange,
                HandoverDate = today,
                CashierName = cashierEmployee?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                PreviousCashierName = lastHandover?.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                PreviousShiftName = lastHandover?.FixedShift?.ShiftName ?? "-",
                PreviousHandoverDate = lastHandover?.HandoverDate.ToString("dd/MM/yyyy") ?? "-",
                PreviousInitialCash = lastHandover != null ? lastHandover.InitialCash.ToString("N0") + " ₫" : "-",
                PreviousApproverName = lastHandover?.IncomingCashier?.FullName ?? "-",
                InitialCash = initialCash,
                IsFirstShift = isFirstShift,
                IsTimeToOpen = isTimeToOpen
            };

            return model;
        }

        public async Task<OperationResult> OpenShiftAsync(OpenShiftViewModel model)
        {
            var today = DateTime.Today;

            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(model.CashierId, today);
            if (roster == null)
            {
                return OperationResult.Fail("Bạn không có lịch trực ca hôm nay tại chi nhánh. Chỉ thu ngân có lịch làm việc mới được mở ca.");
            }

            var activeShiftHandover = await _cashHandoverRepository.GetActiveHandoverByShiftAsync(model.BranchId, model.ShiftId, today);
            if (activeShiftHandover != null)
            {
                return OperationResult.Fail("Ca làm việc này tại chi nhánh đã được mở. Không thể tạo ca trùng lặp.");
            }

            var existing = await _cashHandoverRepository.GetActiveHandoverAsync(model.CashierId, today);
            if (existing != null)
            {
                return OperationResult.Fail("Ca làm việc hôm nay của bạn đã được mở. Vui lòng chuyển sang Giao ca.");
            }

            var currentTime = DateTime.Now.TimeOfDay;
            var thirtyMinutes = TimeSpan.FromMinutes(30);
            if (roster.FixedShift != null && currentTime < roster.FixedShift.StartTime.Subtract(thirtyMinutes))
            {
                return OperationResult.Fail("Chưa đến giờ mở ca. Bạn chỉ có thể mở ca trước 30 phút so với giờ bắt đầu ca trực.");
            }

            var cashierEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.CashierId);
            var branchId = roster?.BranchId ?? cashierEmployee?.BranchId ?? model.BranchId;
            var lastHandover = string.IsNullOrEmpty(branchId)
                ? null
                : await _cashHandoverRepository.GetLastClosedHandoverForBranchAsync(branchId);

            decimal initialCash = model.InitialCash;
            bool isFirstShift = (lastHandover == null || lastHandover.HandoverDate.Date < today.Date);
            if (!isFirstShift && lastHandover != null)
            {
                initialCash = lastHandover.ActualCash;
            }

            var handover = new CashHandover
            {
                BranchId = model.BranchId,
                HandoverDate = today,
                ShiftId = model.ShiftId,
                OutgoingCashierId = model.CashierId,
                IncomingCashierId = model.CashierId,
                InitialCash = initialCash,
                MachineCashRevenue = 0,
                BankTransferRevenue = 0,
                CashRefundAmount = 0,
                TheoreticalCash = initialCash,
                ActualCash = 0,
                IsPasswordConfirmed = false,
                HandoverType = CashHandoverConstants.HandoverTypeFirstShift,
                Status = CashHandoverConstants.ActiveStatus,
                OpenedAt = DateTime.Now,
            };

            await _cashHandoverRepository.AddHandoverAsync(handover);
            await _cashHandoverRepository.SyncAttendanceOnOpenShiftAsync(model.CashierId, model.ShiftId, today);

            return OperationResult.Ok($"Đã mở ca đầu ngày thành công. Tiền đầu ca: {initialCash:N0} đ");
        }

        // =========================================================
        //  2. BÀN GIAO CA GIỮA NGÀY (UC12)
        // =========================================================

        public async Task<HandoverViewModel?> GetHandoverModelAsync(int cashierId)
        {
            var today = DateTime.Today;

            var activeHandover = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today);
            if (activeHandover == null)
            {
                var cashier = await _cashHandoverRepository.GetEmployeeByIdAsync(cashierId);
                if (cashier != null && !string.IsNullOrEmpty(cashier.BranchId))
                {
                    activeHandover = await _cashHandoverRepository.GetCurrentActiveHandoverForBranchAsync(cashier.BranchId, today);
                }
            }

            if (activeHandover == null)
            {
                return null;
            }

            // Kiểm tra xem ca hiện tại có ca làm việc tiếp theo không
            var nextShift = await _cashHandoverRepository.GetNextFixedShiftAsync(activeHandover.ShiftId);
            if (nextShift == null)
            {
                // Ca này là ca cuối cùng trong ngày -> Không thể bàn giao, phải đóng ca
                return null;
            }

            // Tính doanh thu và hoàn tiền mặt thực tế phát sinh trong ca
            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                activeHandover.BranchId, activeHandover.OutgoingCashierId, activeHandover.OpenedAt, null);

            var cashiers = await _cashHandoverRepository.GetCashiersInBranchAsync(activeHandover.BranchId);

            var currentTime = DateTime.Now.TimeOfDay;
            var nextCashierId = await _cashHandoverRepository.GetNextCashierForHandoverAsync(activeHandover.BranchId, today, currentTime);

            bool isSelfHandover = (nextCashierId.HasValue && nextCashierId.Value == activeHandover.OutgoingCashierId);

            return new HandoverViewModel
            {
                HandoverId = activeHandover.HandoverId,
                OutgoingCashierId = activeHandover.OutgoingCashierId,
                BranchId = activeHandover.BranchId,
                OutgoingCashierName = activeHandover.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                ShiftName = activeHandover.FixedShift?.ShiftName ?? "-",
                OpenedAt = activeHandover.OpenedAt,
                InitialCash = activeHandover.InitialCash,
                MachineCashRevenue = cashRev,
                BankTransferRevenue = bankRev,
                CashRefundAmount = cashRefunds,
                TargetShiftName = nextShift.ShiftName,
                TargetShiftTimeRange = $"{nextShift.StartTime:hh\\:mm} – {nextShift.EndTime:hh\\:mm}",
                IsSelfHandover = isSelfHandover,
                IncomingCashierId = nextCashierId ?? activeHandover.OutgoingCashierId,
                DelivererName = activeHandover.DelivererName,
                IncomingCashiers = cashiers.Select(c => new CashierOption
                {
                    CashierId = c.EmployeeId,
                    CashierName = c.FullName ?? c.Username ?? CashHandoverConstants.UnassignedCashierLabel
                }).ToList(),
            };
        }

        public async Task<OperationResult> CloseShiftAsync(HandoverViewModel model)
        {
            var handover = await _cashHandoverRepository.GetHandoverByIdAsync(model.HandoverId);
            if (handover == null || handover.Status != CashHandoverConstants.ActiveStatus)
            {
                return OperationResult.Fail("Không tìm thấy ca đang mở. Vui lòng kiểm tra lại.");
            }

            var nextShift = await _cashHandoverRepository.GetNextFixedShiftAsync(handover.ShiftId);
            if (nextShift == null)
            {
                return OperationResult.Fail("Hiện tại là ca cuối cùng trong ngày (không có ca tiếp theo). Không thể thực hiện bàn giao, vui lòng sử dụng chức năng Đóng ca cuối ngày.");
            }

            if (!model.IncomingCashierId.HasValue || model.IncomingCashierId <= 0)
            {
                return OperationResult.Fail("Vui lòng chọn người nhận ca.");
            }

            var incomingEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.IncomingCashierId.Value);
            if (incomingEmployee == null)
            {
                return OperationResult.Fail("Không tìm thấy thông tin nhân viên nhận ca.");
            }

            // Xác minh mật khẩu người nhận ca
            if (!DAT_PasswordHasher.VerifyPassword(model.IncomingPassword, incomingEmployee.Password ?? ""))
            {
                return OperationResult.Fail("Mật khẩu xác nhận không chính xác. Vui lòng thử lại.");
            }

            // Tính toán doanh thu và hoàn tiền chính xác đến thời điểm đóng ca
            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                handover.BranchId, handover.OutgoingCashierId, handover.OpenedAt, DateTime.Now);

            var theoretical = handover.InitialCash + cashRev - cashRefunds;
            var discrepancy = model.ActualCash - theoretical;

            if (discrepancy != 0 && string.IsNullOrWhiteSpace(model.Notes))
            {
                return OperationResult.Fail("Tiền két bị chênh lệch so với hệ thống. Vui lòng nhập lý do chênh lệch!");
            }

            handover.IncomingCashierId = model.IncomingCashierId.Value;
            handover.MachineCashRevenue = cashRev;
            handover.BankTransferRevenue = bankRev;
            handover.CashRefundAmount = cashRefunds;
            handover.ActualCash = model.ActualCash;
            handover.TheoreticalCash = theoretical;
            handover.Notes = model.Notes;
            handover.DelivererName = model.DelivererName;
            handover.IsPasswordConfirmed = true;
            handover.HandoverType = model.IsSelfHandover ? "SelfHandover" : CashHandoverConstants.HandoverTypeMidShift;
            handover.Status = CashHandoverConstants.ClosedStatus;
            handover.ClosedAt = DateTime.Now;

            await _cashHandoverRepository.UpdateHandoverAsync(handover);
            await _cashHandoverRepository.SyncAttendanceOnCloseShiftAsync(handover.OutgoingCashierId, handover.ShiftId, handover.HandoverDate);

            // Tự động kích hoạt bản ghi mở ca mới cho ca tiếp theo (hoặc cho chính mình nếu làm 2 ca liên tiếp)
            if (nextShift != null)
            {
                var nextShiftActive = await _cashHandoverRepository.GetActiveHandoverByShiftAsync(handover.BranchId, nextShift.ShiftId, handover.HandoverDate);
                if (nextShiftActive == null)
                {
                    var nextHandover = new CashHandover
                    {
                        BranchId = handover.BranchId,
                        HandoverDate = handover.HandoverDate,
                        ShiftId = nextShift.ShiftId,
                        OutgoingCashierId = model.IncomingCashierId.Value,
                        IncomingCashierId = model.IncomingCashierId.Value,
                        InitialCash = model.ActualCash, // Kế thừa số dư thực tế từ ca trước
                        MachineCashRevenue = 0,
                        BankTransferRevenue = 0,
                        CashRefundAmount = 0,
                        TheoreticalCash = model.ActualCash,
                        ActualCash = 0,
                        IsPasswordConfirmed = false,
                        HandoverType = CashHandoverConstants.HandoverTypeMidShift,
                        Status = CashHandoverConstants.ActiveStatus,
                        OpenedAt = DateTime.Now
                    };
                    await _cashHandoverRepository.AddHandoverAsync(nextHandover);
                    await _cashHandoverRepository.SyncAttendanceOnOpenShiftAsync(model.IncomingCashierId.Value, nextShift.ShiftId, handover.HandoverDate);
                }
            }

            var discrepancyText = discrepancy >= 0 ? $"+{discrepancy:N0} đ" : $"{discrepancy:N0} đ";
            return OperationResult.Ok($"Bàn giao ca thành công. Chênh lệch: {discrepancyText}");
        }

        // =========================================================
        //  3. ĐÓNG CA CUỐI NGÀY (Ca đêm / Chốt sổ ngày)
        // =========================================================

        public async Task<CloseShiftViewModel?> GetCloseShiftModelAsync(int cashierId)
        {
            var today = DateTime.Today;

            var activeHandover = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today);
            if (activeHandover == null)
            {
                var cashier = await _cashHandoverRepository.GetEmployeeByIdAsync(cashierId);
                if (cashier != null && !string.IsNullOrEmpty(cashier.BranchId))
                {
                    activeHandover = await _cashHandoverRepository.GetCurrentActiveHandoverForBranchAsync(cashier.BranchId, today);
                }
            }

            if (activeHandover == null)
            {
                return null;
            }

            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                activeHandover.BranchId, activeHandover.OutgoingCashierId, activeHandover.OpenedAt, null);

            return new CloseShiftViewModel
            {
                HandoverId = activeHandover.HandoverId,
                OutgoingCashierId = activeHandover.OutgoingCashierId,
                BranchId = activeHandover.BranchId,
                CashierName = activeHandover.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                ShiftName = activeHandover.FixedShift?.ShiftName ?? "-",
                ShiftTimeRange = activeHandover.FixedShift != null ? $"{activeHandover.FixedShift.StartTime:hh\\:mm} – {activeHandover.FixedShift.EndTime:hh\\:mm}" : "-",
                HandoverDate = activeHandover.HandoverDate,
                OpenedAt = activeHandover.OpenedAt,
                InitialCash = activeHandover.InitialCash,
                MachineCashRevenue = cashRev,
                BankTransferRevenue = bankRev,
                CashRefundAmount = cashRefunds,
                RetainedCashForTomorrow = activeHandover.InitialCash
            };
        }

        public async Task<OperationResult> CloseShiftOfDayAsync(CloseShiftViewModel model)
        {
            var handover = await _cashHandoverRepository.GetHandoverByIdAsync(model.HandoverId);
            if (handover == null || handover.Status != CashHandoverConstants.ActiveStatus)
            {
                return OperationResult.Fail("Không tìm thấy ca làm việc đang mở để đóng cuối ngày.");
            }

            var employee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.OutgoingCashierId);
            if (employee == null)
            {
                return OperationResult.Fail("Không tìm thấy thông tin nhân viên đóng ca.");
            }

            if (!DAT_PasswordHasher.VerifyPassword(model.Password, employee.Password ?? ""))
            {
                return OperationResult.Fail("Mật khẩu xác nhận đóng ca không chính xác.");
            }

            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                handover.BranchId, handover.OutgoingCashierId, handover.OpenedAt, DateTime.Now);

            var theoretical = handover.InitialCash + cashRev - cashRefunds;
            var discrepancy = model.ActualCash - theoretical;

            if (discrepancy != 0 && string.IsNullOrWhiteSpace(model.Notes))
            {
                return OperationResult.Fail("Tiền két cuối ngày bị chênh lệch. Vui lòng nhập lý do giải trình!");
            }

            handover.IncomingCashierId = model.OutgoingCashierId;
            handover.MachineCashRevenue = cashRev;
            handover.BankTransferRevenue = bankRev;
            handover.CashRefundAmount = cashRefunds;
            handover.ActualCash = model.ActualCash;
            handover.TheoreticalCash = theoretical;
            handover.Notes = $"[ĐÓNG CA CUỐI NGÀY - Để lại két: {model.RetainedCashForTomorrow:N0}đ, Nộp két tổng: {model.DepositedCashAmount:N0}đ] " + (model.Notes ?? "");
            handover.IsPasswordConfirmed = true;
            handover.HandoverType = CashHandoverConstants.HandoverTypeLastShift;
            handover.Status = CashHandoverConstants.ClosedStatus;
            handover.ClosedAt = DateTime.Now;

            await _cashHandoverRepository.UpdateHandoverAsync(handover);
            await _cashHandoverRepository.SyncAttendanceOnCloseShiftAsync(handover.OutgoingCashierId, handover.ShiftId, handover.HandoverDate);

            var discrepancyText = discrepancy >= 0 ? $"+{discrepancy:N0} đ" : $"{discrepancy:N0} đ";
            return OperationResult.Ok($"Đã đóng ca cuối ngày thành công. Chốt doanh thu ngày hoàn tất. Chênh lệch: {discrepancyText}");
        }

        // =========================================================
        //  4. BÀN GIAO ĐỘT XUẤT GIỮA CA (Ốm / Việc khẩn cấp)
        // =========================================================

        public async Task<HandoverViewModel?> GetEmergencyHandoverModelAsync(int cashierId)
        {
            var model = await GetHandoverModelAsync(cashierId);
            if (model != null)
            {
                model.IsEmergencyHandover = true;
            }
            return model;
        }

        public async Task<OperationResult> EmergencyHandoverAsync(HandoverViewModel model)
        {
            var handover = await _cashHandoverRepository.GetHandoverByIdAsync(model.HandoverId);
            if (handover == null || handover.Status != CashHandoverConstants.ActiveStatus)
            {
                return OperationResult.Fail("Không tìm thấy ca làm việc đang mở.");
            }

            if (!model.IncomingCashierId.HasValue || model.IncomingCashierId <= 0)
            {
                return OperationResult.Fail("Vui lòng chọn nhân viên tiếp nhận ủy quyền ca.");
            }

            var incomingEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.IncomingCashierId.Value);
            if (incomingEmployee == null)
            {
                return OperationResult.Fail("Không tìm thấy thông tin nhân viên tiếp nhận ủy quyền.");
            }

            if (!DAT_PasswordHasher.VerifyPassword(model.IncomingPassword, incomingEmployee.Password ?? ""))
            {
                return OperationResult.Fail("Mật khẩu xác nhận của người tiếp nhận không chính xác.");
            }

            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                handover.BranchId, handover.OutgoingCashierId, handover.OpenedAt, DateTime.Now);

            var theoretical = handover.InitialCash + cashRev - cashRefunds;
            var discrepancy = model.ActualCash - theoretical;

            string emergencyNote = $"[BÀN GIAO ĐỘT XUẤT GIỮA CA] Lý do: {model.EmergencyReason ?? "Nhân viên ốm/nghỉ đột xuất"}. " + (model.Notes ?? "");

            handover.IncomingCashierId = model.IncomingCashierId.Value;
            handover.MachineCashRevenue = cashRev;
            handover.BankTransferRevenue = bankRev;
            handover.CashRefundAmount = cashRefunds;
            handover.ActualCash = model.ActualCash;
            handover.TheoreticalCash = theoretical;
            handover.Notes = emergencyNote;
            handover.EmergencyReason = model.EmergencyReason;
            handover.DelivererName = model.DelivererName;
            handover.IsPasswordConfirmed = true;
            handover.HandoverType = CashHandoverConstants.HandoverTypeEmergency;
            handover.Status = CashHandoverConstants.ClosedStatus;
            handover.ClosedAt = DateTime.Now;

            await _cashHandoverRepository.UpdateHandoverAsync(handover);
            await _cashHandoverRepository.SyncAttendanceOnCloseShiftAsync(handover.OutgoingCashierId, handover.ShiftId, handover.HandoverDate);

            // Mở phiên tiếp tục cho người nhận ủy quyền trong cùng ca
            var continueHandover = new CashHandover
            {
                BranchId = handover.BranchId,
                HandoverDate = handover.HandoverDate,
                ShiftId = handover.ShiftId,
                OutgoingCashierId = model.IncomingCashierId.Value,
                IncomingCashierId = model.IncomingCashierId.Value,
                InitialCash = model.ActualCash,
                MachineCashRevenue = 0,
                BankTransferRevenue = 0,
                CashRefundAmount = 0,
                TheoreticalCash = model.ActualCash,
                ActualCash = 0,
                IsPasswordConfirmed = false,
                HandoverType = CashHandoverConstants.HandoverTypeEmergency,
                Notes = $"Tiếp quản ca từ {handover.OutgoingCashier?.FullName} do nghỉ đột xuất.",
                Status = CashHandoverConstants.ActiveStatus,
                OpenedAt = DateTime.Now
            };

            await _cashHandoverRepository.AddHandoverAsync(continueHandover);
            await _cashHandoverRepository.SyncAttendanceOnOpenShiftAsync(model.IncomingCashierId.Value, handover.ShiftId, handover.HandoverDate);

            return OperationResult.Ok($"Đã xử lý bàn giao đột xuất thành công. Ca đã được chuyển giao cho {incomingEmployee.FullName}.");
        }

        // =========================================================
        //  5. KIỂM TRA ĐIỀU KIỆN MỞ BÁN HÀNG CHO CASHIER
        // =========================================================

        public async Task<(bool isEligible, string reasonCode, string message, int? activeShiftId, string? shiftPhase)> CheckCashierSaleEligibilityAsync(int cashierId, string branchId)
        {
            return await _cashHandoverRepository.CheckCashierSaleEligibilityAsync(cashierId, branchId);
        }

        // =========================================================
        //  6. LỊCH SỬ GIAO CA
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
                    var theoretical = ch.InitialCash + ch.MachineCashRevenue - ch.CashRefundAmount;
                    var discrepancy = ch.ActualCash - theoretical;

                    string sessionType = ch.HandoverType switch
                    {
                        CashHandoverConstants.HandoverTypeFirstShift => "Mở ca đầu ngày",
                        CashHandoverConstants.HandoverTypeLastShift => "Đóng ca cuối ngày",
                        CashHandoverConstants.HandoverTypeEmergency => "Bàn giao đột xuất",
                        "SelfHandover" => "Giao ca liên tiếp (Cùng NV)",
                        _ => ch.Status == CashHandoverConstants.ActiveStatus ? "Mở ca" : "Bàn giao ca"
                    };

                    return new HandoverHistoryItemViewModel
                    {
                        HandoverId = ch.HandoverId,
                        HandoverDate = ch.HandoverDate.ToString("dd/MM/yyyy"),
                        ShiftName = ch.FixedShift?.ShiftName ?? "-",
                        OutgoingCashierName = ch.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                        IncomingCashierName = ch.IncomingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                        InitialCash = ch.InitialCash.ToString("N0") + " đ",
                        MachineCashRevenue = ch.MachineCashRevenue.ToString("N0") + " đ",
                        BankTransferRevenue = ch.BankTransferRevenue.ToString("N0") + " đ",
                        CashRefundAmount = ch.CashRefundAmount.ToString("N0") + " đ",
                        TheoreticalCash = theoretical.ToString("N0") + " đ",
                        ActualCash = ch.ActualCash.ToString("N0") + " đ",
                        Discrepancy = (discrepancy >= 0 ? "+" : "") + discrepancy.ToString("N0") + " đ",
                        Status = ch.Status,
                        OpenedAt = ch.OpenedAt.ToString("HH:mm dd/MM/yyyy"),
                        ClosedAt = ch.ClosedAt?.ToString("HH:mm dd/MM/yyyy") ?? "-",
                        Notes = ch.Notes,
                        DelivererName = ch.DelivererName,
                        SessionType = sessionType,
                        HandoverType = ch.HandoverType ?? "Normal",
                        EmergencyReason = ch.EmergencyReason
                    };
                }).ToList()
            };
        }

        // =========================================================
        //  7. DANH SÁCH THU NGÂN
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
                    CashierId = emp.EmployeeId,
                    CashierName = emp.FullName ?? emp.Username ?? "–",
                    Username = emp.Username ?? "–",
                    BranchId = branchId,
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
