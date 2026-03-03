using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;

namespace PetLink
{
    public class AnimalListingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalListingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AnimalListings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AnimalListings.Include(a => a.Tutor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AnimalListings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalListing == null)
            {
                return NotFound();
            }

            return View(animalListing);
        }

        // GET: AnimalListings/Create
        public IActionResult Create()
        {
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: AnimalListings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Species,Location,AgeMonths,Description,IsVaccinated,IsDewormed,IsSterilized,Status,CreatedAt,TutorId")] AnimalListing animalListing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(animalListing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // GET: AnimalListings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalListing = await _context.AnimalListings.FindAsync(id);
            if (animalListing == null)
            {
                return NotFound();
            }
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // POST: AnimalListings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,Location,AgeMonths,Description,IsVaccinated,IsDewormed,IsSterilized,Status,CreatedAt,TutorId")] AnimalListing animalListing)
        {
            if (id != animalListing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animalListing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalListingExists(animalListing.Id))
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
            ViewData["TutorId"] = new SelectList(_context.Users, "Id", "Email", animalListing.TutorId);
            return View(animalListing);
        }

        // GET: AnimalListings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animalListing = await _context.AnimalListings
                .Include(a => a.Tutor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animalListing == null)
            {
                return NotFound();
            }

            return View(animalListing);
        }

        // POST: AnimalListings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animalListing = await _context.AnimalListings.FindAsync(id);
            if (animalListing != null)
            {
                _context.AnimalListings.Remove(animalListing);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalListingExists(int id)
        {
            return _context.AnimalListings.Any(e => e.Id == id);
        }
    }
}
