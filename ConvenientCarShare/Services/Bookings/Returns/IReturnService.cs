using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Views.Customer;

namespace ConvenientCarShare.Services
{
    public interface IReturnService
    {
        /// <summary>
        /// Gets the available parking areas for returning a car.
        /// </summary>
        Task<List<ParkingArea>> GetAvailableParkingAreasAsync();

        /// <summary>
        /// Processes the return of a booking.
        /// </summary>
        Task<Booking> ProcessReturnAsync(int bookingId, int spotId);
    }
}
