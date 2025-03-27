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
    public class ReturnServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ReturnService _returnService;

        public ReturnServiceTests()
        {
            _context = TestHelpers.GetSqliteInMemoryContext();
            _returnService = new ReturnService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAvailableParkingAreasAsync_Returns_AllSpots_WithAvailableCapacity()
        {
            var fullSpot = TestHelpers.GetDummyParkingArea(_context, capacity: 1);
            var availableSpot = TestHelpers.GetDummyParkingArea(_context, capacity: 5);

            var carInFullSpot = TestHelpers.GetDummyCar(_context);
            carInFullSpot.CurrentlyParkedAt = fullSpot;
            _context.Update(carInFullSpot);
            await _context.SaveChangesAsync();

            var carInAvailableSpot = TestHelpers.GetDummyCar(_context);
            carInAvailableSpot.CurrentlyParkedAt = availableSpot;
            _context.Update(carInAvailableSpot);
            await _context.SaveChangesAsync();

            // get available parking areas from the service
            var availableAreas = await _returnService.GetAvailableParkingAreasAsync();

            // availableSpot should be in the list; fullSpot should be omitted
            Assert.Contains(availableSpot, availableAreas);
            Assert.DoesNotContain(fullSpot, availableAreas);
        }

        [Fact]
        public async Task ProcessReturnAsync_UpdatesBookingAndCarCorrectly()
        {
            var returnSpot = TestHelpers.GetDummyParkingArea(_context, capacity: 10);
            var user = TestHelpers.GetDummyUser("user1", "user1@test.com", _context);
            var car = TestHelpers.GetDummyCar(_context);

            // set an initial parking area for the car
            var initialSpot = TestHelpers.GetDummyParkingArea(_context, capacity: 5);
            car.CurrentlyParkedAt = initialSpot;
            _context.Update(car);
            await _context.SaveChangesAsync();

            var booking = TestHelpers.GetDummyBooking(
                _context,
                user,
                car,
                DateTime.Now.AddHours(-3),
                DateTime.Now.AddHours(-1),
                Constants.statusDriving
            );

            var updatedBooking = await _returnService.ProcessReturnAsync(booking.Id, returnSpot.Id);
            var updatedCar = _context.Cars.FirstOrDefault(c => c.Id == car.Id);

            Assert.Equal(returnSpot.Id, updatedBooking.ReturnArea.Id);
            Assert.Equal(Constants.statusFinished, updatedBooking.Status);

            // verify car's parking spot and coordinates are updated.
            Assert.NotNull(updatedCar);
            Assert.NotNull(updatedCar.CurrentlyParkedAt);
            Assert.Equal(returnSpot.Id, updatedCar.CurrentlyParkedAt.Id);
            Assert.Equal(returnSpot.Latitude, updatedCar.Latitude);
            Assert.Equal(returnSpot.Longitude, updatedCar.Longitude);
        }

        [Fact]
        public async Task ProcessReturnAsync_ThrowsException_WhenBookingNotFound()
        {
            // use an invalid booking ID.
            int invalidBookingId = -1;
            var returnSpot = TestHelpers.GetDummyParkingArea(_context, capacity: 10);

            // should throw an exception
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _returnService.ProcessReturnAsync(invalidBookingId, returnSpot.Id));
        }

        [Fact]
        public async Task ProcessReturnAsync_ThrowsException_WhenParkingSpotNotFound()
        {
            var user = TestHelpers.GetDummyUser("user2", "user2@test.com", _context);
            var car = TestHelpers.GetDummyCar(_context);
            var booking = TestHelpers.GetDummyBooking(
                _context,
                user,
                car,
                DateTime.Now.AddHours(-3),
                DateTime.Now.AddHours(-1),
                Constants.statusDriving
            );

            int invalidSpotId = -1;

            // should throw an exception.
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _returnService.ProcessReturnAsync(booking.Id, invalidSpotId));
        }
    }
}
