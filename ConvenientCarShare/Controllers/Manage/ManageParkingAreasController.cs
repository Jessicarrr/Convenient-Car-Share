using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConvenientCarShare.Controllers.Manage
{
    [Authorize(Roles = Constants.AdministratorRole)]
    public class ManageParkingAreasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageParkingAreasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View("/Views/Manage/ParkingAreas/Index.cshtml",await _context.ParkingAreas.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingArea = await _context.ParkingAreas
                .SingleOrDefaultAsync(m => m.Id == id);
            if (parkingArea == null)
            {
                return NotFound();
            }

            return View("/Views/Manage/ParkingAreas/Details.cshtml", parkingArea);
        }

        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Latitude,Longitude,MaximumCars")] ParkingArea parkingArea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkingArea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Manage/ParkingAreas/Create.cshtml", parkingArea);
        }

        // GET: ParkingAreas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingArea = await _context.ParkingAreas.SingleOrDefaultAsync(m => m.Id == id);
            if (parkingArea == null)
            {
                return NotFound();
            }
            return View("/Views/Manage/ParkingAreas/Edit.cshtml", parkingArea);
        }

        // POST: ParkingAreas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Latitude,Longitude,MaximumCars")] ParkingArea parkingArea)
        {
            if (id != parkingArea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkingArea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingAreaExists(parkingArea.Id))
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
            return View("/Views/Manage/ParkingAreas/Edit.cshtml", parkingArea);
        }

        // GET: ParkingAreas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingArea = await _context.ParkingAreas
                .SingleOrDefaultAsync(m => m.Id == id);
            if (parkingArea == null)
            {
                return NotFound();
            }

            return View("/Views/Manage/ParkingAreas/Delete.cshtml", parkingArea);
        }

        // POST: ParkingAreas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkingArea = await _context.ParkingAreas.SingleOrDefaultAsync(m => m.Id == id);
            _context.ParkingAreas.Remove(parkingArea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingAreaExists(int id)
        {
            return _context.ParkingAreas.Any(e => e.Id == id);
        }
    }
}
