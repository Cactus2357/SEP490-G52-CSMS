using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Employees;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly CSMSAppDbContext _context;

        public AuthController(CSMSAppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
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
                {
                    employee.LockoutUntil = DateTime.UtcNow.AddMinutes(5);
                }
                await _context.SaveChangesAsync();

                if (employee.FailedLoginAttempts >= 5)
                {
                    ModelState.AddModelError("", "Account locked due to multiple failed login attempts.");
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect username or password. Please try again.");
                }
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
            {
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt trống." });
            }

            var employees = await _context.Employees
                .Where(e => e.FaceData == request.FacialId)
                .ToListAsync();

            if (employees.Count == 0)
            {
                return Json(new { success = false, errorMessage = "Face not recognized" });
            }

            if (employees.Count > 1)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt bị trùng lặp hệ thống." });
            }

            var employee = employees[0];

            if (employee.Status != "Active")
            {
                return Json(new { success = false, errorMessage = "Tài khoản đã bị ngừng hoạt động." });
            }

            if (employee.LockoutUntil.HasValue && employee.LockoutUntil.Value > DateTime.UtcNow)
            {
                return Json(new { success = false, errorMessage = "Account locked due to multiple failed login attempts." });
            }

            employee.FailedLoginAttempts = 0;
            employee.LockoutUntil = null;
            await _context.SaveChangesAsync();

            await SignInUserAsync(employee);

            return Json(new { success = true });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
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
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword)) return false;
            if (HashPassword(inputPassword) == storedPassword) return true;
            return inputPassword == storedPassword;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }

    public class FaceLoginRequest
    {
        public string? FacialId { get; set; }
    }
}
