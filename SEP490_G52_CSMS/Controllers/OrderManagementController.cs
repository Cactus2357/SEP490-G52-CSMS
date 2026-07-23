using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class OrderManagementController : Controller
    {
        private readonly IOrderManagementService _orderManagementService;

        public OrderManagementController(IOrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }

        public async Task<IActionResult> Index(string searchCashier = "", string status = "Tất cả")
        {
            // Hardcode branch context for now as "BR001" and "Chi nhánh 1"
            string branchId = "CB002";
            string branchName = "Chi nhánh 1";

            var model = await _orderManagementService.GetOrderManagementListAsync(branchId, branchName, searchCashier, status);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string orderId)
        {
            string branchId = "CB002"; // Hardcoded branch context
            var model = await _orderManagementService.GetOrderDetailAsync(orderId, branchId);
            
            if (model == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            return PartialView("_OrderDetailModal", model);
        }
    }
}
