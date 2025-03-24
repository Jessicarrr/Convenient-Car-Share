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
using System.Text;
using System.Threading.Tasks;

namespace Test_Xunit_CCS
{
    public static class TestHelpers
    {
        /// <summary>
        /// Get a reference to a fake ApplicationUser while also adding them to the
        /// in memory database.
        /// </summary>
        /// <returns>An ApplicationUser instance.</returns>
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
        /// <param name="context">The db context currently being used.</param>
        /// <returns></returns>
        public static Car GetDummyCar(ApplicationDbContext context)
        {
            var testCar = new Car("test-brand",
                "test-model",
                "test-colour",
                "test-xyz-plate",
                23.0,
                23.0,
                50.0M,
                "Four"
            );

            context.Cars.Add(testCar);
            context.SaveChanges();

            return testCar;
        }

        /// <summary>
        /// Get an ApplicationDbContext, in this case to test db actions. This uses an in-memory
        /// sqlite database so the actual database on disk is not affected.
        /// </summary>
        /// <returns>An ApplicationDbContext instance.</returns>
        public static ApplicationDbContext GetSqliteInMemoryContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
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
        /// <returns>Returns (ClaimsIdentity, ClaimsPrinciple)</returns>
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
        /// <returns></returns>
        public static UserManager<ApplicationUser> GetUserManager()
        {
            // Create a mock for the IUserStore
            var store = new Mock<IUserStore<ApplicationUser>>().Object;

            // Use Options.Create to provide an IdentityOptions instance
            var identityOptions = Options.Create(new IdentityOptions());

            // Use a PasswordHasher
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            // Provide empty lists for validators
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();

            // Use a real lookup normalizer
            var keyNormalizer = new UpperInvariantLookupNormalizer();

            // Provide an instance of IdentityErrorDescriber
            var identityErrorDescriber = new IdentityErrorDescriber();

            // Build a minimal IServiceProvider. This is optional and can be replaced with a mock if needed.
            var services = new ServiceCollection().BuildServiceProvider();

            // Create a mock for ILogger<UserManager<ApplicationUser>>
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>().Object;

            // After all that, we can now create a userManager.
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
