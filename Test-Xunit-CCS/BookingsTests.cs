using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Controllers;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Drawing;
using System.Security.Claims;

namespace Test_Xunit_CCS
{
    public class BookingsTests
    {
        [Fact]
        public async Task CancelBookingAsync_ReturnsError_WhenBookingNotFound()
        {
            var context = TestHelpers.GetSqliteInMemoryContext();

            // Create test users and car.
            var testUser = TestHelpers.GetDummyUser("test-id", "test@gmail.com", context);
            var testUser2 = TestHelpers.GetDummyUser("test-id-2", "tester2@gmail.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            // Create a booking that belongs to dummyUser and dummyCar
            context.Bookings.Add(new Booking
            {
                Id = 1,
                User = testUser,
                Car = testCar,
                StartDate = DateTime.Now.AddHours(1),
                EndDate = DateTime.Now.AddHours(2),
                Price = 100.00m,
                Status = Constants.statusBooked,
                ActivationCode = "ABC123"  // or any valid code
            });

            context.SaveChanges();

            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            var userManager = TestHelpers.GetUserManager();

            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            // Try to cancel the booking as a different user
            var result = await service.CancelBookingAsync(1, testUser2);

            // Assert: since the booking doesn't belong to testUser2, expect an error.
            Assert.Contains("An unknown error occurred", result.errors.FirstOrDefault());
        }
    }
}
