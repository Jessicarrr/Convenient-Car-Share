using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Services
{
    public class StartBookingService : IStartBookingService
    {
        private readonly ApplicationDbContext _context;

        public StartBookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, int statusCode, string message)> StartBookingAsync(string activationCode)
        {
            if (string.IsNullOrEmpty(activationCode))
            {
                return (false, 400, "A code must be supplied for starting the booking.");
            }

            // find the booking with the given activation code including its Car.
            var currBooking = await _context.Bookings
                .Where(b => b.ActivationCode == activationCode)
                .Include(b => b.Car)
                .FirstOrDefaultAsync();

            if (currBooking == null)
            {
                return (false, 404, "Something went wrong.");
            }

            // check booking time: must be within the booking period.
            if (currBooking.StartDate.CompareTo(DateTime.Now) > 0 || currBooking.EndDate.CompareTo(DateTime.Now) < 0)
            {
                return (false, 400, "Current time is not in the booking period!");
            }

            // check if the booking has already been finished.
            if (currBooking.ReturnArea != null)
            {
                return (false, 400, "The booking has already finished!");
            }

            // check if the booking was cancelled.
            if (currBooking.Status == Constants.statusCancelled)
            {
                return (false, 400, "The booking has already cancelled!");
            }

            // find the car and its parking info.
            var currCar = await _context.Cars
                .Where(car => car.Id == currBooking.Car.Id)
                .Include(car => car.CurrentlyParkedAt)
                .FirstOrDefaultAsync();

            if (currCar.CurrentlyParkedAt == null)
            {
                return (false, 400, "The car is already unlocked!");
            }

            // process the start booking: unlock the car and update status.
            currCar.CurrentlyParkedAt = null;
            currBooking.Status = Constants.statusDriving;

            try
            {
                _context.Update(currBooking);
                _context.Update(currCar);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return (false, 500, "An error occurred while unlocking the car.");
            }

            return (true, 200, "Your car is unlocked! Enjoy! To view more details, feel free to visit the 'Manage Trips' page on our website.");
        }
    }
}