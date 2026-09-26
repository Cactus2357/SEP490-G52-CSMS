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
        private readonly IShiftChangeRepository? _shiftChangeRepository;
        private readonly INotificationService? _notificationService;

        public CashHandoverService(
            ICashHandoverRepository cashHandoverRepository,
            IShiftChangeRepository? shiftChangeRepository = null,
            INotificationService? notificationService = null)
        {
            _cashHandoverRepository = cashHandoverRepository;
            _shiftChangeRepository = shiftChangeRepository;
            _notificationService = notificationService;
        }

        private static string FormatShiftTimeRange(FixedShift? shift)
        {
            if (shift == null) return "-";
            var endStr = (shift.EndTime.Hours == 23 && shift.EndTime.Minutes >= 59) ? "24:00" : shift.EndTime.ToString(@"hh\:mm");
            return $"{shift.StartTime:hh\\:mm} – {endStr}";
        }

        // =========================================================
        //  XÁC ĐỊNH LOẠI CA HIỆN TẠI
        // =========================================================

        public async Task<string> DetermineCurrentShiftPhaseAsync(int cashierId, string branchId)
        {
            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync(branchId);
            var today = DateTime.UtcNow.ToVietnamTime().Date;
            var activeHandover = await _cashHandoverRepository.GetActiveHandoverAsync(cashierId, today)
                              ?? await _cashHandoverRepository.GetCurrentActiveHandoverForBranchAsync(branchId, today);

            if (activeHandover != null)
            {
                return await _cashHandoverRepository.GetShiftPhaseAsync(activeHandover.ShiftId, branchId, today);
            }

            var lastHandover = await _cashHandoverRepository.GetLastClosedHandoverForBranchAsync(branchId);
            if (lastHandover == null || lastHandover.HandoverDate.Date < today.Date)
            {
                return CashHandoverConstants.HandoverTypeFirstShift;
            }

            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(cashierId, today);
            if (roster != null)
            {
                return await _cashHandoverRepository.GetShiftPhaseAsync(roster.ShiftId, branchId, today);
            }

            return CashHandoverConstants.HandoverTypeFirstShift;
        }

        // =========================================================
        //  1. MỞ CA ĐẦU NGÀY (UC11)
        // =========================================================

        public async Task<OpenShiftViewModel?> GetOpenShiftModelAsync(int cashierId)
        {
            var today = DateTime.UtcNow.ToVietnamTime().Date;

            var cashierEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(cashierId);
            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(cashierId, today);
            var branchId = roster?.BranchId ?? cashierEmployee?.BranchId ?? "CN001";

            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync(branchId);

            // Chỉ thu ngân có lịch làm việc hôm nay mới được mở ca
            if (roster == null)
            {
                return null;
            }

            // 1. Nếu ngày hôm nay tại chi nhánh đã thực hiện Đóng ca cuối ngày -> Không cho mở ca nữa
            var isDayClosed = await _cashHandoverRepository.IsDayClosedAsync(branchId, today);
            if (isDayClosed)
            {
                return null; // Đã đóng ca cuối ngày hôm nay
            }

            // 2. Nếu ca làm việc cụ thể này hôm nay đã bị đóng (Closed) -> Không cho mở lại
            var specificClosed = await _cashHandoverRepository.GetClosedHandoverByShiftAsync(branchId, roster.ShiftId, today);
            if (specificClosed != null)
            {
                return null; // Ca này hôm nay đã chốt đóng
            }

            // 3. Kiểm tra đã có ca Active hôm nay tại chi nhánh chưa
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
                ? FormatShiftTimeRange(roster.FixedShift)
                : (firstShift != null ? FormatShiftTimeRange(firstShift) : "-");

            bool isTimeToOpen = true;
            if (roster.FixedShift != null)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var thirtyMinutes = TimeSpan.FromMinutes(30);

                if (roster.FixedShift.StartTime <= roster.FixedShift.EndTime)
                {
                    isTimeToOpen = (currentTime >= roster.FixedShift.StartTime.Subtract(thirtyMinutes) && currentTime <= roster.FixedShift.EndTime);
                }
                else
                {
                    isTimeToOpen = (currentTime >= roster.FixedShift.StartTime.Subtract(thirtyMinutes));
                }
            }

            bool isFirstShift = (lastHandover == null || lastHandover.HandoverDate.Date < today.Date);
            decimal initialCash = 0;
            if (lastHandover != null)
            {
                if (isFirstShift && lastHandover.RetainedCash > 0)
                {
                    initialCash = lastHandover.RetainedCash;
                }
                else
                {
                    initialCash = lastHandover.ActualCash;
                }
            }

            string previousInitialCashText = "-";
            if (lastHandover != null)
            {
                if (isFirstShift && lastHandover.RetainedCash > 0)
                {
                    previousInitialCashText = lastHandover.RetainedCash.ToString("N0") + " ₫ (Tiền lưu két từ ca trước)";
                }
                else
                {
                    previousInitialCashText = lastHandover.ActualCash.ToString("N0") + " ₫";
                }
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
                PreviousInitialCash = previousInitialCashText,
                PreviousApproverName = lastHandover?.IncomingCashier?.FullName ?? "-",
                DelivererName = lastHandover?.OutgoingCashier?.FullName ?? "",
                InitialCash = initialCash,
                IsFirstShift = isFirstShift,
                IsTimeToOpen = isTimeToOpen
            };

            return model;
        }

        public async Task<OperationResult> OpenShiftAsync(OpenShiftViewModel model)
        {
            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync(model.BranchId);
            var today = DateTime.UtcNow.ToVietnamTime().Date;

            var cashierEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.CashierId);
            var roster = await _cashHandoverRepository.GetCurrentRosterAsync(model.CashierId, today);
            var branchId = roster?.BranchId ?? cashierEmployee?.BranchId ?? model.BranchId;

            if (roster == null)
            {
                return OperationResult.Fail("Bạn không có lịch phân công ca làm việc tại chi nhánh hôm nay. Vui lòng kiểm tra lại Lịch làm việc.");
            }

            // 1. Kiểm tra ngày hôm nay tại chi nhánh đã thực hiện Đóng ca cuối ngày chưa
            var isDayClosed = await _cashHandoverRepository.IsDayClosedAsync(branchId, today);
            if (isDayClosed)
            {
                return OperationResult.Fail("Chi nhánh đã thực hiện Đóng ca cuối ngày hôm nay. Không thể mở thêm ca mới.");
            }

            // 2. Kiểm tra ca làm việc cụ thể này hôm nay đã từng bị đóng (Closed) chưa
            var specificClosed = await _cashHandoverRepository.GetClosedHandoverByShiftAsync(branchId, model.ShiftId, today);
            if (specificClosed != null)
            {
                return OperationResult.Fail($"Ca làm việc {roster.FixedShift?.ShiftName ?? "này"} hôm nay đã được chốt đóng ca. Không thể mở lại.");
            }

            // 3. Kiểm tra xem ca làm việc này đã có ai mở (Active) chưa
            var existingShiftActive = await _cashHandoverRepository.GetActiveHandoverByShiftAsync(branchId, model.ShiftId, today);
            if (existingShiftActive != null)
            {
                return OperationResult.Fail("Ca làm việc này tại chi nhánh đã được mở. Không thể tạo ca trùng lặp.");
            }

            var existing = await _cashHandoverRepository.GetActiveHandoverAsync(model.CashierId, today);
            if (existing != null)
            {
                return OperationResult.Fail("Ca làm việc hôm nay của bạn đã được mở. Vui lòng chuyển sang Giao ca.");
            }

            if (string.IsNullOrWhiteSpace(model.DelivererName))
            {
                return OperationResult.Fail("Vui lòng nhập thông tin người giao tiền khi mở ca.");
            }

            // 4. KIỂM TRA THỜI GIAN CA LÀM VIỆC (CHƯA ĐẾN GIỜ HOẶC ĐÃ QUÁ GIỜ)
            var currentTime = DateTime.Now.TimeOfDay;
            var thirtyMinutes = TimeSpan.FromMinutes(30);
            if (roster.FixedShift != null)
            {
                if (roster.FixedShift.StartTime <= roster.FixedShift.EndTime)
                {
                    if (currentTime < roster.FixedShift.StartTime.Subtract(thirtyMinutes))
                    {
                        return OperationResult.Fail($"Chưa đến giờ mở ca {roster.FixedShift.ShiftName}. Bạn chỉ có thể mở ca sớm tối đa 30 phút trước khi ca làm bắt đầu.");
                    }
                    if (currentTime > roster.FixedShift.EndTime)
                    {
                        return OperationResult.Fail($"Ca làm việc {roster.FixedShift.ShiftName} ({roster.FixedShift.StartTime:hh\\:mm} – {roster.FixedShift.EndTime:hh\\:mm}) đã hết thời gian làm việc hôm nay. Không thể mở ca đã quá giờ.");
                    }
                }
                else
                {
                    if (currentTime < roster.FixedShift.StartTime.Subtract(thirtyMinutes) && currentTime > roster.FixedShift.EndTime)
                    {
                        return OperationResult.Fail($"Chưa đến giờ mở ca {roster.FixedShift.ShiftName}. Vui lòng quay lại vào ca làm việc.");
                    }
                }
            }

            var lastHandover = string.IsNullOrEmpty(branchId)
                ? null
                : await _cashHandoverRepository.GetLastClosedHandoverForBranchAsync(branchId);

            decimal initialCash = model.InitialCash;
            bool isFirstShift = (lastHandover == null || lastHandover.HandoverDate.Date < today.Date);
            if (!isFirstShift && lastHandover != null)
            {
                initialCash = lastHandover.ActualCash;
            }
            else if (isFirstShift && lastHandover != null && initialCash <= 0)
            {
                initialCash = lastHandover.RetainedCash > 0 ? lastHandover.RetainedCash : lastHandover.ActualCash;
            }

            string handoverType = isFirstShift ? CashHandoverConstants.HandoverTypeFirstShift : CashHandoverConstants.HandoverTypeMidShift;

            var handover = new CashHandover
            {
                BranchId = branchId,
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
                DelivererName = model.DelivererName,
                IsPasswordConfirmed = false,
                HandoverType = handoverType,
                Status = CashHandoverConstants.ActiveStatus,
                OpenedAt = DateTime.UtcNow,
            };

            await _cashHandoverRepository.AddHandoverAsync(handover);
            await _cashHandoverRepository.SyncAttendanceOnOpenShiftAsync(model.CashierId, model.ShiftId, today);

            return OperationResult.Ok($"Đã mở ca ({roster.FixedShift?.ShiftName ?? "mới"}) thành công. Tiền đầu ca: {initialCash:N0} đ");
        }

        // =========================================================
        //  2. BÀN GIAO CA GIỮA NGÀY (UC12)
        // =========================================================

        public async Task<HandoverViewModel?> GetHandoverModelAsync(int cashierId)
        {
            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync();
            var today = DateTime.UtcNow.ToVietnamTime().Date;

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

            var cashiers = await _cashHandoverRepository.GetEligibleHandoverCashiersAsync(
                activeHandover.BranchId, today, activeHandover.ShiftId, activeHandover.OutgoingCashierId);

            var currentTime = DateTime.Now.TimeOfDay;
            var nextCashierId = await _cashHandoverRepository.GetNextCashierForHandoverAsync(activeHandover.BranchId, today, currentTime);

            bool isSelfHandover = (nextCashierId.HasValue && nextCashierId.Value == activeHandover.OutgoingCashierId);

            var theoretical = activeHandover.InitialCash + cashRev - cashRefunds;

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
                ActualCash = theoretical,
                TargetShiftName = nextShift.ShiftName,
                TargetShiftTimeRange = FormatShiftTimeRange(nextShift),
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

            if (model.OutgoingCashierId > 0 && handover.OutgoingCashierId != model.OutgoingCashierId)
            {
                return OperationResult.Fail("Chỉ thu ngân đang phụ trách ca trực này mới được thực hiện thao tác bàn giao ca.");
            }

            var nextShift = await _cashHandoverRepository.GetNextFixedShiftAsync(handover.ShiftId);
            if (nextShift == null)
            {
                return OperationResult.Fail("Hiện tại là ca cuối cùng trong ngày (không có ca tiếp theo). Không thể thực hiện bàn giao, vui lòng sử dụng chức năng Đóng ca cuối ngày.");
            }

            // Kiểm tra thời gian bàn giao ca cho thu ngân khác (sớm tối đa 1h30p = 90 phút)
            if (!model.IsEmergencyHandover && handover.FixedShift != null)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var ninetyMinutes = TimeSpan.FromMinutes(90);
                var earlyHandoverTime = handover.FixedShift.EndTime.Subtract(ninetyMinutes);
                if (earlyHandoverTime > TimeSpan.Zero && currentTime < earlyHandoverTime)
                {
                    return OperationResult.Fail($"Chưa đến giờ bàn giao ca. Bạn chỉ có thể thực hiện bàn giao ca cho thu ngân khác sớm tối đa 1 tiếng 30 phút trước khi ca kết thúc (từ {earlyHandoverTime:hh\\:mm}).");
                }
            }

            if (!model.IncomingCashierId.HasValue || model.IncomingCashierId <= 0)
            {
                return OperationResult.Fail("Vui lòng chọn người nhận ca.");
            }

            var eligibleCashiers = await _cashHandoverRepository.GetEligibleHandoverCashiersAsync(
                handover.BranchId, handover.HandoverDate, handover.ShiftId, handover.OutgoingCashierId);
            if (!eligibleCashiers.Any(c => c.EmployeeId == model.IncomingCashierId.Value))
            {
                return OperationResult.Fail("Nhân viên được chọn nhận ca không hợp lệ. Chỉ có thể bàn giao cho thu ngân có ca làm việc tiếp theo trong ngày hoặc chính bản thân.");
            }

            var incomingEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.IncomingCashierId.Value);
            if (incomingEmployee == null)
            {
                return OperationResult.Fail("Không tìm thấy thông tin nhân viên nhận ca.");
            }

            if (incomingEmployee.Role != CashHandoverConstants.CashierRole && incomingEmployee.Role != "Cashier")
            {
                return OperationResult.Fail($"Nhân viên {incomingEmployee.FullName} không có vai trò Thu ngân (Vai trò: {incomingEmployee.Role}). Chỉ có thể bàn giao cho nhân viên Thu ngân.");
            }

            // Xác minh mật khẩu người nhận ca
            if (!DAT_PasswordHasher.VerifyPassword(model.IncomingPassword, incomingEmployee.Password ?? ""))
            {
                return OperationResult.Fail("Mật khẩu xác nhận không chính xác. Vui lòng thử lại.");
            }

            // Tính toán doanh thu và hoàn tiền chính xác đến thời điểm đóng ca
            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                handover.BranchId, handover.OutgoingCashierId, handover.OpenedAt, DateTime.UtcNow);

            var theoretical = handover.InitialCash + cashRev;
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
            handover.ClosedAt = DateTime.UtcNow;

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
                        OpenedAt = DateTime.UtcNow
                    };
                    await _cashHandoverRepository.AddHandoverAsync(nextHandover);
                    await _cashHandoverRepository.SyncAttendanceOnOpenShiftAsync(model.IncomingCashierId.Value, nextShift.ShiftId, handover.HandoverDate);
                }
                else
                {
                    nextShiftActive.InitialCash = model.ActualCash;
                    nextShiftActive.TheoreticalCash = model.ActualCash;
                    await _cashHandoverRepository.UpdateHandoverAsync(nextShiftActive);
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
            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync();
            var today = DateTime.UtcNow.ToVietnamTime().Date;

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

            var theoretical = activeHandover.InitialCash + cashRev;

            return new CloseShiftViewModel
            {
                HandoverId = activeHandover.HandoverId,
                OutgoingCashierId = activeHandover.OutgoingCashierId,
                BranchId = activeHandover.BranchId,
                CashierName = activeHandover.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                ShiftName = activeHandover.FixedShift?.ShiftName ?? "-",
                ShiftTimeRange = FormatShiftTimeRange(activeHandover.FixedShift),
                HandoverDate = activeHandover.HandoverDate,
                OpenedAt = activeHandover.OpenedAt,
                InitialCash = activeHandover.InitialCash,
                MachineCashRevenue = cashRev,
                BankTransferRevenue = bankRev,
                CashRefundAmount = 0,
                ActualCash = theoretical,
                RetainedCashForTomorrow = 0,
                ReceiverName = string.Empty,
                Notes = string.Empty
            };
        }

        public async Task<OperationResult> CloseShiftOfDayAsync(CloseShiftViewModel model)
        {
            var handover = await _cashHandoverRepository.GetHandoverByIdAsync(model.HandoverId);
            if (handover == null || handover.Status != CashHandoverConstants.ActiveStatus)
            {
                return OperationResult.Fail("Không tìm thấy ca làm việc đang mở để đóng cuối ngày.");
            }

            if (model.OutgoingCashierId > 0 && handover.OutgoingCashierId != model.OutgoingCashierId)
            {
                return OperationResult.Fail("Chỉ thu ngân đang phụ trách ca trực này mới được thực hiện thao tác đóng ca.");
            }

            if (string.IsNullOrWhiteSpace(model.ReceiverName))
            {
                return OperationResult.Fail("Vui lòng nhập thông tin người nhận tiền đóng ca.");
            }

            if (string.IsNullOrWhiteSpace(model.Notes))
            {
                return OperationResult.Fail("Vui lòng nhập lý do / ghi chú chốt két và chênh lệch!");
            }

            // Kiểm tra thời gian đóng ca cuối ngày (Cho phép sau khi ca làm việc cuối bắt đầu 30 phút)
            if (handover.FixedShift != null)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var allowedCloseTime = handover.FixedShift.StartTime.Add(TimeSpan.FromMinutes(30));
                bool isPastShiftDay = DateTime.Today > handover.HandoverDate.Date;
                bool isPastMidnight = (DateTime.Today == handover.HandoverDate.Date.AddDays(1) || currentTime < TimeSpan.FromHours(6));

                if (!isPastShiftDay && !isPastMidnight && currentTime < allowedCloseTime)
                {
                    return OperationResult.Fail($"Chưa đến giờ đóng ca cuối ngày. Chỉ được phép thực hiện Đóng ca cuối ngày sau khi ca làm việc cuối bắt đầu 30 phút (từ {allowedCloseTime:hh\\:mm}).");
                }
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
                handover.BranchId, handover.OutgoingCashierId, handover.OpenedAt, DateTime.UtcNow);

            var theoretical = handover.InitialCash + cashRev;
            var discrepancy = model.ActualCash - theoretical;

            handover.IncomingCashierId = model.OutgoingCashierId;
            handover.DelivererName = model.ReceiverName;
            handover.MachineCashRevenue = cashRev;
            handover.BankTransferRevenue = bankRev;
            handover.CashRefundAmount = cashRefunds;
            handover.ActualCash = model.ActualCash;
            handover.RetainedCash = model.RetainedCashForTomorrow;
            handover.DepositedCash = model.DepositedCashAmount;
            handover.TheoreticalCash = theoretical;
            handover.Notes = $"[ĐÓNG CA CUỐI NGÀY - Tổng két chốt: {model.ActualCash:N0}đ, Để lại két ngày mai: {model.RetainedCashForTomorrow:N0}đ, Nộp két tổng: {model.DepositedCashAmount:N0}đ" + (!string.IsNullOrWhiteSpace(model.ReceiverName) ? $", Người nhận tiền: {model.ReceiverName}" : "") + "] " + (model.Notes ?? "");
            handover.IsPasswordConfirmed = true;
            handover.HandoverType = CashHandoverConstants.HandoverTypeLastShift;
            handover.Status = CashHandoverConstants.ClosedStatus;
            handover.ClosedAt = DateTime.UtcNow;

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
            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync();
            var today = DateTime.UtcNow.ToVietnamTime().Date;
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

            // Kiểm tra ca tiếp theo nếu có. Nếu là ca cuối cùng trong ngày, target chính là ca hiện tại
            var nextShift = await _cashHandoverRepository.GetNextFixedShiftAsync(activeHandover.ShiftId);
            string targetShiftName = nextShift?.ShiftName ?? (activeHandover.FixedShift?.ShiftName ?? "Ca hiện tại");
            string targetShiftTimeRange = nextShift != null ? FormatShiftTimeRange(nextShift) : FormatShiftTimeRange(activeHandover.FixedShift);

            // Tính doanh thu và hoàn tiền mặt thực tế phát sinh trong ca
            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                activeHandover.BranchId, activeHandover.OutgoingCashierId, activeHandover.OpenedAt, null);

            // Thu ngân có thể nhận bàn giao đột xuất: tất cả thu ngân hoạt động trong chi nhánh
            var branchCashiers = await _cashHandoverRepository.GetCashiersInBranchAsync(activeHandover.BranchId);
            if (!branchCashiers.Any())
            {
                branchCashiers = await _cashHandoverRepository.GetEligibleHandoverCashiersAsync(
                    activeHandover.BranchId, today, activeHandover.ShiftId, activeHandover.OutgoingCashierId);
            }

            var currentTime = DateTime.Now.TimeOfDay;
            var nextCashierId = await _cashHandoverRepository.GetNextCashierForHandoverAsync(activeHandover.BranchId, today, currentTime);

            var otherCashier = branchCashiers.FirstOrDefault(c => c.EmployeeId != activeHandover.OutgoingCashierId);
            var defaultIncomingId = otherCashier?.EmployeeId ?? nextCashierId ?? activeHandover.OutgoingCashierId;

            var theoretical = activeHandover.InitialCash + cashRev - cashRefunds;

            return new HandoverViewModel
            {
                HandoverId = activeHandover.HandoverId,
                OutgoingCashierId = activeHandover.OutgoingCashierId,
                BranchId = activeHandover.BranchId,
                ShiftId = activeHandover.ShiftId,
                HandoverDate = activeHandover.HandoverDate,
                OutgoingCashierName = activeHandover.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel,
                ShiftName = activeHandover.FixedShift?.ShiftName ?? "-",
                OpenedAt = activeHandover.OpenedAt,
                InitialCash = activeHandover.InitialCash,
                MachineCashRevenue = cashRev,
                BankTransferRevenue = bankRev,
                CashRefundAmount = cashRefunds,
                ActualCash = theoretical,
                TargetShiftName = targetShiftName,
                TargetShiftTimeRange = targetShiftTimeRange,
                IsSelfHandover = false,
                IsEmergencyHandover = true,
                IncomingCashierId = defaultIncomingId,
                DelivererName = activeHandover.DelivererName,
                IncomingCashiers = branchCashiers.Select(c => new CashierOption
                {
                    CashierId = c.EmployeeId,
                    CashierName = c.FullName ?? c.Username ?? CashHandoverConstants.UnassignedCashierLabel
                }).ToList(),
            };
        }

        public async Task<OperationResult> EmergencyHandoverAsync(HandoverViewModel model)
        {
            var handover = await _cashHandoverRepository.GetHandoverByIdAsync(model.HandoverId);
            if (handover == null || handover.Status != CashHandoverConstants.ActiveStatus)
            {
                return OperationResult.Fail("Không tìm thấy ca làm việc đang mở.");
            }

            if (model.OutgoingCashierId > 0 && handover.OutgoingCashierId != model.OutgoingCashierId)
            {
                return OperationResult.Fail("Chỉ thu ngân đang phụ trách ca trực này mới được thực hiện bàn giao đột xuất.");
            }

            if (!model.IncomingCashierId.HasValue || model.IncomingCashierId <= 0)
            {
                return OperationResult.Fail("Vui lòng chọn nhân viên tiếp nhận ủy quyền ca.");
            }

            var eligibleCashiers = await _cashHandoverRepository.GetCashiersInBranchAsync(handover.BranchId);
            if (!eligibleCashiers.Any(c => c.EmployeeId == model.IncomingCashierId.Value))
            {
                var rosterCashiers = await _cashHandoverRepository.GetEligibleHandoverCashiersAsync(
                    handover.BranchId, handover.HandoverDate, handover.ShiftId, handover.OutgoingCashierId);
                if (!rosterCashiers.Any(c => c.EmployeeId == model.IncomingCashierId.Value))
                {
                    return OperationResult.Fail("Nhân viên được chọn nhận ủy quyền không hợp lệ. Chỉ có thể bàn giao cho thu ngân thuộc chi nhánh.");
                }
            }

            var incomingEmployee = await _cashHandoverRepository.GetEmployeeByIdAsync(model.IncomingCashierId.Value);
            if (incomingEmployee == null)
            {
                return OperationResult.Fail("Không tìm thấy thông tin nhân viên tiếp nhận ủy quyền.");
            }

            if (incomingEmployee.Role != CashHandoverConstants.CashierRole && incomingEmployee.Role != "Cashier" && incomingEmployee.Role != "BranchManager")
            {
                return OperationResult.Fail($"Nhân viên {incomingEmployee.FullName} không có vai trò Thu ngân hoặc Quản lý chi nhánh (Vai trò: {incomingEmployee.Role}). Chỉ có thể bàn giao cho nhân viên Thu ngân hoặc Quản lý chi nhánh.");
            }

            if (!DAT_PasswordHasher.VerifyPassword(model.IncomingPassword, incomingEmployee.Password ?? ""))
            {
                return OperationResult.Fail("Mật khẩu xác nhận của người tiếp nhận không chính xác.");
            }

            var (cashRev, bankRev, cashRefunds) = await _cashHandoverRepository.GetShiftSalesStatsAsync(
                handover.BranchId, handover.OutgoingCashierId, handover.OpenedAt, DateTime.UtcNow);

            var theoretical = handover.InitialCash + cashRev - cashRefunds;

            var handoverTime = DateTime.Now;
            var outgoingEmployee = handover.OutgoingCashier ?? await _cashHandoverRepository.GetEmployeeByIdAsync(handover.OutgoingCashierId);
            string outgoingName = outgoingEmployee?.FullName ?? outgoingEmployee?.Username ?? model.OutgoingCashierName ?? "Thu ngân";
            string incomingName = incomingEmployee.FullName ?? incomingEmployee.Username ?? "Thu ngân tiếp nhận";
            string shiftDisplayName = handover.FixedShift?.ShiftName ?? $"Ca {handover.ShiftId}";

            string emergencyNote = $"[BÀN GIAO ĐỘT XUẤT] Bàn giao lúc: {handoverTime:HH:mm dd/MM/yyyy}. Thu ngân bàn giao: {outgoingName} -> Thu ngân nhận: {incomingName}. Lý do: {model.EmergencyReason ?? "Nghỉ đột xuất"}. {(string.IsNullOrWhiteSpace(model.Notes) ? "" : "Ghi chú: " + model.Notes)}";

            // Cập nhật thông tin bàn giao đột xuất vào ca làm việc hiện tại
            handover.IncomingCashierId = model.IncomingCashierId.Value;
            handover.MachineCashRevenue = cashRev;
            handover.BankTransferRevenue = bankRev;
            handover.CashRefundAmount = cashRefunds;
            handover.ActualCash = theoretical;
            handover.TheoreticalCash = theoretical;
            handover.Notes = emergencyNote;
            handover.EmergencyReason = model.EmergencyReason;
            handover.DelivererName = model.DelivererName;
            handover.IsPasswordConfirmed = true;
            handover.HandoverType = CashHandoverConstants.HandoverTypeEmergency;
            // Thu ngân sau sẽ dùng tiếp tài khoản của thu ngân trước để bán hàng, giữ trạng thái ca là Active
            handover.Status = CashHandoverConstants.ActiveStatus;

            await _cashHandoverRepository.UpdateHandoverAsync(handover);

            // Tự động tạo 1 đơn xin đổi ca cho quản lý để ghi lại sự thay đổi ca làm việc của thu ngân ca hôm đó
            var shiftRequest = new ShiftChangeRequest
            {
                RequestingEmployeeId = handover.OutgoingCashierId,
                SubmittedAt = DateTime.UtcNow,
                Status = "Submitted",
                Aspiration = $"Bàn giao ca đột xuất: Thu ngân bàn giao [{outgoingName}] -> Thu ngân nhận ca [{incomingName}]. Ca: {shiftDisplayName} ngày {handover.HandoverDate:dd/MM/yyyy}. Thời gian bàn giao: {handoverTime:HH:mm dd/MM/yyyy}.",
                Reason = $"Bàn giao ca đột xuất giữa ca. Lý do: {model.EmergencyReason ?? "Nghỉ đột xuất"}. {(string.IsNullOrWhiteSpace(model.Notes) ? "" : "Ghi chú: " + model.Notes)}"
            };

            if (_shiftChangeRepository != null)
            {
                await _shiftChangeRepository.AddRequestAsync(shiftRequest);
            }
            else
            {
                await _cashHandoverRepository.AddShiftChangeRequestAsync(shiftRequest);
            }

            // Gửi thông báo đến Quản lý chi nhánh
            if (_notificationService != null)
            {
                try
                {
                    await _notificationService.SendAsync(new NotificationEvent(
                        Title: "Đơn đổi ca đột xuất mới",
                        Message: $"Thu ngân {outgoingName} đã bàn giao ca đột xuất ({shiftDisplayName}) cho {incomingName} lúc {handoverTime:HH:mm dd/MM/yyyy}. Lý do: {model.EmergencyReason ?? "Nghỉ đột xuất"}.",
                        RecipientRole: "BranchManager",
                        BranchId: handover.BranchId,
                        ResourceUrl: "/ShiftChange/Index"
                    ));
                }
                catch
                {
                    // Tránh làm gián đoạn quy trình bàn giao nếu việc gửi thông báo gặp lỗi
                }
            }

            return OperationResult.Ok($"Bàn giao ca đột xuất thành công! Hệ thống đã tự động gửi đơn đổi ca tới Quản lý chi nhánh. Thu ngân tiếp quản ({incomingName}) sẽ tiếp tục sử dụng phiên làm việc hiện tại để bán hàng.");
        }

        public async Task<bool> IsDayClosedAsync(string branchId, DateTime date)
        {
            return await _cashHandoverRepository.IsDayClosedAsync(branchId, date);
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
            await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync(branchId);
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

                    string sessionType;
                    string actionTime;
                    string actionTimeType;
                    string outgoingName;
                    string incomingName;
                    bool hasOutgoing;
                    bool hasIncoming;

                    bool isActive = (ch.Status == CashHandoverConstants.ActiveStatus);

                    if (isActive)
                    {
                        actionTime = ch.OpenedAt.ToVietnamTimeString("HH:mm dd/MM/yyyy");
                        actionTimeType = "Mở ca lúc";
                        hasOutgoing = !string.IsNullOrWhiteSpace(ch.DelivererName);
                        outgoingName = !string.IsNullOrWhiteSpace(ch.DelivererName) ? ch.DelivererName : "-";
                        hasIncoming = true;
                        incomingName = ch.OutgoingCashier?.FullName ?? ch.IncomingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;

                        if (ch.HandoverType == CashHandoverConstants.HandoverTypeFirstShift)
                        {
                            sessionType = "Mở ca đầu ngày";
                        }
                        else if (ch.HandoverType == CashHandoverConstants.HandoverTypeEmergency)
                        {
                            sessionType = "Tiếp quản ca đột xuất (Đang mở)";
                        }
                        else if (ch.HandoverType == "SelfHandover")
                        {
                            sessionType = "Nhận ca liên tiếp (Đang mở)";
                        }
                        else
                        {
                            sessionType = "Tiếp quản ca làm việc (Đang mở)";
                        }
                    }
                    else // Status == Closed
                    {
                        actionTime = (ch.ClosedAt ?? ch.OpenedAt).ToVietnamTimeString("HH:mm dd/MM/yyyy");

                        if (ch.HandoverType == CashHandoverConstants.HandoverTypeLastShift)
                        {
                            sessionType = "Đóng ca cuối ngày";
                            actionTimeType = "Đóng ca lúc";
                            hasOutgoing = true;
                            outgoingName = ch.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                            hasIncoming = !string.IsNullOrWhiteSpace(ch.DelivererName);
                            incomingName = !string.IsNullOrWhiteSpace(ch.DelivererName) ? ch.DelivererName : "-";
                        }
                        else if (ch.HandoverType == CashHandoverConstants.HandoverTypeEmergency)
                        {
                            sessionType = "Bàn giao đột xuất giữa ca";
                            actionTimeType = "Bàn giao lúc";
                            hasOutgoing = true;
                            outgoingName = ch.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                            hasIncoming = true;
                            incomingName = ch.IncomingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                        }
                        else if (ch.HandoverType == "SelfHandover")
                        {
                            sessionType = "Chuyển tiếp ca liên tiếp";
                            actionTimeType = "Bàn giao lúc";
                            hasOutgoing = true;
                            outgoingName = ch.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                            hasIncoming = true;
                            incomingName = ch.IncomingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                        }
                        else
                        {
                            sessionType = "Bàn giao ca giữa ngày";
                            actionTimeType = "Bàn giao lúc";
                            hasOutgoing = true;
                            outgoingName = ch.OutgoingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                            hasIncoming = true;
                            incomingName = ch.IncomingCashier?.FullName ?? CashHandoverConstants.UnassignedCashierLabel;
                        }
                    }

                    return new HandoverHistoryItemViewModel
                    {
                        HandoverId = ch.HandoverId,
                        ShiftName = ch.FixedShift?.ShiftName ?? "-",
                        ShiftTimeRange = ch.FixedShift != null ? FormatShiftTimeRange(ch.FixedShift) : "-",
                        HandoverDate = ch.HandoverDate.ToString("dd/MM/yyyy"),
                        ActionTime = actionTime,
                        ActionTimeType = actionTimeType,
                        HasOutgoing = hasOutgoing,
                        OutgoingCashierName = outgoingName,
                        HasIncoming = hasIncoming,
                        IncomingCashierName = incomingName,
                        InitialCash = ch.InitialCash.ToString("N0") + " đ",
                        MachineCashRevenue = (ch.MachineCashRevenue >= 0 ? "+" : "") + ch.MachineCashRevenue.ToString("N0") + " đ",
                        BankTransferRevenue = ch.BankTransferRevenue.ToString("N0") + " đ",
                        CashRefundAmount = (ch.CashRefundAmount > 0 ? "-" : "") + ch.CashRefundAmount.ToString("N0") + " đ",
                        TheoreticalCash = theoretical.ToString("N0") + " đ",
                        ActualCash = isActive ? "-" : ch.ActualCash.ToString("N0") + " đ",
                        RetainedCash = ch.RetainedCash.ToString("N0") + " đ",
                        DepositedCash = ch.DepositedCash.ToString("N0") + " đ",
                        Discrepancy = isActive ? "-" : ((discrepancy >= 0 ? "+" : "") + discrepancy.ToString("N0") + " đ"),
                        Status = ch.Status,
                        OpenedAt = ch.OpenedAt.ToVietnamTimeString("HH:mm dd/MM/yyyy"),
                        ClosedAt = ch.ClosedAt.HasValue ? ch.ClosedAt.ToVietnamTimeString("HH:mm dd/MM/yyyy") : "-",
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

        public async Task<int> AutoCloseStaleActiveHandoversAsync(string? branchId = null)
        {
            return await _cashHandoverRepository.AutoCloseStaleActiveHandoversAsync(branchId);
        }
    }
}
