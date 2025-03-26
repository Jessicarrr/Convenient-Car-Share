using System.Threading.Tasks;

namespace ConvenientCarShare.Services
{
    public interface IStartBookingService
    {
        /// <summary>
        /// Attempts to start a booking given an activation code.
        /// Returns a (success, statusCode, message) tuple.
        /// </summary>
        Task<(bool success, int statusCode, string message)> StartBookingAsync(string activationCode);
    }
}
