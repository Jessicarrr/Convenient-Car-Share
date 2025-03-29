using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConvenientCarShare.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context,
                                  UserManager<ApplicationUser> userManager,
                                  ICustomerService customerService)
        {
            _userManager = userManager;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var parkingAreasAndCars = await _customerService.GetParkingAreasAndCarsAsync();
            var model = new IndexModel()
            {
                ParkingInfo = parkingAreasAndCars
            };

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            model.HasBookingsWithoutReturns = await _customerService.HasBookingsWithoutReturnsAsync(currentUser);

            return View(model);
        }

        public async Task<JsonResult> GetCarsNotBookedDuring(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return Json(new { error = "The end date must be later than the start date." });
            }

            var availableCars = await _customerService.GetCarsNotBookedDuringAsync(startDate, endDate);
            // Serialize the list to JSON.
            var jsonResult = JsonConvert.SerializeObject(availableCars);
            return Json(jsonResult);
        }
    }
}
