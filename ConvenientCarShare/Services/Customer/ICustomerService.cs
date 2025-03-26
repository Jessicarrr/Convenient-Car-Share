using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;

namespace ConvenientCarShare.Services
{
    public interface ICustomerService
    {
        /// <summary>
        /// Returns a dictionary of ParkingAreas and the available Cars in them.
        /// </summary>
        Task<Dictionary<ParkingArea, List<Car>>> GetParkingAreasAndCarsAsync();

        /// <summary>
        /// Returns a list of Cars that are not booked during the given date range.
        /// </summary>
        Task<List<Car>> GetCarsNotBookedDuringAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Checks if the given car has any booking overlapping the specified date range.
        /// </summary>
        bool IsCarBookedWithin(DateTime startDate, DateTime endDate, Booking[] bookings, Car car);

        /// <summary>
        /// Checks if the user has any current bookings (driving or booked without return).
        /// </summary>
        Task<bool> HasBookingsWithoutReturnsAsync(ApplicationUser user);
    }
}
