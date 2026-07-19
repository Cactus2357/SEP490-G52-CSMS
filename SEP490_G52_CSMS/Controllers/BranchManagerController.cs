using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Employees;

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
            return View(managers);
        }
    }
}
