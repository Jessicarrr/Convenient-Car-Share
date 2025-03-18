using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ConvenientCarShare.Models;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Data;

namespace ConvenientCarShare.Controllers
{
    [Authorize(Roles = Constants.AdministratorRole)]
    public class ManageUsersController : Controller
    {

        private readonly UserManager<ApplicationUser>
            _userManager;

        private readonly ApplicationDbContext _context;

        public ManageUsersController(
            UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _userManager.GetUsersInRoleAsync(Constants.CustomerRole);
            return View("/Views/Manage/index.cshtml",customers.ToArray());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userID)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userID);
            IdentityResult result;

            result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleteing user with ID '{user.Id}'.");
            }


            return RedirectToAction("Index");

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> BlockUser(string userID)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userID);
            switch (user.LockoutEnd)
            {

                case (null):
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(10));
                    return Json("locked!");

                default:
                    user.Warnned = false;


                   
                     var result = await _userManager.UpdateAsync(user);
                   
                    //_context.Update(user);
                    //await _context.SaveChangesAsync();

                    
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    await _userManager.SetLockoutEnabledAsync(user, false);

                    return Json("Unlocked!");
            }

        }
    }
}