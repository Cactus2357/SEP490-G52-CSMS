using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Employees;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SEP490_G52_CSMS.Controllers
{
    public class BranchManagerController : Controller
    {
        private readonly CSMSAppDbContext _context;

        public BranchManagerController(CSMSAppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var managers = _context.Employees.Where(e => e.Role == "BranchManager").ToList();
            ViewBag.Branches = _context.Branches
                .Where(b => b.Status == "Active")
                .OrderBy(b => b.BranchName)
                .ToList();
            return View(managers);
        }

        [HttpPost]
        public IActionResult GenerateAccount([FromBody] GenerateAccountRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return Json(new { success = false, errorMessage = "Vui lòng nhập đầy đủ Họ và tên và Email." });
            }

            bool emailExists = _context.Employees.Any(e => e.Email == request.Email);
            if (emailExists)
            {
                return Json(new { success = false, errorMessage = "MSG10: Lỗi trùng lặp dữ liệu Email." });
            }

            string baseUsername = RemoveDiacritics(request.FullName);
            string username = baseUsername;
            int suffix = 1;
            while (_context.Employees.Any(e => e.Username == username))
            {
                suffix++;
                username = baseUsername + suffix;
            }

            string password = GenerateRandomPassword(10);

            return Json(new { success = true, username, password });
        }

        [HttpPost]
        public IActionResult CreateAccount([FromBody] CreateManagerRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.BranchId) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ. Vui lòng thử lại." });
            }

            if (_context.Employees.Any(e => e.Email == request.Email))
            {
                return Json(new { success = false, errorMessage = "MSG10: Lỗi trùng lặp dữ liệu Email." });
            }

            if (_context.Employees.Any(e => e.Username == request.Username))
            {
                return Json(new { success = false, errorMessage = "MSG10: Lỗi trùng lặp dữ liệu Tên tài khoản." });
            }

            var branchExists = _context.Branches.Any(b => b.BranchId == request.BranchId);
            if (!branchExists)
            {
                return Json(new { success = false, errorMessage = "Chi nhánh không hợp lệ." });
            }

            var employee = new Employee
            {
                FullName = request.FullName,
                Email = request.Email,
                Username = request.Username,
                Password = request.Password, // TODO: hash before persisting in production
                Role = "BranchManager",
                BranchId = request.BranchId,
                Status = "Active",
                EmploymentType = "Full-time",
                DateOfBirth = DateTime.Today,
                Address = string.Empty,
                PhoneNumber = string.Empty,
                CitizenId = string.Empty
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            _context.BranchManagers.Add(new BranchManager
            {
                BranchId = request.BranchId,
                ManagerId = employee.EmployeeId,
                AppointedDate = DateTime.Now
            });
            _context.SaveChanges();

            // TODO: dispatch generated credentials to employee.Email (POS-02)

            return Json(new { success = true });
        }

        private static string RemoveDiacritics(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (char c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            string result = builder.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace('Đ', 'D').Replace('đ', 'd');

            var parts = result.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1).ToLower() : string.Empty));

            return string.Concat(parts);
        }

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new StringBuilder(length);

            foreach (byte b in bytes)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }

        public class GenerateAccountRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
        }

        public class CreateManagerRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string BranchId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}