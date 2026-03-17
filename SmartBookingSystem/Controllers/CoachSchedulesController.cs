using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.Controllers
{
    [Authorize]
    public class CoachSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoachSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.CoachSchedules
                .Include(c => c.Coach)
                .ThenInclude(c => c.User)
                .ToListAsync();

            return View(schedules);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var coaches = await _context.Coaches
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Coaches = coaches;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CoachSchedule coachSchedule)
        {
            if (coachSchedule.StartTime >= coachSchedule.EndTime)
            {
                ModelState.AddModelError("", "Start time must be earlier than end time.");
            }
            var hasOverlap = await _context.CoachSchedules.AnyAsync(s =>
                s.CoachId == coachSchedule.CoachId &&
                s.DayOfWeek == coachSchedule.DayOfWeek &&
                s.Id != coachSchedule.Id &&
                coachSchedule.StartTime < s.EndTime &&
                coachSchedule.EndTime > s.StartTime);

            if (hasOverlap)
            {
                ModelState.AddModelError("", "This schedule overlaps with an existing schedule.");
            }

            if (ModelState.IsValid)
            {
                _context.CoachSchedules.Add(coachSchedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Schedule created successfully.";
                return RedirectToAction(nameof(Index));
            }

            var coaches = await _context.Coaches
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Coaches = coaches;
            return View(coachSchedule);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachSchedule = await _context.CoachSchedules.FindAsync(id);

            if (coachSchedule == null)
            {
                return NotFound();
            }
            if (coachSchedule.StartTime >= coachSchedule.EndTime)
            {
                ModelState.AddModelError("", "Start time must be earlier than end time.");
            }

            var coaches = await _context.Coaches
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Coaches = coaches;
            return View(coachSchedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, CoachSchedule coachSchedule)
        {

            if (id != coachSchedule.Id)
            {
                return NotFound();
            }
            if (coachSchedule.StartTime >= coachSchedule.EndTime)
            {
                ModelState.AddModelError("", "Start time must be earlier than end time.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(coachSchedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Schedule updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            var coaches = await _context.Coaches
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Coaches = coaches;
            return View(coachSchedule);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachSchedule = await _context.CoachSchedules
                .Include(c => c.Coach)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (coachSchedule == null)
            {
                return NotFound();
            }

            return View(coachSchedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coachSchedule = await _context.CoachSchedules.FindAsync(id);

            if (coachSchedule != null)
            {
                _context.CoachSchedules.Remove(coachSchedule);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Schedule deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
