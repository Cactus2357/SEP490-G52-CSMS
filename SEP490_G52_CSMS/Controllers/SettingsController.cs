using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager")]
    public class SettingsController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingsController(CSMSAppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> GetUserBranchIdAsync()
        {
            var branchIdClaim = User.GetBranchId();
            if (!string.IsNullOrWhiteSpace(branchIdClaim))
            {
                return branchIdClaim;
            }

            var userId = User.GetEmployeeId();
            if (userId.HasValue)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == userId.Value);
                if (employee != null && !string.IsNullOrEmpty(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            var firstBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Status == "Active");
            return firstBranch?.BranchId ?? "CB001";
        }

        public async Task<IActionResult> Index(string activeTab = "general")
        {
            DbInitializer.EnsureTablesCreated(_context);
            var branchId = await GetUserBranchIdAsync();
            var setting = await _context.BranchSettings
                .FirstOrDefaultAsync(s => s.BranchId == branchId);

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId);

            if (setting == null)
            {
                setting = new BranchSetting
                {
                    BranchId = branchId,
                    BranchDisplayName = branch?.BranchName ?? "CSMS Coffee",
                    Address = branch?.Address ?? "",
                    ContactPhone = branch?.PhoneNumber ?? "0988888888",
                    BankCode = "MBBank",
                    AccountNumber = "0333333333",
                    AccountName = "CSMS CAFE",
                    TransferPrefix = "CSMS",
                    AutoConfirmOrder = true,
                    IsSePayActive = true,
                    AutoPrintReceipt = true,
                    EnableSoundNotification = true
                };
                _context.BranchSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            ViewBag.ActiveTab = activeTab;
            ViewBag.WebhookUrl = $"{Request.Scheme}://{Request.Host}/api/sepay/webhook";
            
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGeneralSettings(BranchSetting model)
        {
            var branchId = await GetUserBranchIdAsync();
            var setting = await _context.BranchSettings
                .FirstOrDefaultAsync(s => s.BranchId == branchId);

            if (setting == null)
            {
                setting = new BranchSetting { BranchId = branchId };
                _context.BranchSettings.Add(setting);
            }

            setting.BranchDisplayName = string.IsNullOrWhiteSpace(model.BranchDisplayName) ? "CSMS Coffee" : model.BranchDisplayName.Trim();
            
            var phone = model.ContactPhone?.Trim() ?? "";
            if (!string.IsNullOrEmpty(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$"))
            {
                TempData["ErrorMessage"] = "Số điện thoại không hợp lệ! Vui lòng nhập đúng định dạng 10 chữ số bắt đầu bằng số 0 (VD: 0912345678).";
                return RedirectToAction(nameof(Index), new { activeTab = "general" });
            }
            setting.ContactPhone = phone;
            setting.Address = string.IsNullOrWhiteSpace(model.Address) ? "" : model.Address.Trim();
            setting.AutoPrintReceipt = model.AutoPrintReceipt;
            setting.EnableSoundNotification = model.EnableSoundNotification;
            setting.UpdatedAt = DateTime.UtcNow;

            // Also update main Branch record
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId);
            if (branch != null)
            {
                branch.BranchName = setting.BranchDisplayName;
                branch.Address = setting.Address;
                branch.PhoneNumber = setting.ContactPhone;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật cài đặt chung chi nhánh thành công!";

            return RedirectToAction(nameof(Index), new { activeTab = "general" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSePay(BranchSetting model)
        {
            var branchId = await GetUserBranchIdAsync();
            var setting = await _context.BranchSettings
                .FirstOrDefaultAsync(s => s.BranchId == branchId);

            if (setting == null)
            {
                setting = new BranchSetting { BranchId = branchId };
                _context.BranchSettings.Add(setting);
            }

            setting.BankCode = string.IsNullOrWhiteSpace(model.BankCode) ? "MBBank" : model.BankCode.Trim();
            setting.AccountNumber = string.IsNullOrWhiteSpace(model.AccountNumber) ? "" : model.AccountNumber.Trim();
            setting.AccountName = string.IsNullOrWhiteSpace(model.AccountName) ? "" : model.AccountName.Trim();
            setting.SePayApiKey = model.SePayApiKey?.Trim();
            setting.WebhookSecretToken = model.WebhookSecretToken?.Trim();
            setting.TransferPrefix = string.IsNullOrWhiteSpace(model.TransferPrefix) ? "CSMS" : model.TransferPrefix.Trim();
            setting.AutoConfirmOrder = model.AutoConfirmOrder;
            setting.IsSePayActive = model.IsSePayActive;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật tài khoản ngân hàng & cấu hình SePay thành công!";

            return RedirectToAction(nameof(Index), new { activeTab = "sepay" });
        }

        [HttpPost]
        public async Task<IActionResult> TestConnection(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Json(new { success = false, message = "Vui lòng nhập SePay API Key để kiểm tra." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey.Trim()}");
                var response = await client.GetAsync("https://my.sepay.vn/userapi/transactions/list?limit=1");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Kết nối SePay thành công!", details = content });
                }
                else
                {
                    return Json(new { success = false, message = $"SePay phản hồi lỗi: {response.StatusCode}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối máy chủ SePay: " + ex.Message });
            }
        }

        // =========================================================
        //  SEPAY WEBHOOK ENDPOINT (Real-time Bank Transfer Detection)
        //  Uses BranchSetting model
        // =========================================================
        [HttpPost]
        [AllowAnonymous]
        [Route("api/sepay/webhook")]
        [Route("Settings/SePayWebhook")]
        public async Task<IActionResult> Webhook([FromBody] SePayWebhookModel payload)
        {
            if (payload == null)
            {
                return BadRequest(new { success = false, message = "Invalid payload" });
            }

            if (payload.TransferType != null && !payload.TransferType.Equals("in", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, message = "Ignored outgoing transfer" });
            }

            string textToSearch = $"{payload.Content} {payload.Description} {payload.Code}";
            if (string.IsNullOrWhiteSpace(textToSearch))
            {
                return Json(new { success = false, message = "No content to parse" });
            }

            Order? order = null;

            // 1. Direct match: Check if textToSearch contains any active unpaid or partially paid order's OrderId
            var unpaidOrders = await _context.Orders
                .Include(o => o.Payments)
                .Where(o => o.PaymentStatus == "Unpaid" || o.PaymentStatus == "PartiallyPaid")
                .OrderByDescending(o => o.CreatedAt)
                .Take(50)
                .ToListAsync();

            foreach (var candidate in unpaidOrders)
            {
                string candidateId = candidate.OrderId.Replace("-", "");
                string cleanedSearch = textToSearch.Replace("-", "");
                if (cleanedSearch.IndexOf(candidateId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    order = candidate;
                    break;
                }
            }

            // 2. Fallback Regex match if direct match didn't find unpaid order (e.g. MH260814007, OD10025, etc.)
            if (order == null)
            {
                var matches = Regex.Matches(textToSearch, @"\b(?:MH|OD)[A-Z0-9_-]+\b", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string extractedId = match.Value.ToUpper();
                    order = await _context.Orders
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.OrderId == extractedId || o.OrderId.Replace("-", "") == extractedId.Replace("-", ""));
                    if (order != null) break;
                }
            }

            if (order == null)
            {
                return Json(new { success = false, message = $"No Order ID found in content: '{textToSearch}'" });
            }

            if (order.PaymentStatus == "Completed" || order.PaymentStatus == "PaidSuccess" || order.PaymentStatus == "TransferSuccessPending")
            {
                return Json(new { success = true, message = $"Order {order.OrderId} is already paid or pending confirmation." });
            }

            // Verify received amount vs order total (allowing 1đ tolerance)
            decimal receivedAmount = payload.TransferAmount;
            string refCode = payload.ReferenceCode ?? payload.Id.ToString();
            string paymentDetail = $"CK SePay ({refCode}) - Đã nhận:{receivedAmount:N0}đ";
            if (paymentDetail.Length > 200)
            {
                paymentDetail = paymentDetail.Substring(0, 200);
            }

            var payment = new Payment
            {
                OrderId = order.OrderId,
                BranchId = order.BranchId,
                CashierId = order.CashierId,
                PaymentType = "Payment",
                PaymentMethod = "BankTransfer",
                Amount = receivedAmount,
                TransactionCode = refCode,
                Status = "Success",
                Notes = paymentDetail,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Payments.AddAsync(payment);
            order.Payments.Add(payment);

            decimal totalBankReceived = order.BankPaid;
            decimal totalPaid = order.PaidAmount;

            if (totalPaid >= (order.TotalAmount - 1))
            {
                order.PaymentStatus = "TransferSuccessPending";
                order.PaymentMethod = paymentDetail;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Payment of {receivedAmount:N0}đ for order {order.OrderId} processed successfully" });
            }
            else
            {
                order.PaymentStatus = "PartiallyPaid";
                order.PaymentMethod = $"CK SePay 1 phần ({refCode}) - Đã nhận:{totalBankReceived:N0}đ";
                await _context.SaveChangesAsync();

                return Json(new { success = true, isPartial = true, message = $"Partial transfer of {receivedAmount:N0}đ recorded. Total received: {totalBankReceived:N0}đ, Remaining: {(order.TotalAmount - totalPaid):N0}đ" });
            }
        }
    }
}
