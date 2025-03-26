using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using Xunit;

namespace Test_Xunit_CCS
{
    public class StartBookingServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly StartBookingService _startBookingService;

        public StartBookingServiceTests()
        {
            _context = TestHelpers.GetSqliteInMemoryContext();
            _startBookingService = new StartBookingService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsBadRequest_WhenActivationCodeIsEmpty()
        {
            string activationCode = "";

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("A code must be supplied", result.message);
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsNotFound_WhenBookingDoesNotExist()
        {
            string activationCode = "non-existent-code";

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Contains("Something went wrong", result.message);
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsBadRequest_WhenNotInBookingPeriod()
        {
            // create a booking with a future start date.
            var car = TestHelpers.GetDummyCar(_context);
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);

            // set activation code for testing.
            string activationCode = "future-code";
            var booking = new Booking
            {
                ActivationCode = activationCode,
                Car = car,
                User = user,
                StartDate = DateTime.Now.AddHours(1),  // starts in the future
                EndDate = DateTime.Now.AddHours(2),
                Status = Constants.statusBooked
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("Current time is not in the booking period", result.message);
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsBadRequest_WhenBookingAlreadyFinished()
        {
            // create a booking with a ReturnArea (indicating finished)
            var car = TestHelpers.GetDummyCar(_context);
            string activationCode = "finished-code";
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);
            var parkingArea = TestHelpers.GetDummyParkingArea(_context);

            var booking = new Booking
            {
                ActivationCode = activationCode,
                Car = car,
                User = user,
                StartDate = DateTime.Now.AddHours(-2),
                EndDate = DateTime.Now.AddHours(2),
                ReturnArea = parkingArea,
                Status = Constants.statusBooked
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("already finished", result.message);
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsBadRequest_WhenBookingCancelled()
        {
            // create a booking with status cancelled.
            var car = TestHelpers.GetDummyCar(_context);
            string activationCode = "cancelled-code";
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);
            var booking = new Booking
            {
                ActivationCode = activationCode,
                Car = car,
                User = user,
                StartDate = DateTime.Now.AddHours(-2),
                EndDate = DateTime.Now.AddHours(2),
                Status = Constants.statusCancelled
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("cancelled", result.message);
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsBadRequest_WhenCarAlreadyUnlocked()
        {
            var car = TestHelpers.GetDummyCar(_context);
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);

            car.CurrentlyParkedAt = null;
            _context.Update(car);
            await _context.SaveChangesAsync();

            string activationCode = "unlocked-code";
            var booking = new Booking
            {
                ActivationCode = activationCode,
                Car = car,
                User = user,
                StartDate = DateTime.Now.AddHours(-1),
                EndDate = DateTime.Now.AddHours(1),
                Status = Constants.statusBooked
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("already unlocked", result.message);
        }

        [Fact]
        public async Task StartBookingAsync_ReturnsOk_WhenBookingStartsSuccessfully()
        {
            // create a valid booking that is currently in its booking period
            var car = TestHelpers.GetDummyCar(_context);
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);

            // ensure the car is locked (has a valid CurrentlyParkedAt)
            var parkingArea = TestHelpers.GetDummyParkingArea(_context);
            car.CurrentlyParkedAt = parkingArea;
            _context.Update(car);
            await _context.SaveChangesAsync();

            string activationCode = "valid-code";
            var booking = new Booking
            {
                ActivationCode = activationCode,
                Car = car,
                User = user,
                StartDate = DateTime.Now.AddHours(-1),
                EndDate = DateTime.Now.AddHours(1),
                Status = Constants.statusBooked
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _startBookingService.StartBookingAsync(activationCode);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Contains("unlocked", result.message);

            // Additionally, verify that the car is now unlocked and booking status updated.
            var updatedBooking = _context.Bookings.FirstOrDefault(b => b.ActivationCode == activationCode);
            var updatedCar = _context.Cars.FirstOrDefault(c => c.Id == car.Id);

            Assert.NotNull(updatedCar);
            Assert.NotNull(updatedBooking);
            Assert.Equal(Constants.statusDriving, updatedBooking.Status);
            Assert.Null(updatedCar.CurrentlyParkedAt);
        }
    }
}
