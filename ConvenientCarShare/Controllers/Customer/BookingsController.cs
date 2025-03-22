using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;


        public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;

        }

        [HttpPost]
        public async Task<ActionResult> Cancel(int cancelBookingId)
        {
            var booking = _context.Bookings
                .Where(b => b.Id == cancelBookingId)
                .SingleOrDefault();

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var allBookings = await _context.Bookings
                    .Where(b => b.User.Id == currentUser.Id)
                    .Include(b => b.Car)
                    .ToArrayAsync();

            var manageTripsModel = new ManageTrips()
            {
                bookings = allBookings

            };

            if (booking == null || booking.User != currentUser)
            {
                manageTripsModel.errors.Add("An unknown error occurred when trying to cancel the booking. Sorry about that.");
            }
            else if (booking.Status != Constants.statusBooked)
            {
                manageTripsModel.errors.Add("You can only cancel a booking with the status " + Constants.statusBooked);
            }

            if (manageTripsModel.errors.Count > 0)
            {
                return View("~/Views/Customer/ManageTrips.cshtml", manageTripsModel);
            }

            booking.Status = Constants.statusCancelled;
            await _context.SaveChangesAsync();
            manageTripsModel.messages.Add("Successfully cancelled booking. You have been refunded the full amount. ($" + booking.Price + ")");
            return View("~/Views/Customer/ManageTrips.cshtml", manageTripsModel);
        }

        public async Task<ActionResult> ResendActivationCode(int id)
        {
            var booking = _context.Bookings
                .Where(b => b.Id == id)
                .SingleOrDefault();

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var allBookings = await _context.Bookings
                    .Where(b => b.User == currentUser)
                    .Include(b => b.Car)
                    .ToArrayAsync();

            var manageTripsModel = new ManageTrips()
            {
                bookings = allBookings

            };

            if (booking == null || booking.User.Id != currentUser.Id)
            {
                manageTripsModel.errors.Add("Something went wrong with re-sending the email. The booking did not exist.");
                return View("~/Views/Customer/ManageTrips.cshtml", manageTripsModel);
            }
            else if(booking.Status != Constants.statusBooked)
            {
                manageTripsModel.errors.Add("That booking cannot have an activation code applied anymore because it has a status of " + booking.Status);
                return View("~/Views/Customer/ManageTrips.cshtml", manageTripsModel);
            }

            var callbackUrl = Url.Action("OnGet", "StartBooking", values: new { booking.ActivationCode }, protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                booking.User.Email,
                "Activation Code",
                $"Your activication code is {booking.ActivationCode}. Or Click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> here</a> to start your booking and unlock the car.");

            manageTripsModel.messages.Add("A new activation code email has been sent.");
            return View("~/Views/Customer/ManageTrips.cshtml", manageTripsModel);
        }

        public async Task<ActionResult> ManageTrips()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var bookingsHistory = await _context.Bookings
                .Where(booking => booking.User == currentUser)
                .Include(booking => booking.Car)
                .ToArrayAsync();

            var model = new ManageTrips()
            {
                bookings = bookingsHistory
            };

            return View("/Views/Customer/ManageTrips.cshtml", model);
        }

        public async Task<ActionResult> Book(int Id, string startTime, string endTime)
        {
            var user = await _userManager.GetUserAsync(User);

            if(!user.EmailConfirmed)
            {
                return RedirectToAction("Index", "Customer");
            }

            var car = _context.Cars
                .Where(x => x.Id == Id)
                .ToArray();

            if (car.Length <= 0)
            {
                // no car found
                return NotFound("The car selected does not exist."); // 404 not found
            }
            if (car.Length > 1)
            {
                // more than one car? how??
                return StatusCode(500); // internal server error
            }

            var model = new BookModel()
            {
                Booking = new Booking(),
                Car = car.First()
            };

            if (TempData.ContainsKey("ErrorMessage"))
            {
                string error = (string)TempData["ErrorMessage"];
                model.Errors.Add(error);
            }

            ViewData["start"] = startTime;
            ViewData["end"] = endTime;

            return View("/Views/Customer/Book.cshtml", model);
        }

        [HttpPost]
        public async Task<ActionResult> Payment(DateTime StartDate, DateTime EndDate, decimal Price, int CarId)
        {
            bool BookingPeriodValid = IsBookingPeriodValid(StartDate, EndDate);

            if (!CarExists(CarId))
            {
                // the car id does not exist. something went wrong!
                // TODO: Return a view.
            }

            var ActualPrice = GetPriceFor(CarId, StartDate, EndDate);

            if (Price != ActualPrice || ActualPrice == -1.00m)
            {
                // the user was being cheeky and edited the Price or the CarId in the html. not good!
                // TODO: Return a view.
            }

            if (!BookingPeriodValid)
            {
                string error = "The booking period entered was invalid. Either the end date was earlier than the start date, "
                    + " or one of the dates entered is earlier than the current date/time.";

                TempData["ErrorMessage"] = error;
                return RedirectToAction("Book", "Bookings", new { Id = CarId });
            }

            var user = await _userManager.GetUserAsync(User);

            if (!user.EmailConfirmed)
            {
                return RedirectToAction("Index", "Customer");
            }

            var ViewModel = new PaymentModel()
            {
                CarId = CarId,
                StartDate = StartDate,
                EndDate = EndDate,
                Price = ActualPrice,
                FullName = user.Name
                
            };
            return View("/Views/Customer/Payment.cshtml", ViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitPayment(int CarId, decimal Price, DateTime StartDate, DateTime EndDate,
            string FullName, string CreditCardNumber, string cvv, DateTime ExpiryDate)
        {
            bool CreditCardNumberValid = IsCreditCardNumberValid(CreditCardNumber);
            bool CreditCardExpired = IsCreditCardExpired(ExpiryDate);
            bool CvvValid = IsCvvValid(cvv);
            bool FullNameValid = IsFullNameValid(FullName);
            bool BookingPeriodValid = IsBookingPeriodValid(StartDate, EndDate);
            decimal ActualPrice = GetPriceFor(CarId, StartDate, EndDate);

            var ViewModel = new PaymentModel()
            {
                CarId = CarId,
                StartDate = StartDate,
                EndDate = EndDate,
                Price = ActualPrice,
                FullName = FullName,
                CreditCardNumber = CreditCardNumber,
                CVV = cvv
            };

            if(!BookingPeriodValid)
            {
                string error = "The booking period entered was invalid. Either the end date was earlier than the start date, "
                    + " or one of the dates entered is earlier than the current date/time.";

                TempData["ErrorMessage"] = error;
                return RedirectToAction("Book", "Bookings", new { Id = CarId });
            }

            if (!CarExists(CarId) || Price != ActualPrice || ActualPrice == -1.00m)
            {
                // the car id does not exist. something went wrong!
                ViewModel.SubmissionErrors.Add("An unknown error occurred when trying to make a booking. Please try again.");
                return View("/Views/Customer/Payment.cshtml", ViewModel);
            }

            if (!CreditCardNumberValid || CreditCardExpired || !CvvValid || !FullNameValid)
            {
                if(!FullNameValid)
                {
                    ViewModel.SubmissionErrors.Add("You must enter your full name.");
                }
                if(!CreditCardNumberValid)
                {
                    ViewModel.SubmissionErrors.Add("The credit card number was invalid.");
                }

                else if(CreditCardExpired)
                {
                    ViewModel.SubmissionErrors.Add("The expiry date you entered has already expired.");
                }

                if(!CvvValid)
                {
                    ViewModel.SubmissionErrors.Add("The CVV you entered was invalid.");
                }
                return View("/Views/Customer/Payment.cshtml", ViewModel);
            }

            var user = await _userManager.GetUserAsync(User);

            if(!user.EmailConfirmed)
            {
                return RedirectToAction("Index", "Customer");
            }

            await _userManager.UpdateAsync(user);

            Booking booking = await CreateBookingAsync(CarId, StartDate, EndDate, ActualPrice);
            ViewModel.ActivationCode = booking.ActivationCode;
            TempData["msg"] = "<script>$('#modal').modal();</script>";

            var callbackUrl = Url.Action("OnGet","StartBooking",values:new { booking.ActivationCode },protocol:Request.Scheme);

            await _emailSender.SendEmailAsync(
                booking.User.Email,
                "Activication Code",
                $"Your activication code is {booking.ActivationCode}. Or Click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> here</a> to start your booking and unlock the car.");

            return View("/Views/Customer/Payment.cshtml", ViewModel);
        }

        private async Task<Booking> CreateBookingAsync(int CarId, DateTime StartDate, DateTime EndDate, decimal Price)
        {

            var code = GetRandomKey(8);

            Car car = _context.Cars.Where(c => c.Id == CarId).First();
            ApplicationUser CurrentUser = await _userManager.GetUserAsync(User);

            Booking booking = new Booking()
            {
                Car = car,
                User = CurrentUser,
                Price = Price,
                StartDate = StartDate,
                EndDate = EndDate,
                ActivationCode = code,
                Status = Constants.statusBooked
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return booking;
        }

        private bool IsFullNameValid(string FullName)
        {
            if(FullName == null)
            {
                return false;
            }
            if(FullName.Length > 0)
            {
                return true;
            }
            return false;
        }

        private bool IsBookingPeriodValid(DateTime StartDate, DateTime EndDate)
        {
            if(EndDate <= StartDate)
            {
                return false;
            }

            if(StartDate <= DateTime.Now.AddMinutes(-5) || EndDate <= DateTime.Now)
            {
                return false;
            }
            return true;
        }

        private bool IsCreditCardNumberValid(string CreditCardNumber)
        {
            /*
             * The best way to verify credit card numbers is to send them to
             * the payment provider and check if the payment is successful.
             * 
             * If the payment was not successful, this method can be run to give
             * some potential context as to why.
             * 
             * If no reason could be found, we could return an unknown error.
             */

            var WhitespaceRegex = @"\s+";
            var CreditCardDigitsOnlyValidation = @"\d+";
            var CheckSum = 0;

            if(String.IsNullOrEmpty(CreditCardNumber))
            {
                return false;
            }

            // remove all whitespace, if there is any
            CreditCardNumber = Regex.Replace(CreditCardNumber, WhitespaceRegex, "");

            if(!Regex.IsMatch(CreditCardNumber, CreditCardDigitsOnlyValidation))
            {
                return false;
            }

            for(var i = CreditCardNumber.Length - 1; i >= 0; i -=2)
            {
                CheckSum += (CreditCardNumber[i] - '0');
            }

            for(var i = CreditCardNumber.Length - 2; i >= 0; i -= 2)
            {
                var value = ((CreditCardNumber[i] - '0') * 2);

                while(value > 0)
                {
                    CheckSum += (value % 10);
                    value /= 10;
                }
            }

            bool IsDivisibleByTen = ((CheckSum % 10) == 0);

            // If the credit card number is divisible by 10, it is valid.
            return IsDivisibleByTen;
        }

        private bool IsCreditCardExpired(DateTime ExpiryDate)
        {
            // Assuming ExpiryDate is the last valid month.
            return DateTime.Now > ExpiryDate;
        }

        private bool IsCvvValid(string cvv)
        {
            /*
             * Some cards use different cvv lengths.
             * Some are 3, some are 4.
             * So this method could use some improvement.
             * At the moment this method just checks if the string is all digits and 
             * if it has at least 3 digits.
             */

            var WhitespaceRegex = @"\s+";
            var DigitsOnlyValidation = @"^\d+$";

            if(String.IsNullOrEmpty(cvv))
            {
                return false;
            }

            // remove all whitespace, if there is any
            cvv = Regex.Replace(cvv, WhitespaceRegex, "");

            if (!Regex.IsMatch(cvv, DigitsOnlyValidation))
            {
                // there are non digit characters
                return false;
            }

            if(cvv.Length == 3 || cvv.Length == 4)
            {
                return true;
            }

            return false;
        }

        private decimal GetPriceFor(int CarId, DateTime StartDate, DateTime EndDate)
        {
            var carList = _context.Cars.Where(c => c.Id == CarId);

            if(!carList.Any() || carList.Count() > 1)
            {
                return -1.00m;
            }

            decimal PricePerHour = carList.First().Price;
            TimeSpan BookingTimeTotal = EndDate.Subtract(StartDate);
            decimal TotalHours = (decimal) BookingTimeTotal.TotalHours;

            decimal TotalPrice = Math.Round(PricePerHour * TotalHours, 2, MidpointRounding.ToEven);

            return TotalPrice;
        }

        private bool CarExists(int CarId)
        {
            return _context.Cars.Where(car => car.Id == CarId).Any();
        }

        private string GetRandomKey(int keylen)
        {
            string randomChars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIKJLMNOPQRSTUVWXYZ";
            StringBuilder password = new StringBuilder(keylen);
            int seed = GetRandomSeed();
            Random random = new Random(seed);

            for (int i = 0; i < keylen; i++)
            {
                int randomNum = random.Next(randomChars.Length);
                password.Append(randomChars[randomNum]);
            }
            return password.ToString();
        }


        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            bytes = RandomNumberGenerator.GetBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}