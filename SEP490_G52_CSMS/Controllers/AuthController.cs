using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SEP490_G52_CSMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public AuthController(CSMSAppDbContext context, IMemoryCache cache, IEmailService emailService)
        {
            _context = context;
            _cache = cache;
            _emailService = emailService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Tài khoản và mật khẩu là bắt buộc.");
                return View();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == username || e.Email == username || e.PhoneNumber == username);

            if (employee == null)
            {
                ModelState.AddModelError("", "Incorrect username or password. Please try again.");
                return View();
            }

            if (employee.Status != "Active")
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động.");
                return View();
            }

            if (employee.LockoutUntil.HasValue && employee.LockoutUntil.Value > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Account locked due to multiple failed login attempts.");
                return View();
            }

            if (!VerifyPassword(password, employee.Password ?? ""))
            {
                employee.FailedLoginAttempts++;
                if (employee.FailedLoginAttempts >= 5)
                    employee.LockoutUntil = DateTime.UtcNow.AddMinutes(5);
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", employee.FailedLoginAttempts >= 5
                    ? "Account locked due to multiple failed login attempts."
                    : "Incorrect username or password. Please try again.");
                return View();
            }

            employee.FailedLoginAttempts = 0;
            employee.LockoutUntil = null;
            await _context.SaveChangesAsync();
            await SignInUserAsync(employee);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FaceLogin([FromBody] FaceLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FacialId))
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt trống." });

            var facialId = request.FacialId.Trim();
            Employee? matchedEmployee = null;

            // Attempt to parse facialId as a JSON array of 128 floats (face-api.js descriptor)
            float[]? inputDescriptor = null;
            if (facialId.StartsWith("[") && facialId.EndsWith("]"))
            {
                try
                {
                    inputDescriptor = System.Text.Json.JsonSerializer.Deserialize<float[]>(facialId);
                }
                catch
                {
                    // Ignore parsing error
                }
            }

            if (inputDescriptor != null && inputDescriptor.Length == 128)
            {
                // Retrieve all active employees with face data registered
                var activeEmployees = await _context.Employees
                    .Where(e => e.Status == "Active" && e.FaceData != null)
                    .ToListAsync();

                double minDistance = double.MaxValue;
                const double threshold = 0.55; // Standard distance threshold for face-api.js (usually 0.6; 0.55 is slightly more conservative)

                foreach (var emp in activeEmployees)
                {
                    var dbFaceData = emp.FaceData!.Trim();
                    if (dbFaceData.StartsWith("[") && dbFaceData.EndsWith("]"))
                    {
                        try
                        {
                            var dbDescriptor = System.Text.Json.JsonSerializer.Deserialize<float[]>(dbFaceData);
                            if (dbDescriptor != null && dbDescriptor.Length == 128)
                            {
                                double distance = 0;
                                for (int i = 0; i < 128; i++)
                                {
                                    double diff = inputDescriptor[i] - dbDescriptor[i];
                                    distance += diff * diff;
                                }
                                distance = Math.Sqrt(distance);

                                if (distance < threshold && distance < minDistance)
                                {
                                    minDistance = distance;
                                    matchedEmployee = emp;
                                }
                            }
                        }
                        catch
                        {
                            // Skip parsing failures for individual records
                        }
                    }
                }
            }

            // Fallback 1: Exact string match (for legacy FaceIO appIds/tokens or exact simulation values)
            if (matchedEmployee == null)
            {
                matchedEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.FaceData != null && e.FaceData.Trim() == facialId && e.Status == "Active");
            }

            // Fallback 2: Simulation username fallback (fio_sim_username)
            if (matchedEmployee == null && facialId.StartsWith("fio_sim_"))
            {
                var simUsername = facialId.Substring("fio_sim_".Length);
                matchedEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Username == simUsername && e.Status == "Active");
            }

            if (matchedEmployee == null)
                return Json(new { success = false, errorMessage = "Không nhận diện được khuôn mặt." });

            if (matchedEmployee.LockoutUntil.HasValue && matchedEmployee.LockoutUntil.Value > DateTime.UtcNow)
                return Json(new { success = false, errorMessage = "Tài khoản đang bị khóa tạm thời do nhiều lần đăng nhập sai." });

            matchedEmployee.FailedLoginAttempts = 0;
            matchedEmployee.LockoutUntil = null;

            // -------------------------------------------------------
            // CHẤM CÔNG: Nếu nhân viên có ca làm hôm nay, ghi nhận check-in
            // -------------------------------------------------------
            var today = DateTime.Now.Date;
            var now = DateTime.Now;

            var todayRoster = await _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .Where(r => r.EmployeeId == matchedEmployee.EmployeeId
                         && r.AssignmentDate.Date == today)
                .FirstOrDefaultAsync();

            string? attendanceMessage = null;

            if (todayRoster != null && todayRoster.FixedShift != null)
            {
                // Check if already checked-in today (don't double-stamp)
                var existingLog = todayRoster.AttendanceLogs
                    .FirstOrDefault(l => l.EmployeeId == matchedEmployee.EmployeeId);

                if (existingLog == null)
                {
                    // Determine OnTime or Late: allow 15-minute grace window after shift start
                    var shiftStart = todayRoster.AssignmentDate.Date + todayRoster.FixedShift.StartTime;
                    var graceCutoff = shiftStart.AddMinutes(15);
                    var checkInStatus = now <= graceCutoff ? "OnTime" : "Late";

                    var log = new Models.Attendance.AttendanceLog
                    {
                        RosterId = todayRoster.RosterId,
                        EmployeeId = matchedEmployee.EmployeeId,
                        CheckInTime = now,
                        IsFaceCheckInValid = true,
                        CheckInConfidence = 95,
                        CheckInStatus = checkInStatus,
                        OverallStatus = "Present",
                    };
                    _context.AttendanceLogs.Add(log);
                    attendanceMessage = checkInStatus == "OnTime"
                        ? $"Chấm công thành công! Ca: {todayRoster.FixedShift.ShiftName} – Đúng giờ."
                        : $"Chấm công thành công! Ca: {todayRoster.FixedShift.ShiftName} – Đi trễ.";
                }
                else
                {
                    attendanceMessage = "Bạn đã chấm công cho ca hôm nay rồi.";
                }
            }

            await _context.SaveChangesAsync();
            await SignInUserAsync(matchedEmployee);
            return Json(new { success = true, attendanceMessage });
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> FaceRegister()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
                return Forbid();

            return View(employee);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FaceRegister([FromBody] FaceRegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FaceData))
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt trống." });

            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
                return Json(new { success = false, errorMessage = "Bạn cần đăng nhập để thực hiện thao tác này." });

            var faceData = request.FaceData.Trim();
            bool isDuplicate = false;

            // Check duplicate using vector distance if incoming data is a face descriptor
            float[]? newDescriptor = null;
            if (faceData.StartsWith("[") && faceData.EndsWith("]"))
            {
                try
                {
                    newDescriptor = System.Text.Json.JsonSerializer.Deserialize<float[]>(faceData);
                }
                catch { }
            }

            if (newDescriptor != null && newDescriptor.Length == 128)
            {
                var otherEmployees = await _context.Employees
                    .Where(e => e.EmployeeId != employee.EmployeeId && e.FaceData != null)
                    .ToListAsync();

                foreach (var emp in otherEmployees)
                {
                    var dbFaceData = emp.FaceData!.Trim();
                    if (dbFaceData.StartsWith("[") && dbFaceData.EndsWith("]"))
                    {
                        try
                        {
                            var dbDescriptor = System.Text.Json.JsonSerializer.Deserialize<float[]>(dbFaceData);
                            if (dbDescriptor != null && dbDescriptor.Length == 128)
                            {
                                double distance = 0;
                                for (int i = 0; i < 128; i++)
                                {
                                    double diff = newDescriptor[i] - dbDescriptor[i];
                                    distance += diff * diff;
                                }
                                distance = Math.Sqrt(distance);

                                if (distance < 0.55) // Duplicate if too close
                                {
                                    isDuplicate = true;
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            else
            {
                // Fallback: exact string check
                isDuplicate = await _context.Employees
                    .AnyAsync(e => e.FaceData != null && e.FaceData.Trim() == faceData && e.EmployeeId != employee.EmployeeId);
            }

            if (isDuplicate)
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt này đã được liên kết với tài khoản khác." });

            employee.FaceData = faceData;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
                return Forbid();

            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
                return Forbid();

            // BR-01: mandatory fields (đã bọc bởi [Required] ở ViewModel)
            if (!ModelState.IsValid)
                return View(model);

            // BR-05: current password must be correct
            if (!VerifyPassword(model.CurrentPassword, employee.Password ?? ""))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            // BR-02: new password and confirm must match
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword),
                    "Mật khẩu mới và Xác nhận mật khẩu mới không trùng khớp. Vui lòng thử lại.");
                return View(model);
            }

            // BR-03: complexity — at least 8 chars, 1 uppercase, 1 digit, 1 special char
            if (!IsPasswordComplex(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword),
                    "Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm 1 chữ hoa, 1 số và 1 ký tự đặc biệt.");
                return View(model);
            }

            // BR-04: new password must not match current password
            if (VerifyPassword(model.NewPassword, employee.Password ?? ""))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới không được trùng với mật khẩu hiện tại.");
                return View(model);
            }

            employee.Password = DAT_PasswordHasher.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["ChangePasswordSuccess"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(ChangePassword));
        }

        private static bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            var hasUpper = Regex.IsMatch(password, "[A-Z]");
            var hasDigit = Regex.IsMatch(password, "[0-9]");
            var hasSpecial = Regex.IsMatch(password, @"[^a-zA-Z0-9]");

            return hasUpper && hasDigit && hasSpecial;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
                return Forbid();

            return View(employee);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UsernameOrEmail))
            {
                return Json(new { success = false, message = "Vui lòng nhập Email hoặc Tên tài khoản." });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == request.UsernameOrEmail || e.Email == request.UsernameOrEmail);

            if (employee == null || employee.Status != "Active")
            {
                return Json(new { success = false, message = "Tài khoản hoặc email không tồn tại trong hệ thống." });
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                return Json(new { success = false, message = "Tài khoản không được liên kết với email hợp lệ." });
            }

            string otp = GenerateOtp();
            var otpKey = $"OTP_{employee.Email}";
            _cache.Set(otpKey, otp, TimeSpan.FromMinutes(5));

            try
            {
                string subject = "[CSMS] Mã xác thực (OTP) đặt lại mật khẩu";
                string body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e5e7eb; border-radius: 8px;'>
                        <h2 style='color: #111827; text-align: center; border-bottom: 1px solid #f3f4f6; padding-bottom: 10px;'>Đặt lại mật khẩu tài khoản CSMS</h2>
                        <p>Xin chào <strong>{employee.FullName}</strong>,</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản: <strong>{employee.Username}</strong></p>
                        <p style='text-align: center; margin: 30px 0;'>
                            <span style='background-color: #f3f4f6; padding: 12px 24px; font-size: 1.5rem; font-weight: bold; letter-spacing: 4px; color: #2563eb; border-radius: 6px; border: 1px dashed #2563eb;'>{otp}</span>
                        </p>
                        <p>Mã xác thực (OTP) này có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
                        <p style='color: #6b7280; font-size: 0.85rem; margin-top: 30px;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                        <p style='margin-top: 20px; border-top: 1px solid #f3f4f6; padding-top: 10px;'>Trân trọng,<br/>Đội ngũ hỗ trợ CSMS</p>
                    </div>";

                await _emailService.SendEmailAsync(employee.Email, subject, body);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Không thể gửi email OTP: {ex.Message}" });
            }

            return Json(new { success = true, email = MaskEmail(employee.Email), message = "Mã OTP đã được gửi đến email liên kết." });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == model.UsernameOrEmail || e.Email == model.UsernameOrEmail);

            if (employee == null || employee.Status != "Active")
            {
                ModelState.AddModelError("", "Tài khoản hoặc email không tồn tại trong hệ thống.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                ModelState.AddModelError("", "Tài khoản không được liên kết với email hợp lệ.");
                return View(model);
            }

            var otpKey = $"OTP_{employee.Email}";
            if (!_cache.TryGetValue(otpKey, out string? cachedOtp) || cachedOtp != model.Otp)
            {
                ModelState.AddModelError("", "Mã OTP không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "Mật khẩu mới và Xác nhận mật khẩu mới không trùng khớp. Vui lòng thử lại.");
                return View(model);
            }

            if (!IsPasswordComplex(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm 1 chữ hoa, 1 số và 1 ký tự đặc biệt.");
                return View(model);
            }

            if (VerifyPassword(model.NewPassword, employee.Password ?? ""))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới không được trùng với mật khẩu hiện tại.");
                return View(model);
            }

            // Securely reset password
            employee.Password = DAT_PasswordHasher.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            // Clear cache OTP
            _cache.Remove(otpKey);

            TempData["ForgotPasswordSuccess"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }

        private async Task SignInUserAsync(Employee employee)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.FullName ?? ""),
                new Claim(ClaimTypes.Role, employee.Role ?? "Cashier"),
                new Claim("Username", employee.Username ?? ""),
                new Claim("BranchId", employee.BranchId ?? "")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var employeeId))
                return null;

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null || employee.Status != "Active")
                return null;

            return employee;
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword)) return false;
            return DAT_PasswordHasher.VerifyPassword(inputPassword, storedPassword);
        }

        private static string GenerateOtp()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        private static string MaskEmail(string email)
        {
            var idx = email.IndexOf('@');
            if (idx <= 1) return email;
            return email[0] + new string('*', idx - 1) + email[idx..];
        }
    }

    public class FaceLoginRequest
    {
        public string? FacialId { get; set; }
    }

    public class FaceRegisterRequest
    {
        public string? FaceData { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [MaxLength(50)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MaxLength(50)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [MaxLength(50)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class SendOtpRequest
    {
        public string? UsernameOrEmail { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên tài khoản.")]
        [MaxLength(100)]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số.")]
        public string Otp { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MaxLength(50)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [MaxLength(50)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

}