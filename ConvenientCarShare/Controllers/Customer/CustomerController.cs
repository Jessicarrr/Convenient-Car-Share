using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConvenientCarShare.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.Now;

            Dictionary<ParkingArea, List<Car>> parkingAreasAndCars = await GetParkingAreasAndCarsAsync();
            var model = new IndexModel()
            {
                ParkingInfo = parkingAreasAndCars
            };

            
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if(currentUser != null)
            {
                var noReturnBookings = await _context.Bookings
                .Where(booking => booking.User == currentUser)
                .Where(booking => booking.status == Constants.statusDriving || (booking.StartDate.Ticks <= now.Ticks && booking.status == Constants.statusBooked))
                .ToArrayAsync();

                if (noReturnBookings.Any())
                {
                    model.HasBookingsWithoutReturns = true;
                }
            }

            

            return View(model);
        }

        /*public IActionResult Test()
        {
            var cars = _context.Cars.ToArray();
            var user = _context.Users.First();

            foreach(var car in cars)
            {
                Booking booking = new Booking()
                {
                    StartDate = DateTime.Now.AddHours(0),
                    EndDate = DateTime.Now.AddHours(5),
                    Car = car,
                    User = user
                };
                _context.Bookings.Add(booking);
            }
            _context.SaveChanges();

            var jsonResult = GetCarsNotBookedDuring(DateTime.Now.AddHours(0), DateTime.Now.AddHours(5));

            
            return Json(jsonResult.Result);
        }*/

        /*public Dictionary<ParkingArea, List<Car>> GetCarsAvailableDuring(DateTime StartDate, DateTime EndDate)
        {
            Dictionary<ParkingArea, List<Car>> parkingAreasAndCars = GetParkingAreasAndCars();
            Dictionary<ParkingArea, List<Car>> currentlyAvailableCars = new Dictionary<ParkingArea, List<Car>>();
            var bookings = _context.Bookings.ToArray();

            foreach (KeyValuePair<ParkingArea, List<Car>> entry in parkingAreasAndCars)
            {
                ParkingArea parkingArea = entry.Key;
                List<Car> carList = entry.Value;

                foreach (var car in carList)
                {
                    bool carAvailable = IsCarAvailableWithin(StartDate, EndDate, bookings, car);

                    if (carAvailable)
                    {
                        if (!currentlyAvailableCars.ContainsKey(parkingArea))
                        {
                            var newCarList = new List<Car>();

                            newCarList.Add(car);
                            currentlyAvailableCars[parkingArea] = newCarList;
                        }
                        else
                        {
                            currentlyAvailableCars[parkingArea].Add(car);
                        }
                    }
                }
            }
            return currentlyAvailableCars;
        }*/

        /*public bool IsCarAvailableWithin(DateTime StartDate, DateTime EndDate, Booking[] bookings, Car car)
        {
            var carBookings = bookings
                .Where(booking => booking.Car == car).ToList() ;

            var thing2 =
                carBookings.Where(booking => booking.StartDate >= StartDate).ToList();
            var thing3 =
                thing2.Where(booking => booking.EndDate <= EndDate).ToList();
            var thing4 = 
                thing3.OrderBy(booking => booking.StartDate)
                .ToList();

            var thing = "thing";

            foreach (var booking in carBookings)
            {
                var thingo = booking.StartDate;

                var thing767 = booking.EndDate;
                var hello = "hello";
            }

            return false;
        }*/

        /// <summary>
        /// <para>Gets all cars that have no booking between the start date minus two hours, and the end date.</para>
        /// <para>If the car has one or more bookings between the start date and the end date, it is filtered out.</para>
        /// <para>Two hours are subtracted from the StartDate because cars should not be booked within 1 hour of another
        /// booking's end date.</para>
        /// </summary>
        public async Task<JsonResult> GetCarsNotBookedDuring(DateTime startDate, DateTime endDate)
        {
            var bookings = await _context.Bookings
                .Where(b => b.status != Constants.statusCancelled)
                .ToArrayAsync();
            // subtract a certain amount of hours from the start date, so the date range is bigger.
            // we do this because we don't want people to book cars immediately after another booking.
            // there should be an hour between the last booking and the next booking.
            DateTime StartDate = startDate.AddHours(Constants.MinimumTimeBetweenBookings * -1);
            DateTime EndDate = endDate.AddHours(Constants.MinimumTimeBetweenBookings);

            var cars = await _context.Cars
                            .Include(x => x.CurrentlyParkedAt)
                            .ToArrayAsync();

            List<Car> carList = new List<Car>();

            foreach (Car car in cars)
            {

                bool carBooked = IsCarBookedWithin(StartDate, EndDate, bookings, car);

                if (!carBooked)
                {
                    carList.Add(car);
                }

            }

            var jsonResult = JsonConvert.SerializeObject(carList);
            return Json(jsonResult);

        }

        /// <summary>
        /// Return true if the car has any booking between the start date and the end date.
        /// </summary>
        public bool IsCarBookedWithin(DateTime StartDate, DateTime EndDate, Booking[] Bookings, Car Car)
        {
            var CarBookings = Bookings.Where(booking => booking.Car == Car).ToArray();

            foreach(var booking in CarBookings)
            {
                if (booking.StartDate.Ticks < EndDate.Ticks && booking.EndDate.Ticks > StartDate.Ticks)
                {
                    if (booking.status == Constants.statusCancelled)
                    {

                        continue;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets avaliable cars and parking areas in 2 hours.
        /// </summary>
        public async Task<Dictionary<ParkingArea, List<Car>>> GetParkingAreasAndCarsAsync()
        {
            var cars =await _context.Cars
                .Include(x => x.CurrentlyParkedAt)
                .ToArrayAsync();

            Dictionary<ParkingArea, List<Car>> parkingAreasAndCars = new Dictionary<ParkingArea, List<Car>>();

            DateTime now = DateTime.Now;

            //var followingBookings = new Booking[];


            foreach (var car in cars)
            {
                var parkingArea = car.CurrentlyParkedAt;

                if (parkingArea == null)
                {
                    continue;
                }

                var start = DateTime.Now.AddHours(-1);
                var end = DateTime.Now.AddHours(2);

                var followingBookings = await _context.Bookings
                    .Where(booking => booking.status != Constants.statusCancelled)
                    .Where(booking => start.Ticks < booking.EndDate.Ticks && end.Ticks > booking.StartDate.Ticks)
                    .Where(booking => booking.Car.Id == car.Id)
                    .ToArrayAsync();

                if (followingBookings.Any())
                {
                    continue;
                }


                /*
                 * If this dictionary does already have the parking area, add the current
                 * car to the list.
                 */
                if (parkingAreasAndCars.ContainsKey(parkingArea))
                {
                    List<Car> carList = parkingAreasAndCars[parkingArea];
                    carList.Add(car);

                }

                /*
                 * If the dictionary does not have a key for this parking area, create one
                 * and add the car to it.
                 */
                else
                {
                    var carList = new List<Car>();
                    carList.Add(car);
                    parkingAreasAndCars[parkingArea] = carList;
                }
            }
            return parkingAreasAndCars;

        }
    }
}
