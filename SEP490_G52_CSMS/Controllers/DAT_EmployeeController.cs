using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class DAT_EmployeeController : Controller
    {
        private readonly IDAT_EmployeeService _employeeService;
        private readonly Repositories.IDAT_EmployeeRepository _employeeRepository;

        public DAT_EmployeeController(IDAT_EmployeeService employeeService, Repositories.IDAT_EmployeeRepository employeeRepository)
        {
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
        }

        // GET: /DAT_Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetEmployeesListAsync();
            // Chỉ hiển thị nhân viên thuộc chi nhánh Quận 1 (CB001) và loại bỏ những người là Quản lý chi nhánh
            var filteredEmployees = System.Linq.Enumerable.Where(employees, e => e.BranchId == "CB001" && e.Role != "BranchManager");
            return View(filteredEmployees);
        }

        // GET: /DAT_Employee/GetBranches
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _employeeService.GetBranchesAsync();
            return Json(branches);
        }

        // POST: /DAT_Employee/CreateAccount
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] DAT_EmployeeCreationDto dto)
        {
            if (dto == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu nhập vào chưa đúng định dạng." });
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

        // GET: /DAT_Employee/RegisterFace/{id}
        [HttpGet]
        public async Task<IActionResult> RegisterFace(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // POST: /DAT_Employee/SaveFaceData
        [HttpPost]
        public async Task<IActionResult> SaveFaceData([FromBody] DAT_SaveFaceDataModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FaceData))
            {
                return Json(new { success = false, errorMessage = "Dữ liệu khuôn mặt không hợp lệ." });
            }

            var success = await _employeeService.RegisterFaceDataAsync(model.EmployeeId, model.FaceData);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
        }

        // GET: /DAT_Employee/Detail/{id}
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            // BR01 (Data Visibility Scope): Chỉ quản lý thuộc chi nhánh của nhân viên mới được xem chi tiết (mặc định CB001)
            if (employee.BranchId != "CB001")
            {
                return Content("<div class='alert alert-danger m-3'>Bạn không có quyền xem chi tiết nhân viên thuộc chi nhánh khác.</div>", "text/html");
            }

            return PartialView("_EmployeeDetail", employee);
        }

        // GET: /DAT_Employee/UpdateForm/{id}
        [HttpGet]
        public async Task<IActionResult> UpdateForm(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            // BR07: Chỉ quản lý thuộc chi nhánh của nhân viên mới được cập nhật (mặc định CB001)
            if (employee.BranchId != "CB001")
            {
                return Content("<div class='alert alert-danger m-3'>Bạn không có quyền cập nhật nhân viên thuộc chi nhánh khác.</div>", "text/html");
            }

            return PartialView("_EmployeeUpdateForm", employee);
        }

        // POST: /DAT_Employee/Update/{id}
        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromForm] DAT_UpdateEmployeeDto dto)
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

            // BR07: Chỉ quản lý thuộc chi nhánh của nhân viên mới được cập nhật (mặc định CB001)
            if (employee.BranchId != "CB001")
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

        // GET: /DAT_Employee/Permissions
        public async Task<IActionResult> Permissions()
        {
            var employees = await _employeeService.GetEmployeesListAsync();
            // Show only branch CB001, exclude BranchManager, and exclude Inactive employees
            var filteredEmployees = System.Linq.Enumerable.Where(employees, e => e.BranchId == "CB001" && e.Role != "BranchManager" && e.Status == "Active");
            return View(filteredEmployees);
        }

        // POST: /DAT_Employee/UpdatePermissions
        [HttpPost]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdatePermissionsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ." });
            }

            var result = await _employeeService.UpdatePermissionsAsync(model.EmployeeId, model.Role, model.EmploymentType);
            if (result.Success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errorMessage = result.ErrorMessage });
        }

        // GET: /DAT_Employee/CheckDeactivation/{id}
        [HttpGet]
        public async Task<IActionResult> CheckDeactivation(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return Json(new { success = false, errorMessage = "Không tìm thấy nhân viên." });
            }

            // BR03: Branch Managers can only deactivate employee records belonging to their managed branch.
            if (employee.BranchId != "CB001")
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

        // POST: /DAT_Employee/Deactivate
        [HttpPost]
        public async Task<IActionResult> Deactivate([FromBody] DAT_DeactivateRequestDto request)
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

            // BR03: Check branch
            if (employee.BranchId != "CB001")
            {
                return Json(new { success = false, errorMessage = "Bạn không có quyền vô hiệu hóa nhân viên thuộc chi nhánh khác." });
            }

            // BR02: Verify constraints
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

    public class DAT_SaveFaceDataModel
    {
        public int EmployeeId { get; set; }
        public string FaceData { get; set; } = null!;
    }
}
