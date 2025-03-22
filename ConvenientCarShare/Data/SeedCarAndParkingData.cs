using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Data
{
    public class SeedCarAndParkingData
    {
        public static Task InitializeAsync(
            IServiceProvider services)
        {
            ParkingArea[] spots =
            {
                new ParkingArea(-37.8136, 144.9631, 5),
                new ParkingArea(-37.8145, 144.9688, 5),
                new ParkingArea(-37.8200, 144.9611, 5),
                new ParkingArea(-37.8230, 144.9622, 5),
                new ParkingArea(-37.8150, 144.9700, 5),
                new ParkingArea(-37.8088, 144.9629, 5),
                new ParkingArea(-37.8088, 144.9688, 5),
                new ParkingArea(-37.8088, 144.9730, 5),
                new ParkingArea(-37.8105, 144.9532, 5),
            };

            Car[] cars =
            {
                new Car("Hyundai", "iMax", "Black", "ABC-123", -37.8136, 144.9631, 20.99m, "5 passengers", spots[0]),
                new Car("Toyota", "HiAce", "Black", "ABC-100", -37.8145, 144.9688, 20.99m, "5 passengers",spots[0]),
                new Car("Toyota", "Prius", "Black", "ABC-101", -37.8200, 144.9611, 20.99m, "5 passengers",spots[0]),
                new Car("Jeep", "Compass North", "Black", "ABC-102", -37.8230, 144.9622, 20.99m, "5 passengers",spots[1]),
                new Car("Jeep", "Compass Limited", "Black", "ABC-103", -37.8150, 144.9700, 20.99m, "5 passengers",spots[1]),
                new Car("Hyundai", "2016 i30", "Black", "ABC-104", -37.8088, 144.9629, 20.99m, "5 passengers",spots[2]),
                new Car("Hyundai", "2016 Tucson", "Black", "ABC-105", -37.8088, 144.9688, 20.99m, "5 passengers",spots[2]),
                new Car("Holden", "2016 Trax", "Black", "ABC-106", -37.8088, 144.9730, 20.99m, "5 passengers",spots[3]),
                new Car("Honda", "Accord", "Matte Black", "PHP-040", -37.8105, 144.9532, 20.99m, "5 passengers",spots[4])
            };

            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            EnsureParkingAreas(context, spots);
            EnsureCars(context, cars);
            return Task.CompletedTask;
        }

        private static void EnsureCars(ApplicationDbContext context, Car[] cars)
        {
            foreach(var car in cars)
            {
                bool carExists = context.Cars
                    .Where(x => x.NumberPlate == car.NumberPlate)
                    .Any();

                if (!carExists)
                {
                    context.Cars.Add(car);
                }
            }
            context.SaveChanges();
        }

        private static void EnsureParkingAreas(ApplicationDbContext context, ParkingArea[] spots)
        {
            foreach(ParkingArea area in spots)
            {
                bool spotExists = context.ParkingAreas
                    .Where(x => x.Latitude == area.Latitude)
                    .Where(x => x.Longitude == area.Longitude)
                    .Where(x => x.MaximumCars == area.MaximumCars)
                    .Any();

                if(!spotExists)
                {
                    context.ParkingAreas.Add(area);
                }
            }
            context.SaveChanges();

            /*ApplicationUser TestUser = null;

            try
            {
                TestUser = await UserManager.Users
                   .Where(x => x.UserName == email)
                   .SingleOrDefaultAsync();

                if (TestUser != null) return;

                TestUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                try
                {
                    Task<IdentityResult> taskResult = UserManager.CreateAsync(
                        TestUser, password);
                    taskResult.Wait();
                    var result = taskResult.Result;

                    if (result.Succeeded)
                    {
                        await UserManager.AddToRoleAsync(
                            TestUser, Constants.CustomerRole);
                    }
                }
                catch (Exception e)
                {
                    // this exception is called when there is a db error.
                    // can be caused by adding a new attribute to the user
                    // without editing the above new ApplicationUser
                    string msg = e.Message;
                }
            }
            catch (Exception e2)
            {
                // can be caused by .SingleOrDefaultAsync returning more than one value.
                string msg2 = e2.Message;
            }*/
        }
        
    }
}
