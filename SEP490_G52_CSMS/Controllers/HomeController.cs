using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CSMSAppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("RManager"))
            {
                return RedirectToAction("Revenue", "Reporting");
            }
            if (User.IsInRole("WManager") || User.IsInRole("WarehouseManager"))
            {
                return RedirectToAction("Index", "Warehouse");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null)
                {
                    string branchId = employee.BranchId ?? User.GetBranchId() ?? "CB001";
                    var branchManager = await _context.Employees
                        .FirstOrDefaultAsync(e => e.BranchId == branchId && e.Role == "BranchManager" && e.Status == "Active");

                    ViewBag.BranchName = employee.Branch?.BranchName ?? "Chi Nhánh CSMS";
                    ViewBag.BranchManagerName = branchManager?.FullName ?? "Quản lý Chi Nhánh";
                    ViewBag.BranchManagerPhone = branchManager?.PhoneNumber ?? "0988888888";
                    ViewBag.BranchManagerEmail = branchManager?.Email ?? "manager@csms.com";
                    ViewBag.EmployeeRole = employee.Role ?? "Cashier";
                    ViewBag.EmployeeName = employee.FullName ?? "Nhân viên";
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
