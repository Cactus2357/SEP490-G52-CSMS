using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Repositories;

namespace SEP490_G52_CSMS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeService employeeService, IEmployeeRepository employeeRepository)
        {
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
        }

        // GET: /Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetEmployeesListAsync();
            // CB001 branch only, exclude BranchManager
            var filteredEmployees = System.Linq.Enumerable.Where(employees, e => e.BranchId == "CB001" && e.Role != "BranchManager");
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
        public async Task<IActionResult> CreateAccount([FromBody] EmployeeCreationDto dto)
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

        // GET: /Employee/RegisterFace/{id}
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

        // POST: /Employee/SaveFaceData
        [HttpPost]
        public async Task<IActionResult> SaveFaceData([FromBody] SaveFaceDataModel model)
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

        // GET: /Employee/Detail/{id}
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            if (employee.BranchId != "CB001")
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

            if (employee.BranchId != "CB001")
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

        // GET: /Employee/Permissions
        public async Task<IActionResult> Permissions()
        {
            var employees = await _employeeService.GetEmployeesListAsync();
            var filteredEmployees = System.Linq.Enumerable.Where(employees, e => e.BranchId == "CB001" && e.Role != "BranchManager" && e.Status == "Active");
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

            if (employee.BranchId != "CB001")
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
