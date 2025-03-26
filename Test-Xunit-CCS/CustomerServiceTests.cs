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
    public class CustomerServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _context = TestHelpers.GetSqliteInMemoryContext();
            _customerService = new CustomerService(_context);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task GetParkingAreasAndCarsAsync_Returns_CorrectParkingInfo()
        {
            var parkingArea = TestHelpers.GetDummyParkingArea(_context);
            var car = new Car("Toyota", "Corolla", "Red", "ABC123", 20.0, 20.0, 50.0m, "Four")
            {
                CurrentlyParkedAt = parkingArea
            };

            _context.Add(car);
            await _context.SaveChangesAsync();

            var result = await _customerService.GetParkingAreasAndCarsAsync();

            Assert.NotNull(result);
            Assert.True(result.ContainsKey(parkingArea));
            Assert.Contains(car, result[parkingArea]);
        }

        [Fact]
        public async Task GetCarsNotBookedDuringAsync_Excludes_BookedCars()
        {
            var parkingArea = TestHelpers.GetDummyParkingArea(_context);
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);
            var car = new Car("Honda", "Civic", "Blue", "XYZ789", 25.0, 25.0, 60.0m, "Four")
            {
                CurrentlyParkedAt = parkingArea
            };
            _context.Add(car);
            await _context.SaveChangesAsync();

            // Create a booking that should make the car unavailable.
            TestHelpers.GetDummyBooking(_context, user, car, DateTime.Now.AddMinutes(10), DateTime.Now.AddHours(1), Constants.statusBooked);

            var availableCars = await _customerService.GetCarsNotBookedDuringAsync(DateTime.Now, DateTime.Now.AddHours(2));
            Assert.DoesNotContain(car, availableCars);
        }

        [Fact]
        public async Task HasBookingsWithoutReturnsAsync_ReturnsTrue_WhenUserHasBooking()
        {
            var user = TestHelpers.GetDummyUser("tester-id", "test@example.com", _context);
            var car = TestHelpers.GetDummyCar(_context);
            TestHelpers.GetDummyBooking(_context, user, car, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(1), Constants.statusDriving);

            var hasBookings = await _customerService.HasBookingsWithoutReturnsAsync(user);
            Assert.True(hasBookings);
        }

        [Fact]
        public void IsCarBookedWithin_ReturnsTrue_WhenBookingOverlaps()
        {
            var car = TestHelpers.GetDummyCar(_context);
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);
            var booking = TestHelpers.GetDummyBooking(_context, user, car, DateTime.Now.AddHours(1), DateTime.Now.AddHours(3), Constants.statusBooked);
            var bookings = new Booking[] { booking };

            bool result = _customerService.IsCarBookedWithin(DateTime.Now, DateTime.Now.AddHours(2), bookings, car);
            Assert.True(result);
        }

        [Fact]
        public void IsCarBookedWithin_ReturnsFalse_WhenNoOverlap()
        {
            var car = TestHelpers.GetDummyCar(_context);
            var user = TestHelpers.GetDummyUser("test123", "test@gmail.com", _context);
            var booking = TestHelpers.GetDummyBooking(_context, user, car, DateTime.Now.AddHours(3), DateTime.Now.AddHours(4), Constants.statusBooked);
            var bookings = new Booking[] { booking };

            bool result = _customerService.IsCarBookedWithin(DateTime.Now, DateTime.Now.AddHours(2), bookings, car);
            Assert.False(result);
        }
    }
}
