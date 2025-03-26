using System.Threading.Tasks;
using ConvenientCarShare.Models;

namespace ConvenientCarShare.Services
{
    public interface IExtendBookingService
    {
        /// <summary>
        /// Check that the booking's user id property matches the given userId.
        /// This can be good for checking if the user sending a request about a booking
        /// actually owns the booking itself.
        /// </summary>
        /// <param name="bookingId">The booking to check.</param>
        /// <param name="userId">The user requesting the booking.</param>
        /// <returns>True if the booking's user id matches the requesting user's id.</returns>
        Task<bool> BookingBelongsToUser(int bookingId, string userId);

        /// <summary>
        /// Checks whether a booking can be extended by a given number of hours,
        /// and returns the extension status and price.
        /// </summary>
        Task<(string status, decimal price)> CheckExtensionAvailabilityAsync(int bookingId, int extendTime);

        /// <summary>
        /// Applies an extension to a booking and persists changes.
        /// </summary>
        Task<Booking> ExtendBookingAsync(int bookingId, int extendHour);
    }
}
