using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ConvenientCarShare.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ConvenientCarShare.Attributes;
using ConvenientCarShare.DataTransferObjects;
using ConvenientCarShare.Services.Registration;

namespace ConvenientCarShare.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IRegistrationService _registrationService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IRegistrationService registrationService,
                             SignInManager<ApplicationUser> signInManager,
                             IEmailSender emailSender,
                             ILogger<RegisterModel> logger)
        {
            _registrationService = registrationService;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Full name")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            [MinimumAge(18)]
            public DateTime DOB { get; set; }

            [Display(Name = "Licence")]
            [ValidLicenceNumber]
            [DataType(DataType.Text)]
            public string Licence { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/Customer/Index");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Map the InputModel to the Data Transfer Object
            var dto = new RegistrationDto
            {
                Name = Input.Name,
                DOB = Input.DOB,
                Licence = Input.Licence,
                Email = Input.Email,
                Password = Input.Password
            };

            var result = await _registrationService.RegisterUserAsync(dto);

            if (result.Success)
            {
                // Generate the callback URL for email confirmation
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = result.User.Id, code = result.EmailConfirmationToken },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by visiting the following link:<br/><br/>{HtmlEncoder.Default.Encode(callbackUrl)}");

                await _signInManager.SignInAsync(result.User, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return Page();
            }
        }
    }

}
