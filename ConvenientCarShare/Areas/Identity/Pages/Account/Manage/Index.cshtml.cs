using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Full name")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            public DateTime DOB { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [StringLength(10, MinimumLength = 8)]
            [Display(Name = "Licence")]
            public string Licence { get; set; }

            [CreditCard]
            [Display(Name = "Credit Card")]
            public string CreditCardNo { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
            [Display(Name = "Expiry Date")]
            public DateTime ExpiryDate { get; set; }

            [Display(Name = "CVV")]
            [DataType(DataType.Text)]
            [StringLength(4, MinimumLength = 3)]
            public string CVV { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber,
                Name = user.Name,
                DOB = user.DOB,
                Licence = user.Licence,
                CreditCardNo = user.CreditCardNo,
                ExpiryDate = user.ExpiryDate,
                CVV = user.CVV

            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.ExpiryDate.AddMonths(1).CompareTo(DateTime.Now) <= 0)
            {
                ModelState.AddModelError(string.Empty ,"The card is already expired!");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (Input.Name != user.Name)
            {
                user.Name = Input.Name;
            }

            if (Input.DOB != user.DOB)
            {
                user.DOB = Input.DOB;
            }

            if (Input.Licence != user.Licence)
            {
                user.Licence = Input.Licence;
            }

            if (Input.DOB != user.DOB)
            {
                user.DOB = Input.DOB;
            }

            if (Input.CreditCardNo != user.CreditCardNo)
            {
                user.CreditCardNo = Input.CreditCardNo;
            }

            if (Input.ExpiryDate != user.ExpiryDate)
            {
                user.ExpiryDate = Input.ExpiryDate;
            }

            if (Input.CVV != user.CVV)
            {
                user.CVV = Input.CVV;
            }






            var email = await _userManager.GetEmailAsync(user);

            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }
            try
            {
                await _userManager.UpdateAsync(user);
          

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            }

            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqliteException se)
                    if (se.SqliteErrorCode == 19)
                    {
                        ModelState.AddModelError(string.Empty, "License number is already taken.");
                        return Page();
                    }

            }
            return RedirectToPage();

        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
