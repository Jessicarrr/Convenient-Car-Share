using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Controllers.Customer
{
    [Authorize]
    public class ExtendController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ExtendController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }


        [HttpPost]
        public async Task<JsonResult> ExtendBooking(string bookingId, int extendTime)
        {
            int bookingID = Int32.Parse(bookingId);

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var currBooking = await _context.Bookings.Where(y => y.Id == bookingID)
                .Include(x => x.Car)
                .Include(x => x.ReturnArea)
                .FirstOrDefaultAsync();

            Booking[] followingBookings;

            if (currBooking.ExtensionDate.Ticks != 0)
            {
                followingBookings = await _context.Bookings
               .Where(booking => booking.Status == Constants.statusBooked && booking.StartDate.Ticks <= currBooking.ExtensionDate.AddHours(extendTime).Ticks
               )
               .Where(booking => booking.Car.Id == currBooking.Car.Id)
               .ToArrayAsync();
            }

            else
            {
                followingBookings = await _context.Bookings
               .Where(booking => booking.Status == Constants.statusBooked && booking.StartDate.Ticks <= currBooking.EndDate.AddHours(extendTime).Ticks
               )
               .Where(booking => booking.Car.Id == currBooking.Car.Id)
               .ToArrayAsync();
            }
            

            if (followingBookings.Any())
            {

                return Json("Not Available");

            }
            else {

                return Json("Available");

            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExtendBookingOnPost(int bookingId, int extendHour)
        {

            var booking = await _context.Bookings.Where(y => y.Id == bookingId)
                .Include(x => x.Car)
                .Include(x => x.ReturnArea)
                .FirstOrDefaultAsync();

            //case not extended yet
            if (booking.ExtensionDate.Ticks == 0)
            {
                booking.ExtensionDate = booking.EndDate.AddHours(extendHour);
                booking.ExtensionPrice = booking.Car.Price * extendHour;
            }

            else {

                booking.ExtensionDate = booking.ExtensionDate.AddHours(extendHour);
                booking.ExtensionPrice = booking.Car.Price + booking.Car.Price * extendHour;
            }


            _context.Update(booking);

            await _context.SaveChangesAsync();

            TempData["msg"] = "<div class='alert alert-success alert - dismissable' style='text-align:center;'>" +
         "<button type = 'button' class='close' data-dismiss='alert'" +
                    "aria-hidden='true'>" +
                "&times;" +
            "</button>" +
                "Successfully extended! Price: $" + booking.Car.Price * extendHour + 
        "</div>";

            _context.Update(booking);

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageTrips", "Bookings");

        }







    }
}