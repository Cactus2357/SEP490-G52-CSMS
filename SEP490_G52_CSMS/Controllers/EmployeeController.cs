using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager,RManager")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly CSMSAppDbContext _context;

        public EmployeeController(IEmployeeService employeeService, IEmployeeRepository employeeRepository, CSMSAppDbContext context)
        {
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
            _context = context;
        }

        private async Task<string> GetUserBranchIdAsync()
        {
            var branchIdClaim = User.GetBranchId();
            if (!string.IsNullOrWhiteSpace(branchIdClaim))
            {
                return branchIdClaim;
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && !string.IsNullOrWhiteSpace(employee.BranchId))
                {
                    return employee.BranchId;
                }
            }
            return "";
        }

        // GET: /Employee
        public async Task<IActionResult> Index()
        {
            var loggedInBranchId = await GetUserBranchIdAsync();
            var employees = await _employeeService.GetEmployeesListAsync();
            // User's branch only, exclude BranchManager unless RManager
            var filteredEmployees = System.Linq.Enumerable.Where(employees, e =>
                (User.IsInRole("RManager") || e.BranchId == loggedInBranchId) && e.Role != "BranchManager");
            return View(filteredEmployees);
        }

        // GET: /Employee/GetBranches
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _employeeService.GetBranchesAsync();
            return Json(branches);
        }

        // POST: /Employee/CreateAccount
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromForm] EmployeeCreationDto dto)
        {
            if (dto == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu gửi lên không hợp lệ." });
            }

            // Scope to manager's branch
            var loggedInBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") || string.IsNullOrWhiteSpace(dto.BranchId))
            {
                dto.BranchId = loggedInBranchId;
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(e => !string.IsNullOrEmpty(e)));
                return Json(new { success = false, errorMessage = string.IsNullOrWhiteSpace(errors) ? "Dữ liệu nhập vào chưa đúng định dạng." : $"Dữ liệu chưa đúng định dạng: {errors}" });
            }

            var result = await _employeeService.CreateEmployeeAccountAsync(dto);
            if (result.Success)
            {
                return Json(new
                {
                    success = true,
                    employeeId = result.Employee?.EmployeeId,
                    username = result.GeneratedUsername,
                    password = result.PlainPassword
                });
            }

            return Json(new { success = false, errorMessage = result.ErrorMessage });
        }

        // GET: /Employee/RegisterFace/{id}
        [HttpGet]
        public async Task<IActionResult> RegisterFace(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") && employee.BranchId != loggedInBranchId)
            {
                return Forbid();
            }

            return View(employee);
        }

        // POST: /Employee/SaveFaceData
        [HttpPost]
        public async Task<IActionResult> SaveFaceData([FromBody] SaveFaceDataModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FaceData))
            {
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt không hợp lệ." });
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (employee == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
            }

            var loggedInUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(loggedInUserIdStr, out int loggedInUserId);

            bool isSelf = loggedInUserId == employee.EmployeeId;
            bool isRManager = User.IsInRole("RManager");
            bool isBranchManager = User.IsInRole("BranchManager");
            var loggedInBranchId = await GetUserBranchIdAsync();

            if (!isSelf && !isRManager && (!isBranchManager || (!string.IsNullOrEmpty(employee.BranchId) && !string.Equals(employee.BranchId, loggedInBranchId, StringComparison.OrdinalIgnoreCase))))
            {
                return Json(new { success = false, errorMessage = "Bạn không có quyền lưu dữ liệu khuôn mặt cho nhân viên này." });
            }

            var faceData = model.FaceData.Trim();
            bool isDuplicate = false;
            string duplicateOwnerInfo = "";

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

            var allEmployees = await _employeeRepository.GetAllAsync();

            if (newDescriptor != null && newDescriptor.Length == 128)
            {
                foreach (var emp in allEmployees)
                {
                    if (emp.EmployeeId == employee.EmployeeId || string.IsNullOrWhiteSpace(emp.FaceData))
                        continue;

                    var dbFaceData = emp.FaceData.Trim();
                    if (dbFaceData.StartsWith("[") && dbFaceData.EndsWith("]"))
                    {
                        try
                        {
                            var dbDescriptor = System.Text.Json.JsonSerializer.Deserialize<float[]>(dbFaceData);
                            if (dbDescriptor != null && dbDescriptor.Length == 128 && dbDescriptor.Any(v => v != 0))
                            {
                                double distance = 0;
                                for (int i = 0; i < 128; i++)
                                {
                                    double diff = newDescriptor[i] - dbDescriptor[i];
                                    distance += diff * diff;
                                }
                                distance = Math.Sqrt(distance);

                                if (distance < 0.38) // Strict threshold for face-api.js duplicate detection (same person < 0.38)
                                {
                                    isDuplicate = true;
                                    duplicateOwnerInfo = $"{emp.FullName} (Mã NV: {emp.EmployeeId})";
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                    else if (dbFaceData == faceData && !string.IsNullOrWhiteSpace(dbFaceData))
                    {
                        isDuplicate = true;
                        duplicateOwnerInfo = $"{emp.FullName} (Mã NV: {emp.EmployeeId})";
                        break;
                    }
                }
            }
            else
            {
                // Fallback for legacy face data strings
                var dupEmp = System.Linq.Enumerable.FirstOrDefault(allEmployees, emp =>
                    emp.EmployeeId != employee.EmployeeId &&
                    emp.FaceData != null &&
                    emp.FaceData.Trim() == faceData);

                if (dupEmp != null)
                {
                    isDuplicate = true;
                    duplicateOwnerInfo = $"{dupEmp.FullName} (Mã NV: {dupEmp.EmployeeId})";
                }
            }

            if (isDuplicate)
            {
                return Json(new { success = false, errorMessage = $"Khuôn mặt này đã được đăng ký cho nhân viên {duplicateOwnerInfo}. Mỗi khuôn mặt chỉ được liên kết với 1 tài khoản duy nhất!" });
            }

            var success = await _employeeService.RegisterFaceDataAsync(model.EmployeeId, faceData);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFaceData(int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhân viên." });
            }

            var loggedInUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(loggedInUserIdStr, out int loggedInUserId);
            bool isSelf = (loggedInUserId == employeeId);
            bool isRManager = User.IsInRole("RManager");
            bool isBranchManager = User.IsInRole("BranchManager");
            var loggedInBranchId = await GetUserBranchIdAsync();

            if (!isSelf && !isRManager && (!isBranchManager || (!string.IsNullOrEmpty(employee.BranchId) && !string.Equals(employee.BranchId, loggedInBranchId, StringComparison.OrdinalIgnoreCase))))
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa dữ liệu khuôn mặt của nhân viên này." });
            }

            employee.FaceData = null;
            await _employeeRepository.UpdateAsync(employee);

            return Json(new { success = true, message = $"Đã xóa thành công dữ liệu khuôn mặt (FaceID) của nhân viên {employee.FullName}." });
        }

        // GET: /Employee/Detail/{id}
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") && employee.BranchId != loggedInBranchId)
            {
                return Content("<div class='alert alert-danger m-3'>Bạn không có quyền xem chi tiết nhân viên thuộc chi nhánh khác.</div>", "text/html");
            }

            return PartialView("_EmployeeDetail", employee);
        }

        // GET: /Employee/UpdateForm/{id}
        [HttpGet]
        public async Task<IActionResult> UpdateForm(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") && employee.BranchId != loggedInBranchId)
            {
                return Content("<div class='alert alert-danger m-3'>Bạn không có quyền cập nhật nhân viên thuộc chi nhánh khác.</div>", "text/html");
            }

            return PartialView("_EmployeeUpdateForm", employee);
        }

        // POST: /Employee/Update/{id}
        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateEmployeeDto dto)
        {
            if (dto == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu nhập vào chưa đúng định dạng." });
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") && employee.BranchId != loggedInBranchId)
            {
                return Json(new { success = false, errorMessage = "Bạn không có quyền cập nhật nhân viên thuộc chi nhánh khác." });
            }

            var result = await _employeeService.UpdateEmployeeInfoAsync(id, dto);
            if (result.Success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errorMessage = result.ErrorMessage });
        }

        // GET: /Employee/Permissions
        public async Task<IActionResult> Permissions()
        {
            var loggedInBranchId = await GetUserBranchIdAsync();
            var employees = await _employeeService.GetEmployeesListAsync();
            var filteredEmployees = System.Linq.Enumerable.Where(employees, e => (User.IsInRole("RManager") || e.BranchId == loggedInBranchId) && e.Role != "BranchManager" && e.Status == "Active");
            return View(filteredEmployees);
        }

        // POST: /Employee/UpdatePermissions
        [HttpPost]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdatePermissionsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (employee == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
            }

            var loggedInBranchId = await GetUserBranchIdAsync();
            if (!User.IsInRole("RManager") && employee.BranchId != loggedInBranchId)
            {
                return Json(new { success = false, errorMessage = "Bạn không có quyền thay đổi quyền hạn của nhân viên thuộc chi nhánh khác." });
            }

            var result = await _employeeService.UpdatePermissionsAsync(model.EmployeeId, model.Role, model.EmploymentType);
            if (result.Success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errorMessage = result.ErrorMessage });
        }

        // GET: /Employee/CheckDeactivation/{id}
        [HttpGet]
        public async Task<IActionResult> CheckDeactivation(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
            }

            var loggedInBranchId = User.GetBranchId() ?? "";
            if (employee.BranchId != loggedInBranchId)
            {
                return Json(new { success = false, errorMessage = "Bạn không có quyền vô hiệu hóa nhân viên thuộc chi nhánh khác." });
            }

            var (canDeactivate, uncompletedShifts) = await _employeeService.CheckDeactivationConstraintsAsync(id);
            return Json(new
            {
                success = true,
                employeeId = employee.EmployeeId,
                fullName = employee.FullName,
                username = employee.Username,
                role = employee.Role,
                canDeactivate = canDeactivate,
                uncompletedShifts = uncompletedShifts
            });
        }

        // POST: /Employee/Deactivate
        [HttpPost]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateRequestDto request)
        {
            if (request == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
            }

            var loggedInBranchId = User.GetBranchId() ?? "";
            if (employee.BranchId != loggedInBranchId)
            {
                return Json(new { success = false, errorMessage = "Bạn không có quyền vô hiệu hóa nhân viên thuộc chi nhánh khác." });
            }

            var (canDeactivate, _) = await _employeeService.CheckDeactivationConstraintsAsync(request.EmployeeId);
            if (!canDeactivate)
            {
                return Json(new { success = false, errorMessage = "Không thể vô hiệu hóa vì nhân viên còn ca làm việc chưa hoàn thành." });
            }

            var success = await _employeeService.DeactivateEmployeeAsync(request.EmployeeId, request.Reason, request.Notes ?? "");
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errorMessage = "Đã xảy ra lỗi khi vô hiệu hóa tài khoản." });
        }
    }

    public class UpdatePermissionsModel
    {
        public int EmployeeId { get; set; }
        public string Role { get; set; } = null!;
        public string EmploymentType { get; set; } = null!;
    }

    public class SaveFaceDataModel
    {
        public int EmployeeId { get; set; }
        public string FaceData { get; set; } = null!;
    }
}
