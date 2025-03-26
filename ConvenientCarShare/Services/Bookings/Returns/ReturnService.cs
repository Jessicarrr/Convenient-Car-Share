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
    public class ReturnService : IReturnService
    {
        private readonly ApplicationDbContext _context;

        public ReturnService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ParkingArea>> GetAvailableParkingAreasAsync()
        {
            // get all cars with their parking areas
            var cars = await _context.Cars
                .Include(x => x.CurrentlyParkedAt)
                .ToArrayAsync();

            // get all parking spots
            var spots = await _context.ParkingAreas.ToArrayAsync();

            // create a dictionary mapping ParkingArea to list of parked cars
            Dictionary<ParkingArea, List<Car>> parkingAreasAndCars = new Dictionary<ParkingArea, List<Car>>();
            foreach (var car in cars)
            {
                var parkingArea = car.CurrentlyParkedAt;
                if (parkingArea == null)
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

            List<ParkingArea> availableSpots = new List<ParkingArea>();
            foreach (var spot in spots)
            {
                if (parkingAreasAndCars.TryGetValue(spot, out var carList))
                {
                    if (carList.Count >= spot.MaximumCars)
                    {
                        continue;
                    }
                }
                availableSpots.Add(spot);
            }

            return availableSpots;
        }

        public async Task<Booking> ProcessReturnAsync(int bookingId, int spotId)
        {
            var booking = await _context.Bookings
                .Where(y => y.Id == bookingId)
                .Include(x => x.Car)
                .Include(x => x.ReturnArea)
                .FirstOrDefaultAsync();

            if (booking == null)
                throw new ArgumentException("Booking not found");

            var spot = await _context.ParkingAreas
                .Where(x => x.Id == spotId)
                .FirstOrDefaultAsync();

            if (spot == null)
                throw new ArgumentException("Parking spot not found");

            // process the return
            booking.ReturnDate = DateTime.Now;
            booking.ReturnArea = spot;
            booking.Status = Constants.statusFinished;

            booking.Car.CurrentlyParkedAt = spot;
            booking.Car.Latitude = spot.Latitude;
            booking.Car.Longitude = spot.Longitude;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return booking;
        }
    }
}
