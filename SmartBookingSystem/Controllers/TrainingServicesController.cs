using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.Controllers
{
    public class TrainingServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.TrainingServices
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(services);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingService = await _context.TrainingServices
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainingService == null)
            {
                return NotFound();
            }

            return View(trainingService);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TrainingService trainingService)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainingService);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(trainingService);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingService = await _context.TrainingServices.FindAsync(id);

            if (trainingService == null)
            {
                return NotFound();
            }

            return View(trainingService);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, TrainingService trainingService)
        {
            if (id != trainingService.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(trainingService);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(trainingService);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainingService = await _context.TrainingServices
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainingService == null)
            {
                return NotFound();
            }

            return View(trainingService);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainingService = await _context.TrainingServices.FindAsync(id);

            if (trainingService == null)
            {
                TempData["ErrorMessage"] = "Service not found.";
                return RedirectToAction(nameof(Index));
            }

            trainingService.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}