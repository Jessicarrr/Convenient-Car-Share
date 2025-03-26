using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Services
{
    public class ExtendBookingService : IExtendBookingService
    {
        private readonly ApplicationDbContext _context;

        public ExtendBookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BookingBelongsToUser(int bookingId, string userId)
        {
            // Find the booking including its associated Car and ReturnArea.
            var currBooking = await _context.Bookings
                .Where(b => b.Id == bookingId)
                .Include(b => b.Car)
                .Include(b => b.ReturnArea)
                .FirstOrDefaultAsync();

            if (currBooking == null || currBooking.User == null || String.IsNullOrEmpty(userId))
            {
                return false;
            }

            var bookingUserId = currBooking.User.Id;

            if (bookingUserId.Equals(userId))
            {
                return true;
            }

            return false;
        }

        public async Task<(string status, decimal price)> CheckExtensionAvailabilityAsync(int bookingId, int extendTime)
        {
            // Find the booking including its associated Car and ReturnArea.
            var currBooking = await _context.Bookings
                .Where(b => b.Id == bookingId)
                .Include(b => b.Car)
                .Include(b => b.ReturnArea)
                .FirstOrDefaultAsync();

            if (currBooking == null)
            {
                throw new ArgumentException("Booking not found");
            }

            // Determine the new end time for checking.
            DateTime newTime = currBooking.ExtensionDate.Ticks != 0
                ? currBooking.ExtensionDate.AddHours(extendTime)
                : currBooking.EndDate.AddHours(extendTime);

            // Check if any other booked booking (status booked) for the same car exists before the newTime.
            var followingBookings = await _context.Bookings
                .Where(b => b.Status == Constants.statusBooked)
                .Where(b => b.StartDate.Ticks <= newTime.Ticks)
                .Where(b => b.Car.Id == currBooking.Car.Id)
                .ToArrayAsync();

            // Calculate extension price
            decimal price = currBooking.Car.Price * extendTime;

            if (followingBookings.Length != 0)
            {
                return ("Not Available", price);
            }
            else
            {
                return ("Available", price);
            }
        }

        public async Task<Booking> ExtendBookingAsync(int bookingId, int extendHour)
        {
            // retrieve the booking with its associated Car and ReturnArea.
            var booking = await _context.Bookings
                .Where(b => b.Id == bookingId)
                .Include(b => b.Car)
                .Include(b => b.ReturnArea)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                throw new ArgumentException("Booking not found");
            }

            // if the booking hasn't been extended yet
            if (booking.ExtensionDate.Ticks == 0)
            {
                booking.ExtensionDate = booking.EndDate.AddHours(extendHour);
                booking.ExtensionPrice = booking.Car.Price * extendHour;
            }
            else
            {
                // if already extended, we can just extend the existing extension
                booking.ExtensionDate = booking.ExtensionDate.AddHours(extendHour);
                booking.ExtensionPrice = booking.Car.Price + booking.Car.Price * extendHour;
            }

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return booking;
        }
    }
}
