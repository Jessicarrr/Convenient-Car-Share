using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConvenientCarShare.Controllers.Manage
{
    [Authorize(Roles = Constants.AdministratorRole)]
    public class ManageCarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageCarsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View("/Views/Manage/Cars/index.cshtml", await _context.Cars.ToListAsync());

        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View("/Views/Manage/Cars/Details.cshtml",car);
        }

        public IActionResult Create()
        {
            return View("/Views/Manage/Cars/Create.cshtml");
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Brand,Model,Colour,NumberPlate,Latitude,Longitude,Price,Capacity")] Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Manage/Cars/Create.cshtml",car);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.SingleOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            return View("/Views/Manage/Cars/Edit.cshtml",car);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Colour,NumberPlate,Latitude,Longitude,Price,Capacity")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Manage/Cars/Edit.cshtml", car);
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .SingleOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View("/Views/Manage/Cars/Delete.cshtml",car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.SingleOrDefaultAsync(m => m.Id == id);
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }

    }
}
