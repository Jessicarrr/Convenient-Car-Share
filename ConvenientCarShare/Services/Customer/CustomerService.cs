using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<ParkingArea, List<Car>>> GetParkingAreasAndCarsAsync()
        {
            var cars = await _context.Cars
                .Include(x => x.CurrentlyParkedAt)
                .ToArrayAsync();

            Dictionary<ParkingArea, List<Car>> parkingAreasAndCars = new Dictionary<ParkingArea, List<Car>>();

            DateTime now = DateTime.Now;

            DateTime startWindow = DateTime.Now.AddHours(-1);
            DateTime endWindow = DateTime.Now.AddHours(2);

            foreach (var car in cars)
            {
                var parkingArea = car.CurrentlyParkedAt;
                if (parkingArea == null)
                {
                    continue;
                }

                // heck for any conflicting bookings for this car
                var followingBookings = await _context.Bookings
                    .Where(booking => booking.Status != Constants.statusCancelled)
                    .Where(booking => startWindow.Ticks < booking.EndDate.Ticks && endWindow.Ticks > booking.StartDate.Ticks)
                    .Where(booking => booking.Car.Id == car.Id)
                    .ToArrayAsync();

                if (followingBookings.Any())
                {
                    continue;
                }

                if (parkingAreasAndCars.ContainsKey(parkingArea))
                {
                    parkingAreasAndCars[parkingArea].Add(car);
                }
                else
                {
                    parkingAreasAndCars[parkingArea] = new List<Car> { car };
                }
            }
            return parkingAreasAndCars;
        }

        public async Task<List<Car>> GetCarsNotBookedDuringAsync(DateTime startDate, DateTime endDate)
        {
            DateTime adjustedStart = startDate.AddHours(Constants.MinimumTimeBetweenBookings * -1);
            DateTime adjustedEnd = endDate.AddHours(Constants.MinimumTimeBetweenBookings);

            // retrieve all non-cancelled bookings
            var bookings = await _context.Bookings
                .Where(b => b.Status != Constants.statusCancelled)
                .ToArrayAsync();

            var cars = await _context.Cars
                            .Include(x => x.CurrentlyParkedAt)
                            .ToArrayAsync();

            List<Car> availableCars = new List<Car>();

            foreach (Car car in cars)
            {
                bool booked = IsCarBookedWithin(adjustedStart, adjustedEnd, bookings, car);
                if (!booked)
                {
                    availableCars.Add(car);
                }
            }
            return availableCars;
        }

        public bool IsCarBookedWithin(DateTime startDate, DateTime endDate, Booking[] bookings, Car car)
        {
            var carBookings = bookings.Where(booking => booking.Car == car).ToArray();

            foreach (var booking in carBookings)
            {
                // if the booking time overlaps with the window.
                if (booking.StartDate.Ticks < endDate.Ticks && booking.EndDate.Ticks > startDate.Ticks)
                {
                    if (booking.Status == Constants.statusCancelled)
                    {
                        continue;
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> HasBookingsWithoutReturnsAsync(ApplicationUser user)
        {
            if (user == null)
                return false;

            DateTime now = DateTime.Now;
            var noReturnBookings = await _context.Bookings
                .Where(booking => booking.User == user)
                .Where(booking => booking.Status == Constants.statusDriving || (booking.StartDate.Ticks <= now.Ticks && booking.Status == Constants.statusBooked))
                .ToArrayAsync();

            return noReturnBookings.Any();
        }
    }
}
