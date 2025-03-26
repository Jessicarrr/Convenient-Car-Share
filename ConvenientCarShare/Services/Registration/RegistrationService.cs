using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;
using ConvenientCarShare.DataTransferObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Services.Registration
{
    public class RegistrationService : IRegistrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(UserManager<ApplicationUser> userManager,
                                   ILogger<RegistrationService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<RegistrationResult> RegisterUserAsync(RegistrationDto dto)
        {
            bool licenceProvided = !string.IsNullOrWhiteSpace(dto.Licence);

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                DOB = dto.DOB,
                IsLicenceProvided = licenceProvided,
            };

            try
            {
                // Create the user with Identity
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    return new RegistrationResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description)
                    };
                }

                var roleResult = await _userManager.AddToRoleAsync(user, Constants.CustomerRole);
                if (!roleResult.Succeeded)
                {
                    return new RegistrationResult
                    {
                        Success = false,
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
                }

                _logger.LogInformation("User created a new account with a password.");

                // Generate an email confirmation token.
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return new RegistrationResult
                {
                    Success = true,
                    User = user,
                    EmailConfirmationToken = token
                };
            }
            catch (DbUpdateException ex)
            {
                // Check for a duplicate licence number (if that's how your DB reports it)
                if (ex.InnerException is SqliteException se && se.SqliteErrorCode == 19)
                {
                    return new RegistrationResult
                    {
                        Success = false,
                        Errors = ["Licence number is already taken."]
                    };
                }
                throw;
            }
        }
    }

}
