using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Employees;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SEP490_G52_CSMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly IMemoryCache _cache;

        public AuthController(CSMSAppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
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
        public async Task<IActionResult> FaceLogin([FromBody] FaceLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FacialId))
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt trống." });

            var employees = await _context.Employees
                .Where(e => e.FaceData == request.FacialId)
                .ToListAsync();

            if (employees.Count == 0)
                return Json(new { success = false, errorMessage = "Face not recognized" });

            if (employees.Count > 1)
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt bị trùng lặp hệ thống." });

            var employee = employees[0];

            if (employee.Status != "Active")
                return Json(new { success = false, errorMessage = "Tài khoản đã bị ngừng hoạt động." });

            if (employee.LockoutUntil.HasValue && employee.LockoutUntil.Value > DateTime.UtcNow)
                return Json(new { success = false, errorMessage = "Account locked due to multiple failed login attempts." });

            employee.FailedLoginAttempts = 0;
            employee.LockoutUntil = null;
            await _context.SaveChangesAsync();
            await SignInUserAsync(employee);
            return Json(new { success = true });
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

            var duplicate = await _context.Employees
                .AnyAsync(e => e.FaceData == request.FaceData && e.EmployeeId != employee.EmployeeId);

            if (duplicate)
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt này đã được liên kết với tài khoản khác." });

            employee.FaceData = request.FaceData;
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

            employee.Password = model.NewPassword;
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
        public IActionResult ForgotPassword()
        {
            ViewBag.Step = 1;
            return View();
        }

        private async Task SignInUserAsync(Employee employee)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.FullName ?? ""),
                new Claim(ClaimTypes.Role, employee.Role ?? "Cashier"),
                new Claim("Username", employee.Username ?? "")
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
            //if (HashPassword(inputPassword) == storedPassword) return true;
            return inputPassword == storedPassword;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
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

}