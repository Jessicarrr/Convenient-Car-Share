using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Test_Xunit_CCS
{
    public static class TestHelpers
    {
        /// <summary>
        /// Get a reference to a fake ApplicationUser while also adding them to the in-memory database.
        /// </summary>
        public static ApplicationUser GetDummyUser(string id, string email, ApplicationDbContext context)
        {
            var testUser = new ApplicationUser { Id = id, Email = email };
            context.Users.Add(testUser);
            context.SaveChanges();
            return testUser;
        }

        /// <summary>
        /// Create a fake car in the ApplicationDbContext and save it to the database.
        /// </summary>
        public static Car GetDummyCar(ApplicationDbContext context)
        {
            var testCar = new Car("test-brand",
                "test-model",
                "test-colour",
                "test-xyz-plate",
                23.0,
                23.0,
                50.0M,
                "Four");
            context.Cars.Add(testCar);
            context.SaveChanges();
            return testCar;
        }

        /// <summary>
        /// Create a fake parking area and add it to the context.
        /// </summary>
        public static ParkingArea GetDummyParkingArea(ApplicationDbContext context, int width = 24, int height = 12, int capacity = 5)
        {
            var parkingArea = new ParkingArea(width, height, capacity);
            context.Add(parkingArea);
            context.SaveChanges();
            return parkingArea;
        }

        /// <summary>
        /// Create a dummy booking in the ApplicationDbContext and save it.
        /// </summary>
        public static Booking GetDummyBooking(ApplicationDbContext context, ApplicationUser user, Car car, DateTime startDate, DateTime endDate, string status)
        {
            var booking = new Booking
            {
                User = user,
                Car = car,
                StartDate = startDate,
                EndDate = endDate,
                Status = status
            };
            context.Bookings.Add(booking);
            context.SaveChanges();
            return booking;
        }

        /// <summary>
        /// Get an ApplicationDbContext for testing using an in-memory SQLite database.
        /// </summary>
        public static ApplicationDbContext GetSqliteInMemoryContext()
        {
            var connectionString = $"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared";
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Create a fake identity to test tasks that require a login or a user to exist.
        /// </summary>
        public static (ClaimsIdentity, ClaimsPrincipal) GetClaimsIdentity()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "testusername")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            return (identity, principal);
        }

        /// <summary>
        /// Create a user manager for various services in the project using Mock.
        /// </summary>
        public static UserManager<ApplicationUser> GetUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>().Object;
            var identityOptions = Options.Create(new IdentityOptions());
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = new UpperInvariantLookupNormalizer();
            var identityErrorDescriber = new IdentityErrorDescriber();
            var services = new ServiceCollection().BuildServiceProvider();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>().Object;

            var userManager = new UserManager<ApplicationUser>(
                store,
                identityOptions,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                identityErrorDescriber,
                services,
                logger);

            return userManager;
        }
    }
}
