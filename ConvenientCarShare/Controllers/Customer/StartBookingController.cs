using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConvenientCarShare.Data;

namespace ConvenientCarShare.Controllers.Customer
{
    public class StartBookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StartBookingController(ApplicationDbContext context)
        {
            _context = context;


        }

        [HttpGet]
        public async Task<IActionResult> OnGet(string ActivationCode)
        {
            if (string.IsNullOrEmpty(ActivationCode))
            {
                return BadRequest("A code must be supplied for starting the booking.");
            }

            var currBooking = await _context.Bookings
            .Where(booking => booking.ActivationCode == ActivationCode)
            .Include(booking => booking.Car).FirstOrDefaultAsync();

            if (currBooking == null)
            {
                return NotFound("Something went wrong.");
            }

            if (currBooking.StartDate.CompareTo(DateTime.Now) > 0 || currBooking.EndDate.CompareTo(DateTime.Now) < 0)
            {
                return BadRequest("Current time is not in the booking period!");

            }

            if (currBooking.ReturnArea != null)
            {
                return BadRequest("The booking has already finished!");

            }

            if (currBooking.Status == Constants.statusCancelled)
            {
                return BadRequest("The booking has already cancelled!");
            }


            var currCar = await _context.Cars
            .Where(car => car.Id == currBooking.Car.Id).Include(car => car.CurrentlyParkedAt).FirstOrDefaultAsync();

            if (currCar.CurrentlyParkedAt == null)
            {
                return BadRequest("The car is already unlocked!");
            }



            currCar.CurrentlyParkedAt = null;
            currBooking.Status = Constants.statusDriving;

            try
            {
                _context.Update(currBooking);
                _context.Update(currCar);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return StatusCode(500, "An error occurred while unlocking the car.");
            }
                
            return Ok("The car is unlocked! Enjoy!");

        }
    }
}