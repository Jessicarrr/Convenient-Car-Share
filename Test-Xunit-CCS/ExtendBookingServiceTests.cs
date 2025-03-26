using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using Test_Xunit_CCS;
using Xunit;

namespace ExtendBookingTests
{
    public class ExtendBookingServiceTests
    {
        [Fact]
        public async Task BookingBelongsToUser_ReturnsTrue_ForMatchingUser()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            // Create a dummy user, car, and booking.
            var user = TestHelpers.GetDummyUser("user1", "user1@test.com", context);
            var car = TestHelpers.GetDummyCar(context);
            var booking = TestHelpers.GetDummyBooking(context, user, car, DateTime.Now, DateTime.Now.AddHours(2), Constants.statusBooked);

            var service = new ExtendBookingService(context);
            var result = await service.BookingBelongsToUser(booking.Id, user.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task BookingBelongsToUser_ReturnsFalse_ForNonMatchingUserOrMissingUserId()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            // Create two users, a car, and a booking.
            var user = TestHelpers.GetDummyUser("user1", "user1@test.com", context);
            var anotherUser = TestHelpers.GetDummyUser("user2", "user2@test.com", context);
            var car = TestHelpers.GetDummyCar(context);
            var booking = TestHelpers.GetDummyBooking(context, user, car, DateTime.Now, DateTime.Now.AddHours(2), Constants.statusBooked);

            var service = new ExtendBookingService(context);

            // Test with a wrong user.
            var resultWrong = await service.BookingBelongsToUser(booking.Id, anotherUser.Id);
            Assert.False(resultWrong);

            // Test with a missing user id.
            var resultNull = await service.BookingBelongsToUser(booking.Id, null);
            Assert.False(resultNull);
        }

        [Fact]
        public async Task CheckExtensionAvailabilityAsync_ThrowsException_WhenBookingNotFound()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var service = new ExtendBookingService(context);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CheckExtensionAvailabilityAsync(999, 2));
        }

        [Fact]
        public async Task CheckExtensionAvailabilityAsync_ReturnsNotAvailable_AndCorrectPrice_WhenConflictExists()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var user = TestHelpers.GetDummyUser("user1", "user1@test.com", context);
            var car = TestHelpers.GetDummyCar(context);

            // Create a booking to be extended.
            var booking = TestHelpers.GetDummyBooking(context, user, car, DateTime.Now, DateTime.Now.AddHours(2), Constants.statusBooked);

            // Create a conflicting booking that starts before the new extension end time.
            // newTime = booking.EndDate.AddHours(extendTime) and we use extendTime = 1.
            // By setting the conflict booking's start date between booking.EndDate and newTime, we simulate a conflict.
            var conflictBooking = new Booking
            {
                User = user,
                Car = car,
                StartDate = booking.EndDate.AddMinutes(30),
                EndDate = booking.EndDate.AddHours(2),
                Status = Constants.statusBooked
            };
            context.Bookings.Add(conflictBooking);
            context.SaveChanges();

            var service = new ExtendBookingService(context);
            int extendTime = 1;
            var (status, price) = await service.CheckExtensionAvailabilityAsync(booking.Id, extendTime);

            Assert.Equal("Not Available", status);
            Assert.Equal(car.Price * extendTime, price);
        }

        [Fact]
        public async Task ExtendBookingAsync_ThrowsException_WhenBookingNotFound()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var service = new ExtendBookingService(context);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.ExtendBookingAsync(999, 2));
        }

        [Fact]
        public async Task ExtendBookingAsync_ExtendsBooking_WhenNotAlreadyExtended()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var user = TestHelpers.GetDummyUser("user1", "user1@test.com", context);
            var car = TestHelpers.GetDummyCar(context);
            // Create a booking without a prior extension (ExtensionDate defaults to DateTime.MinValue/0 ticks).
            var booking = TestHelpers.GetDummyBooking(context, user, car, DateTime.Now, DateTime.Now.AddHours(2), Constants.statusBooked);

            var service = new ExtendBookingService(context);
            int extendHour = 2;
            var updatedBooking = await service.ExtendBookingAsync(booking.Id, extendHour);

            var expectedExtensionDate = booking.EndDate.AddHours(extendHour);
            var expectedPrice = car.Price * extendHour;

            Assert.Equal(expectedExtensionDate, updatedBooking.ExtensionDate);
            Assert.Equal(expectedPrice, updatedBooking.ExtensionPrice);
        }

        [Fact]
        public async Task ExtendBookingAsync_ExtendsBooking_WhenAlreadyExtended()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var user = TestHelpers.GetDummyUser("user1", "user1@test.com", context);
            var car = TestHelpers.GetDummyCar(context);
            // Create a booking and simulate it has already been extended.
            var booking = TestHelpers.GetDummyBooking(context, user, car, DateTime.Now, DateTime.Now.AddHours(2), Constants.statusBooked);
            booking.ExtensionDate = booking.EndDate.AddHours(1);
            booking.ExtensionPrice = car.Price * 1;
            context.Update(booking);
            context.SaveChanges();

            // Capture the original extension date for calculation.
            var originalExtensionDate = booking.ExtensionDate;

            var service = new ExtendBookingService(context);
            int extendHour = 2;
            var updatedBooking = await service.ExtendBookingAsync(booking.Id, extendHour);

            // According to the service: new ExtensionDate = originalExtensionDate + extendHour.
            var expectedExtensionDate = originalExtensionDate.AddHours(extendHour);
            // And new ExtensionPrice = car.Price + car.Price * extendHour.
            var expectedPrice = car.Price + car.Price * extendHour;

            Assert.Equal(expectedExtensionDate, updatedBooking.ExtensionDate);
            Assert.Equal(expectedPrice, updatedBooking.ExtensionPrice);
        }
    }
}
