using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.DataTransferObjects;
using ConvenientCarShare.Services.Registration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Xunit_CCS
{
    public class UserRegistrationTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ILogger<RegistrationService> _logger;
        private readonly IRegistrationService _registrationService;

        public UserRegistrationTests()
        {
            // Create a mock for IUserStore<ApplicationUser> as required by UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Create a mock for IOptions<IdentityOptions>
            var identityOptions = Options.Create(new IdentityOptions());

            // Create the UserManager mock with the required dependencies.
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                identityOptions,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            // Create a logger for the RegistrationService (using LoggerFactory for simplicity).
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<RegistrationService>();

            // Create the instance of RegistrationService using the mocked UserManager.
            _registrationService = new RegistrationService(_userManagerMock.Object, _logger);
        }

        [Fact]
        public async Task RegisterUserAsync_Succeeds_WhenValidInput()
        {
            // Arrange: set up a valid registration DTO.
            var registrationDto = new RegistrationDto
            {
                Name = "John Doe",
                DOB = DateTime.Now.AddYears(-25), // user is 25 years old
                Licence = "12345678",
                Email = "john@example.com",
                Password = "Password123!"
            };

            // Simulate successful creation of the user.
            _userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registrationDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Simulate successful addition of the customer role.
            _userManagerMock
                .Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), Constants.CustomerRole))
                .ReturnsAsync(IdentityResult.Success);

            // Simulate generating an email confirmation token.
            _userManagerMock
                .Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("dummy-token");

            // Act: call the registration service.
            var result = await _registrationService.RegisterUserAsync(registrationDto);

            // Assert: ensure that the registration was successful.
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal("john@example.com", result.User.Email);
            Assert.Equal("dummy-token", result.EmailConfirmationToken);
        }

        [Fact]
        public async Task RegisterUserAsync_Fails_WhenCreateAsyncFails()
        {
            // Arrange: set up a valid registration DTO.
            var registrationDto = new RegistrationDto
            {
                Name = "Jane Doe",
                DOB = DateTime.Now.AddYears(-30), // user is 30 years old
                Licence = "87654321",
                Email = "jane@example.com",
                Password = "Password123!"
            };

            // Simulate failure when creating the user (e.g., email already taken).
            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "DuplicateEmail", Description = "Email already taken." }
            };

            _userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registrationDto.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act: call the registration service.
            var result = await _registrationService.RegisterUserAsync(registrationDto);

            // Assert: ensure that the registration failed with the expected error.
            Assert.False(result.Success);
            Assert.Contains("Email already taken.", result.Errors);
            Assert.Null(result.User);
            Assert.Null(result.EmailConfirmationToken);
        }
    }
}
