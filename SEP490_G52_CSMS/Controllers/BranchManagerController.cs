using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Employees;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "RManager")]
    public class BranchManagerController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly IDAT_EmailHelper _emailHelper;

        public BranchManagerController(CSMSAppDbContext context, IDAT_EmailHelper emailHelper)
        {
            _context = context;
            _emailHelper = emailHelper;
        }

        public IActionResult Index()
        {
            var managers = _context.Employees
                .Where(e => e.Role == "BranchManager")
                .Include(e => e.Branch)
                .ToList();
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
                Password = DAT_PasswordHasher.HashPassword(request.Password),
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

            _emailHelper.SendAccountCredentials(employee.Email, employee.FullName, employee.Username, request.Password);

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

        [HttpGet]
        public IActionResult DetailPartial(int id)
        {
            var manager = _context.Employees
                .Include(e => e.Branch)
                .FirstOrDefault(e => e.EmployeeId == id && e.Role == "BranchManager");

            if (manager == null)
            {
                return NotFound("Không tìm thấy Quản lý chi nhánh.");
            }

            return PartialView("_ManagerDetail", manager);
        }

        [HttpGet]
        public IActionResult UpdatePartial(int id)
        {
            var manager = _context.Employees
                .Include(e => e.Branch)
                .FirstOrDefault(e => e.EmployeeId == id && e.Role == "BranchManager");

            if (manager == null)
            {
                return NotFound("Không tìm thấy Quản lý chi nhánh.");
            }

            ViewBag.Branches = _context.Branches
                .Where(b => b.Status == "Active")
                .OrderBy(b => b.BranchName)
                .ToList();

            return PartialView("_ManagerUpdateForm", manager);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBranchManagerRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var manager = _context.Employees.FirstOrDefault(e => e.EmployeeId == id && e.Role == "BranchManager");
            if (manager == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy Quản lý chi nhánh." });
            }

            // Validations
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Json(new { success = false, errorMessage = "Họ và tên không được để trống." });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Json(new { success = false, errorMessage = "Email không được để trống." });
            }

            if (_context.Employees.Any(e => e.Email == request.Email.Trim() && e.EmployeeId != id))
            {
                return Json(new { success = false, errorMessage = "Email đã tồn tại trên hệ thống." });
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return Json(new { success = false, errorMessage = "Số điện thoại không được để trống." });
            }

            if (_context.Employees.Any(e => e.PhoneNumber == request.PhoneNumber.Trim() && e.EmployeeId != id))
            {
                return Json(new { success = false, errorMessage = "Số điện thoại đã tồn tại trên hệ thống." });
            }

            if (string.IsNullOrWhiteSpace(request.CitizenId) || request.CitizenId.Trim().Length != 12 || !System.Text.RegularExpressions.Regex.IsMatch(request.CitizenId.Trim(), @"^\d{12}$"))
            {
                return Json(new { success = false, errorMessage = "Số CCCD phải chứa chính xác 12 chữ số." });
            }

            if (_context.Employees.Any(e => e.CitizenId == request.CitizenId.Trim() && e.EmployeeId != id))
            {
                return Json(new { success = false, errorMessage = "Số CCCD đã tồn tại trên hệ thống." });
            }

            if (request.DateOfBirth == default)
            {
                return Json(new { success = false, errorMessage = "Ngày sinh không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return Json(new { success = false, errorMessage = "Địa chỉ không được để trống." });
            }

            var branchExists = _context.Branches.Any(b => b.BranchId == request.BranchId);
            if (!branchExists)
            {
                return Json(new { success = false, errorMessage = "Chi nhánh không hợp lệ." });
            }

            // Files upload
            const long MaxFileSize = 5 * 1024 * 1024;
            string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg" };
            string uploadsFolder = Path.Combine("wwwroot", "uploads", "employees");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            if (request.CccdFile != null && request.CccdFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(request.CccdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, errorMessage = "File ảnh CCCD phải có định dạng .pdf hoặc .jpg (.jpeg)." });
                }
                if (request.CccdFile.Length > MaxFileSize)
                {
                    return Json(new { success = false, errorMessage = "Dung lượng file ảnh CCCD không được vượt quá 5MB." });
                }

                string uniqueFileName = $"cccd_{id}_{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.CccdFile.CopyToAsync(fileStream);
                }
                manager.CccdFilePath = $"/uploads/employees/{uniqueFileName}";
            }

            if (request.ContractFile != null && request.ContractFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(request.ContractFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, errorMessage = "File hợp đồng lao động phải có định dạng .pdf hoặc .jpg (.jpeg)." });
                }
                if (request.ContractFile.Length > MaxFileSize)
                {
                    return Json(new { success = false, errorMessage = "Dung lượng file hợp đồng không được vượt quá 5MB." });
                }

                string uniqueFileName = $"contract_{id}_{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ContractFile.CopyToAsync(fileStream);
                }
                manager.ContractFilePath = $"/uploads/employees/{uniqueFileName}";
            }

            // Update text fields
            manager.FullName = request.FullName.Trim();
            manager.Email = request.Email.Trim();
            manager.PhoneNumber = request.PhoneNumber.Trim();
            manager.CitizenId = request.CitizenId.Trim();
            manager.DateOfBirth = request.DateOfBirth;
            manager.Address = request.Address.Trim();

            // Update branch assignment if changed
            if (manager.BranchId != request.BranchId)
            {
                manager.BranchId = request.BranchId;

                // Safely update branch_managers table
                var existingMappings = _context.BranchManagers.Where(bm => bm.ManagerId == id).ToList();
                var appointedDate = DateTime.Now;
                if (existingMappings.Any())
                {
                    appointedDate = existingMappings.First().AppointedDate;
                    _context.BranchManagers.RemoveRange(existingMappings);
                }

                _context.BranchManagers.Add(new BranchManager
                {
                    BranchId = request.BranchId,
                    ManagerId = id,
                    AppointedDate = appointedDate
                });
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult CheckDeactivation(int id)
        {
            var manager = _context.Employees.FirstOrDefault(e => e.EmployeeId == id && e.Role == "BranchManager");
            if (manager == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy Quản lý chi nhánh." });
            }

            // Uncompleted shifts check
            var today = DateTime.Today;
            var rosters = _context.WeeklyRosterGrids
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .Where(r => r.EmployeeId == id)
                .ToList();

            var uncompletedList = new List<object>();
            foreach (var roster in rosters)
            {
                bool isUncompleted = false;
                if (roster.AssignmentDate.Date > today)
                {
                    isUncompleted = true;
                }
                else
                {
                    var log = roster.AttendanceLogs.FirstOrDefault(l => l.EmployeeId == id);
                    if (log == null || log.CheckOutTime == null)
                    {
                        isUncompleted = true;
                    }
                }

                if (isUncompleted)
                {
                    uncompletedList.Add(new
                    {
                        date = roster.AssignmentDate.ToString("dd/MM"),
                        shiftName = roster.FixedShift?.ShiftName ?? "Ca làm việc",
                        status = "Chưa hoàn thành"
                    });
                }
            }

            // Managed branch check
            var managedBranches = _context.BranchManagers
                .Include(bm => bm.Branch)
                .Where(bm => bm.ManagerId == id)
                .Select(bm => bm.Branch.BranchName)
                .ToList();

            bool canDeactivate = uncompletedList.Count == 0;

            return Json(new
            {
                success = true,
                employeeId = manager.EmployeeId,
                fullName = manager.FullName,
                username = manager.Username,
                role = "BranchManager",
                canDeactivate = canDeactivate,
                uncompletedShifts = uncompletedList,
                managedBranches = managedBranches
            });
        }

        [HttpPost]
        public IActionResult Deactivate([FromBody] DeactivateRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var manager = _context.Employees.FirstOrDefault(e => e.EmployeeId == request.EmployeeId && e.Role == "BranchManager");
            if (manager == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy Quản lý chi nhánh." });
            }

            manager.Status = "Inactive";
            _context.SaveChanges();

            // Log details
            Console.WriteLine($"[BRANCH MANAGER DEACTIVATION LOG] Employee ID: {request.EmployeeId}, Username: {manager.Username}, Time: {DateTime.Now}, Reason: {request.Reason}, Notes: {request.Notes}");

            return Json(new { success = true });
        }
    }

    public class UpdateBranchManagerRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CitizenId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string BranchId { get; set; }
        public IFormFile? CccdFile { get; set; }
        public IFormFile? ContractFile { get; set; }
    }

    public class DeactivateRequest
    {
        public int EmployeeId { get; set; }
        public string Reason { get; set; }
        public string? Notes { get; set; }
    }
}
