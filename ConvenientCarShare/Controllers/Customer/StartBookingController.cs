using System.Threading.Tasks;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConvenientCarShare.Controllers.Customer
{
    public class StartBookingController : Controller
    {
        private readonly IStartBookingService _startBookingService;

        public StartBookingController(ApplicationDbContext context, IStartBookingService startBookingService)
        {
            _startBookingService = startBookingService;
        }

        [HttpGet]
        public async Task<IActionResult> OnGet(string ActivationCode)
        {
            var result = await _startBookingService.StartBookingAsync(ActivationCode);

            // Return appropriate IActionResult based on the service's result.
            if (!result.success)
            {
                // Use StatusCode to return the provided status and message.
                return StatusCode(result.statusCode, result.message);
            }
            return Ok(result.message);
        }
    }
}
