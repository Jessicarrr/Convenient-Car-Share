using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Controllers;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Services;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Test_Xunit_CCS
{
    public class BookingsTests
    {
        [Fact]
        public async Task CancelBookingAsync_ReturnsError_WhenBookingNotFoundOrWrongUser()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();

            // Create test users and car.
            var testUser = TestHelpers.GetDummyUser("test-id", "test@gmail.com", context);
            var testUser2 = TestHelpers.GetDummyUser("test-id-2", "tester2@gmail.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            // Create a booking that belongs to testUser.
            context.Bookings.Add(new Booking
            {
                Id = 1,
                User = testUser,
                Car = testCar,
                StartDate = DateTime.Now.AddHours(1),
                EndDate = DateTime.Now.AddHours(2),
                Price = 100.00m,
                Status = Constants.statusBooked,
                ActivationCode = "ABC123"
            });
            context.SaveChanges();

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();

            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            // Test cancellation with wrong user.
            var resultWrongUser = await service.CancelBookingAsync(1, testUser2);
            Assert.Contains("An unknown error occurred", resultWrongUser.errors.FirstOrDefault());

            // Test cancellation for non-existent booking.
            var resultNonExistent = await service.CancelBookingAsync(999, testUser);
            Assert.Contains("An unknown error occurred", resultNonExistent.errors.FirstOrDefault());
        }

        [Fact]
        public async Task CancelBookingAsync_CancelsBooking_WhenValid()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("test-id", "test@gmail.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            // Create a booking that belongs to testUser.
            var booking = new Booking
            {
                Id = 2,
                User = testUser,
                Car = testCar,
                StartDate = DateTime.Now.AddHours(2),
                EndDate = DateTime.Now.AddHours(4),
                Price = 150.00m,
                Status = Constants.statusBooked,
                ActivationCode = "XYZ789"
            };
            context.Bookings.Add(booking);
            context.SaveChanges();

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var result = await service.CancelBookingAsync(2, testUser);
            Assert.Empty(result.errors);
            Assert.Contains("Successfully cancelled booking", result.messages.FirstOrDefault());
            Assert.Equal(Constants.statusCancelled, booking.Status);
        }

        [Fact]
        public async Task ResendActivationCodeAsync_ReturnsError_WhenBookingNotFoundOrWrongStatus()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("test-id", "test@gmail.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            // Create a booking that belongs to testUser.
            var booking = new Booking
            {
                Id = 3,
                User = testUser,
                Car = testCar,
                StartDate = DateTime.Now.AddHours(3),
                EndDate = DateTime.Now.AddHours(5),
                Price = 200.00m,
                Status = Constants.statusCancelled, // wrong status for email resend
                ActivationCode = "RESEND1"
            };
            context.Bookings.Add(booking);
            context.SaveChanges();

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://callbackurl");

            var result = await service.ResendActivationCodeAsync(3, testUser, urlHelperMock.Object, "http");
            Assert.Contains("booking cannot be activated", result.errors.FirstOrDefault());
        }

        [Fact]
        public async Task ResendActivationCodeAsync_SendsEmail_WhenValid()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("test-id", "test@gmail.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            // Create a valid booking.
            var booking = new Booking
            {
                Id = 4,
                User = testUser,
                Car = testCar,
                StartDate = DateTime.Now.AddHours(1),
                EndDate = DateTime.Now.AddHours(3),
                Price = 120.00m,
                Status = Constants.statusBooked,
                ActivationCode = "VALID1"
            };
            context.Bookings.Add(booking);
            context.SaveChanges();

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "OnGet" &&
                ctx.Controller == "StartBooking" &&
                ctx.Protocol == "http")))
                .Returns("http://callbackurl");


            var result = await service.ResendActivationCodeAsync(4, testUser, urlHelperMock.Object, "http");

            Assert.Empty(result.errors);
            Assert.Contains("A new email has been sent", result.messages.FirstOrDefault());
            Assert.NotNull(testUser.Email);

            emailSenderMock.Verify(es => es.SendEmailAsync(
                testUser.Email,
                "You May Now Unlock Your Car",
                It.Is<string>(s => s.Contains("http://callbackurl"))),
                Times.Once);
        }

        [Fact]
        public async Task GetManageTripsModelAsync_ReturnsUserBookings()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("user-1", "user1@gmail.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            // Add two bookings for the user.
            context.Bookings.AddRange(
                new Booking
                {
                    Id = 5,
                    User = testUser,
                    Car = testCar,
                    StartDate = DateTime.Now.AddHours(2),
                    EndDate = DateTime.Now.AddHours(3),
                    Price = 80.00m,
                    Status = Constants.statusBooked,
                    ActivationCode = "MT1"
                },
                new Booking
                {
                    Id = 6,
                    User = testUser,
                    Car = testCar,
                    StartDate = DateTime.Now.AddHours(4),
                    EndDate = DateTime.Now.AddHours(6),
                    Price = 150.00m,
                    Status = Constants.statusBooked,
                    ActivationCode = "MT2"
                });
            context.SaveChanges();

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var model = await service.GetManageTripsModelAsync(testUser);
            Assert.Equal(2, model.bookings.Length);
        }

        [Fact]
        public async Task GetBookModelAsync_ReturnsModel_WithErrorsFromTempData()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testCar = TestHelpers.GetDummyCar(context);

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            // Setup tempData with an error message.
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["ErrorMessage"] = "Sample error";

            var model = await service.GetBookModelAsync(testCar.Id, "2025-01-01", "2025-01-02", tempData);
            Assert.Equal("Sample error", model.Errors.FirstOrDefault());
        }

        [Fact]
        public async Task GetPaymentModelAsync_ReturnsValidModel_WhenValidInput()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("user-pay", "pay@test.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            // Assuming the car price per hour is set in testCar.Price.
            var startDate = DateTime.Now.AddHours(2);
            var endDate = DateTime.Now.AddHours(5);
            var expectedPrice = Math.Round(testCar.Price * 3, 2);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = await service.GetPaymentModelAsync(testCar.Id, startDate, endDate, expectedPrice, testUser, tempData);
            Assert.Equal(expectedPrice, model.Price);
            Assert.Equal(testUser.Name, model.FullName);
        }

        [Fact]
        public async Task GetPaymentModelAsync_ThrowsException_WhenPriceValidationFails()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("user-pay2", "pay2@test.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var startDate = DateTime.Now.AddHours(2);
            var endDate = DateTime.Now.AddHours(4);
            var wrongPrice = 999.99m; // intentionally wrong

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            await Assert.ThrowsAsync<Exception>(() => service.GetPaymentModelAsync(testCar.Id, startDate, endDate, wrongPrice, testUser, tempData));
        }

        [Fact]
        public async Task SubmitPaymentAsync_ReturnsErrors_ForInvalidCreditCard()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("user-submit", "submit@test.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var startDate = DateTime.Now.AddHours(2);
            var endDate = DateTime.Now.AddHours(4);
            var expectedPrice = Math.Round(testCar.Price * 2, 2);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "OnGet" &&
                ctx.Controller == "StartBooking" &&
                ctx.Protocol == "http")))
                .Returns("http://callbackurl");

            // Provide an invalid credit card number.
            var model = await service.SubmitPaymentAsync(
                testCar.Id,
                expectedPrice,
                startDate,
                endDate,
                "", // missing full name
                "1234", // invalid CC number
                "12",  // invalid CVV
                DateTime.Now.AddMonths(1), // valid expiry
                testUser,
                urlHelperMock.Object,
                "http",
                tempData
            );

            Assert.True(model.SubmissionErrors.Count > 0);
            Assert.Contains("You must enter your full name.", model.SubmissionErrors);
            Assert.Contains("The credit card number was invalid.", model.SubmissionErrors);
            Assert.Contains("The CVV you entered was invalid.", model.SubmissionErrors);
        }

        [Fact]
        public async Task SubmitPaymentAsync_ProcessesPayment_WhenValidInput()
        {
            using var context = TestHelpers.GetSqliteInMemoryContext();
            var testUser = TestHelpers.GetDummyUser("user-submit2", "submit2@test.com", context);
            var testCar = TestHelpers.GetDummyCar(context);

            var emailSenderMock = new Mock<IEmailSender>();
            var userManager = TestHelpers.GetUserManager();
            var service = new BookingsService(context, emailSenderMock.Object, userManager);

            var startDate = DateTime.Now.AddHours(2);
            var endDate = DateTime.Now.AddHours(4);
            var expectedPrice = Math.Round(testCar.Price * 2, 2);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "OnGet" &&
                ctx.Controller == "StartBooking" &&
                ctx.Protocol == "http")))
                .Returns("http://callbackurl");

            // Provide valid payment information.
            // Note: Use a valid credit card number that passes the Luhn algorithm. For test purposes, "4242424242424242" is often used.
            var model = await service.SubmitPaymentAsync(
                testCar.Id,
                expectedPrice,
                startDate,
                endDate,
                "John Doe",
                "4242424242424242",
                "123",
                DateTime.Now.AddYears(1),
                testUser,
                urlHelperMock.Object,
                "http",
                tempData
            );

            Assert.Empty(model.SubmissionErrors);
            Assert.NotNull(model.ActivationCode);
            Assert.NotNull(testUser.Email);

            emailSenderMock.Verify(es => es.SendEmailAsync(
                testUser.Email,
                "You May Now Unlock Your Car",
                It.IsAny<string>()),
                Times.Once);

        }
    }
}
