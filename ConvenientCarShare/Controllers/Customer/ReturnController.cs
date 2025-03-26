using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConvenientCarShare.Controllers.Customer
{
    [Authorize]
    public class ReturnController : Controller
    {
        private readonly IReturnService _returnService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReturnController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                IReturnService returnService)
        {
            _userManager = userManager;
            _returnService = returnService;
        }

        public async Task<ActionResult> ReturnCar(int id)
        {
            var parkingAreas = await _returnService.GetAvailableParkingAreasAsync();

            var viewModel = new ReturnCarModel()
            {
                BookingId = id,
                parkingAreas = parkingAreas
            };

            return View("/Views/Customer/ReturnCar.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReturnCarOnPost(int bookingId, int spotId)
        {
            var updatedBooking = await _returnService.ProcessReturnAsync(bookingId, spotId);

            TempData["msg"] = "<div class='alert alert-success alert-dismissable'>" +
                "<button type='button' class='close' data-dismiss='alert' aria-hidden='true'>&times;</button>" +
                "Successfully returned the car! Thank you!" +
                "</div>";

            return RedirectToAction("Index", "Customer");
        }
    }
}
