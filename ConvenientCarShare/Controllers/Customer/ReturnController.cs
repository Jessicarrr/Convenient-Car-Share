using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Controllers.Customer
{
    [Authorize]
    public class ReturnController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReturnController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        public async Task<ActionResult> ReturnCar(int id)
        {

            var cars = await _context.Cars
                .Include(x => x.CurrentlyParkedAt)
                .ToArrayAsync();

            var spots = await _context.ParkingAreas.ToArrayAsync();



            Dictionary<ParkingArea, List<Car>> parkingAreasAndCars = new Dictionary<ParkingArea, List<Car>>();

            foreach (var car in cars)
            {
                var parkingArea = car.CurrentlyParkedAt;

                if (parkingArea == null)
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
            List<ParkingArea> parkingAreas = new List<ParkingArea>();


            foreach (var spot in spots)
            {
                List<Car> car;
                if (parkingAreasAndCars.TryGetValue(spot, out car))
                {
                    if (car.Count >= spot.MaximumCars)
                    {
                        continue;
                    }

                    parkingAreas.Add(spot);


                }
                else {

                    parkingAreas.Add(spot);

                }
            }



            var ViewModel = new ReturnCarModel()
            {
                BookingId = id,
                parkingAreas = parkingAreas
            };




            return View("/Views/Customer/ReturnCar.cshtml", ViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReturnCarOnPost(int bookingId, int spotId)
        {

            var booking = await _context.Bookings.Where(y => y.Id == bookingId)
                .Include(x => x.Car)
                .Include(x => x.ReturnArea)
                .FirstOrDefaultAsync();

            var currCar = await _context.Cars
               .Where(car => car.Id == booking.Car.Id).Include(car => car.CurrentlyParkedAt).FirstOrDefaultAsync();

            var spot = await _context.ParkingAreas
                .Where(x => x.Id == spotId).FirstOrDefaultAsync();

            booking.ReturnDate = DateTime.Now;
            booking.ReturnArea = spot;
            booking.status = Constants.statusFinished;

            booking.Car.CurrentlyParkedAt = spot;
            booking.Car.Latitude = spot.Latitude;
            booking.Car.Longitude = spot.Longitude;

            TempData["msg"] = "<div class='alert alert-success alert - dismissable'>"+
         "<button type = 'button' class='close' data-dismiss='alert'"+
                    "aria-hidden='true'>"+
                "&times;"+
            "</button>"+
                "Successfully returned the car! Thank you!"+
        "</div>";

            _context.Update(booking);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Customer");

        }

    }
}