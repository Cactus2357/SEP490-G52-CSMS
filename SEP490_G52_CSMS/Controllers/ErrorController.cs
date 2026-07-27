using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_G52_CSMS.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/404")]
        public IActionResult NotFound404()
        {
            return View("NotFound");
        }

        [Route("Error/500")]
        public IActionResult Error500()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred at path: {Path}", exceptionHandlerPathFeature.Path);
            }
            return View("Error");
        }

        [Route("Error/{code:int}")]
        public IActionResult HttpStatusCodeHandler(int code)
        {
            if (code == 404)
            {
                return RedirectToAction(nameof(NotFound404));
            }
            return View("Error");
        }
    }
}
