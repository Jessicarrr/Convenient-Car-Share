using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Services
{
    public class BookingsService : IBookingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsService(ApplicationDbContext context, IEmailSender emailSender, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<ManageTrips> CancelBookingAsync(int cancelBookingId, ApplicationUser currentUser)
        {
            var booking = _context.Bookings.SingleOrDefault(b => b.Id == cancelBookingId);
            var allBookings = await _context.Bookings
                    .Where(b => b.User.Id == currentUser.Id)
                    .Include(b => b.Car)
                    .ToArrayAsync();

            var model = new ManageTrips { bookings = allBookings };

            if (booking == null || booking.User != currentUser)
            {
                model.errors.Add("An unknown error occurred when trying to cancel the booking. Sorry about that.");
            }
            else if (booking.Status != Constants.statusBooked)
            {
                model.errors.Add("You can only cancel a booking with the status " + Constants.statusBooked);
            }

            if (model.errors.Count > 0)
            {
                return model;
            }

            booking.Status = Constants.statusCancelled;
            await _context.SaveChangesAsync();
            model.messages.Add("Successfully cancelled booking. You have been refunded the full amount. ($" + booking.Price + ")");
            return model;
        }

        public async Task<ManageTrips> ResendActivationCodeAsync(int bookingId, ApplicationUser currentUser, IUrlHelper urlHelper, string scheme)
        {
            var booking = _context.Bookings.SingleOrDefault(b => b.Id == bookingId);
            var allBookings = await _context.Bookings
                    .Where(b => b.User == currentUser)
                    .Include(b => b.Car)
                    .ToArrayAsync();

            var model = new ManageTrips { bookings = allBookings };

            if (booking == null || booking.User.Id != currentUser.Id)
            {
                model.errors.Add("Something went wrong with re-sending the email. The booking did not exist.");
                return model;
            }
            else if (booking.Status != Constants.statusBooked)
            {
                model.errors.Add("That booking cannot have an activation code applied anymore because it has a status of " + booking.Status);
                return model;
            }

            var callbackUrl = urlHelper.Action("OnGet", "StartBooking", new { booking.ActivationCode }, scheme);
            await _emailSender.SendEmailAsync(
                booking.User.Email,
                "Activation Code",
                $"Your activation code is {booking.ActivationCode}. Or click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>here</a> to start your booking and unlock the car.");

            model.messages.Add("A new activation code email has been sent.");
            return model;
        }

        public async Task<ManageTrips> GetManageTripsModelAsync(ApplicationUser currentUser)
        {
            var bookingsHistory = await _context.Bookings
                .Where(booking => booking.User == currentUser)
                .Include(booking => booking.Car)
                .ToArrayAsync();

            return new ManageTrips { bookings = bookingsHistory };
        }

        public async Task<BookModel> GetBookModelAsync(int carId, string startTime, string endTime, ITempDataDictionary tempData)
        {
            var carArray = await _context.Cars.Where(x => x.Id == carId).ToArrayAsync();
            if (carArray.Length != 1)
            {
                // Handle error appropriately (e.g., throw an exception or return a model with an error message)
                throw new Exception("Car not found or multiple cars found.");
            }

            var model = new BookModel { Booking = new Booking(), Car = carArray.First() };

            if (tempData.ContainsKey("ErrorMessage"))
            {
                model.Errors.Add(tempData["ErrorMessage"] as string);
            }

            // If you need startTime/endTime in the model, add properties to BookModel.
            return model;
        }

        public async Task<PaymentModel> GetPaymentModelAsync(int carId, DateTime startDate, DateTime endDate, decimal price, ApplicationUser currentUser, ITempDataDictionary tempData)
        {
            bool isPeriodValid = IsBookingPeriodValid(startDate, endDate);

            if (!CarExists(carId))
            {
                throw new Exception("The selected car does not exist.");
            }

            var actualPrice = GetPriceFor(carId, startDate, endDate);
            if (price != actualPrice || actualPrice == -1.00m)
            {
                throw new Exception("Price validation failed.");
            }

            if (!isPeriodValid)
            {
                tempData["ErrorMessage"] = "The booking period entered was invalid. Either the end date was earlier than the start date, or one of the dates entered is earlier than the current date/time.";
                // Depending on your design, you could throw an exception here.
            }

            return new PaymentModel
            {
                CarId = carId,
                StartDate = startDate,
                EndDate = endDate,
                Price = actualPrice,
                FullName = currentUser.Name
            };
        }

        public async Task<PaymentModel> SubmitPaymentAsync(
            int carId, decimal price, DateTime startDate, DateTime endDate,
            string fullName, string creditCardNumber, string cvv, DateTime expiryDate,
            ApplicationUser currentUser, IUrlHelper urlHelper, string scheme, ITempDataDictionary tempData)
        {
            bool isCcValid = IsCreditCardNumberValid(creditCardNumber);
            bool isCcExpired = IsCreditCardExpired(expiryDate);
            bool isCvvValid = IsCvvValid(cvv);
            bool isFullNameValid = IsFullNameValid(fullName);
            bool isPeriodValid = IsBookingPeriodValid(startDate, endDate);
            decimal actualPrice = GetPriceFor(carId, startDate, endDate);

            var model = new PaymentModel
            {
                CarId = carId,
                StartDate = startDate,
                EndDate = endDate,
                Price = actualPrice,
                FullName = fullName,
                CreditCardNumber = creditCardNumber,
                CVV = cvv
            };

            if (!isPeriodValid)
            {
                tempData["ErrorMessage"] = "The booking period entered was invalid.";
                return model;
            }

            if (!CarExists(carId) || price != actualPrice || actualPrice == -1.00m)
            {
                model.SubmissionErrors.Add("An unknown error occurred when trying to make a booking. Please try again.");
                return model;
            }

            if (!isCcValid || isCcExpired || !isCvvValid || !isFullNameValid)
            {
                if (!isFullNameValid)
                    model.SubmissionErrors.Add("You must enter your full name.");
                if (!isCcValid)
                    model.SubmissionErrors.Add("The credit card number was invalid.");
                else if (isCcExpired)
                    model.SubmissionErrors.Add("The expiry date you entered has already expired.");
                if (!isCvvValid)
                    model.SubmissionErrors.Add("The CVV you entered was invalid.");
                return model;
            }

            // Update user if needed.
            await _userManager.UpdateAsync(currentUser);

            Booking booking = await CreateBookingAsync(carId, startDate, endDate, actualPrice, currentUser);
            model.ActivationCode = booking.ActivationCode;
            tempData["msg"] = "<script>$('#modal').modal();</script>";

            var callbackUrl = urlHelper.Action("OnGet", "StartBooking", new { booking.ActivationCode }, scheme);
            await _emailSender.SendEmailAsync(
                booking.User.Email,
                "Activation Code",
                $"Your activation code is {booking.ActivationCode}. Or click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>here</a> to start your booking and unlock the car.");

            return model;
        }

        // -- Private helper methods --

        private async Task<Booking> CreateBookingAsync(int carId, DateTime startDate, DateTime endDate, decimal price, ApplicationUser currentUser)
        {
            var code = GetRandomKey(8);
            var car = _context.Cars.First(c => c.Id == carId);

            var booking = new Booking
            {
                Car = car,
                User = currentUser,
                Price = price,
                StartDate = startDate,
                EndDate = endDate,
                ActivationCode = code,
                Status = Constants.statusBooked
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        private bool IsFullNameValid(string fullName) => !string.IsNullOrEmpty(fullName);

        private bool IsBookingPeriodValid(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate) return false;
            if (startDate <= DateTime.Now.AddMinutes(-5) || endDate <= DateTime.Now) return false;
            return true;
        }

        private bool IsCreditCardNumberValid(string creditCardNumber)
        {
            var whitespaceRegex = @"\s+";
            var digitsRegex = @"\d+";
            int checkSum = 0;

            if (string.IsNullOrEmpty(creditCardNumber))
                return false;

            creditCardNumber = Regex.Replace(creditCardNumber, whitespaceRegex, "");
            if (!Regex.IsMatch(creditCardNumber, digitsRegex))
                return false;

            for (var i = creditCardNumber.Length - 1; i >= 0; i -= 2)
            {
                checkSum += (creditCardNumber[i] - '0');
            }

            for (var i = creditCardNumber.Length - 2; i >= 0; i -= 2)
            {
                int value = (creditCardNumber[i] - '0') * 2;
                while (value > 0)
                {
                    checkSum += (value % 10);
                    value /= 10;
                }
            }

            return (checkSum % 10) == 0;
        }

        private bool IsCreditCardExpired(DateTime expiryDate) => DateTime.Now > expiryDate;

        private bool IsCvvValid(string cvv)
        {
            var whitespaceRegex = @"\s+";
            var digitsRegex = @"^\d+$";
            if (string.IsNullOrEmpty(cvv))
                return false;
            cvv = Regex.Replace(cvv, whitespaceRegex, "");
            return Regex.IsMatch(cvv, digitsRegex) && (cvv.Length == 3 || cvv.Length == 4);
        }

        private decimal GetPriceFor(int carId, DateTime startDate, DateTime endDate)
        {
            var car = _context.Cars.SingleOrDefault(c => c.Id == carId);
            if (car == null)
                return -1.00m;

            decimal pricePerHour = car.Price;
            TimeSpan duration = endDate - startDate;
            decimal totalPrice = Math.Round(pricePerHour * (decimal)duration.TotalHours, 2, MidpointRounding.ToEven);
            return totalPrice;
        }

        private bool CarExists(int carId) => _context.Cars.Any(car => car.Id == carId);

        private string GetRandomKey(int keyLen)
        {
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIKJLMNOPQRSTUVWXYZ";
            var builder = new StringBuilder(keyLen);
            int seed = GetRandomSeed();
            Random random = new Random(seed);
            for (int i = 0; i < keyLen; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }
            return builder.ToString();
        }

        private static int GetRandomSeed()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
