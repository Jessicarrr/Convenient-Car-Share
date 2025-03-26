using System;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConvenientCarShare.Controllers.Customer
{
    [Authorize]
    public class ExtendController : Controller
    {
        private readonly IExtendBookingService _extendBookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExtendController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                IExtendBookingService extendBookingService)
        {
            _userManager = userManager;
            _extendBookingService = extendBookingService;
        }

        [HttpPost]
        public async Task<JsonResult> ExtendBooking(string bookingId, int extendTime)
        {
            var error = Json(new { status = "Something went wrong, we're sorry about that.", price = -1.0 });
            int bookingID = int.Parse(bookingId);

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser == null)
            {
                return error;
            }

            bool userIsValid = await _extendBookingService.BookingBelongsToUser(bookingID, currentUser.Id);

            if (!userIsValid)
            {
                return error;
            }

            var (status, price) = await _extendBookingService.CheckExtensionAvailabilityAsync(bookingID, extendTime);
            return Json(new { status, price });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExtendBookingOnPost(int bookingId, int extendHour)
        {
            var updatedBooking = await _extendBookingService.ExtendBookingAsync(bookingId, extendHour);

            TempData["msg"] = "<div class='alert alert-success alert-dismissable' style='text-align:center;'>" +
                "<button type='button' class='close' data-dismiss='alert' aria-hidden='true'>&times;</button>" +
                "Successfully extended! Price: $" + (updatedBooking.Car.Price * extendHour) +
                "</div>";

            return RedirectToAction("ManageTrips", "Bookings");
        }
    }
}
