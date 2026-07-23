using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEP490_G52_CSMS.Services;

namespace SEP490_G52_CSMS.Controllers
{
    public class BrewingController : Controller
    {
        private readonly IOrderService _orderService;
        
        public BrewingController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var waitingAndBrewingOrders = await _orderService.GetWaitingAndBrewingOrdersAsync();
            return View(waitingAndBrewingOrders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            var details = await _orderService.GetOrderDetailsAsync(orderId);
            if (details == null) return NotFound();
            
            return PartialView("_BrewingOrderDetailPartial", details);
        }

        [HttpPost]
        public async Task<IActionResult> StartBrewing(string orderId)
        {
            var result = await _orderService.StartBrewingAsync(orderId);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Không thể bắt đầu pha chế đơn hàng này." });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteBrewing(string orderId)
        {
            var result = await _orderService.CompleteBrewingAsync(orderId);
            if (result) return Json(new { success = true });
            return BadRequest(new { success = false, message = "Không thể hoàn thành đơn hàng này." });
        }
    }
}
