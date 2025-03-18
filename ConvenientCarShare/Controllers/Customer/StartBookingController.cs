using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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

        public async Task<String> OnGet(string activicationCode)
        {
            if (activicationCode == null)
            {
                return "A code must be supplied for starting the booking.";
            }
            else
            {
                var currBooking = await _context.Bookings
                .Where(booking => booking.activicationCode == activicationCode)
                .Include(booking => booking.Car).FirstOrDefaultAsync();

                if (currBooking.StartDate.CompareTo(DateTime.Now) > 0 || currBooking.EndDate.CompareTo(DateTime.Now) < 0)
                {
                    return "Current time is not in the booking period!!";

                }

                if (currBooking.ReturnArea != null)
                {
                    return "The booking has already finished!";

                }

                if (currBooking.status == Constants.statusCancelled)
                {

                    return "The booking has already cancelled!";
                }


                var currCar = await _context.Cars
                .Where(car => car.Id == currBooking.Car.Id).Include(car => car.CurrentlyParkedAt).FirstOrDefaultAsync();

                if (currCar.CurrentlyParkedAt == null)
                {
                    return "The car is already unlocked!";
                }



                currCar.CurrentlyParkedAt = null;
                currBooking.status = Constants.statusDriving;

                _context.Update(currBooking);
                _context.Update(currCar);
                

                await _context.SaveChangesAsync();
                return ("The car is unlocked! Enjoy!");

            }
        }
    }
}