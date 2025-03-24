using System;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConvenientCarShare.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IBookingsService _bookingsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(IBookingsService bookingsService, UserManager<ApplicationUser> userManager)
        {
            _bookingsService = bookingsService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult> Cancel(int cancelBookingId)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var viewModel = await _bookingsService.CancelBookingAsync(cancelBookingId, currentUser);
            return View("~/Views/Customer/ManageTrips.cshtml", viewModel);
        }

        public async Task<ActionResult> ResendActivationCode(int id)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var viewModel = await _bookingsService.ResendActivationCodeAsync(
                id,
                currentUser,
                Url,
                Request.Scheme);
            return View("~/Views/Customer/ManageTrips.cshtml", viewModel);
        }

        public async Task<ActionResult> ManageTrips()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var viewModel = await _bookingsService.GetManageTripsModelAsync(currentUser);
            return View("/Views/Customer/ManageTrips.cshtml", viewModel);
        }

        public async Task<ActionResult> Book(int Id, string startTime, string endTime)
        {
            var viewModel = await _bookingsService.GetBookModelAsync(Id, startTime, endTime, TempData);
            return View("/Views/Customer/Book.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Payment(DateTime StartDate, DateTime EndDate, decimal Price, int CarId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "Customer");
            }
            var viewModel = await _bookingsService.GetPaymentModelAsync(CarId, StartDate, EndDate, Price, currentUser, TempData);
            return View("/Views/Customer/Payment.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitPayment(int CarId, decimal Price, DateTime StartDate, DateTime EndDate,
            string FullName, string CreditCardNumber, string cvv, DateTime ExpiryDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (!currentUser.EmailConfirmed)
            {
                return RedirectToAction("Index", "Customer");
            }
            var viewModel = await _bookingsService.SubmitPaymentAsync(
                CarId, Price, StartDate, EndDate,
                FullName, CreditCardNumber, cvv, ExpiryDate,
                currentUser, Url, Request.Scheme, TempData);
            return View("/Views/Customer/Payment.cshtml", viewModel);
        }
    }
}
