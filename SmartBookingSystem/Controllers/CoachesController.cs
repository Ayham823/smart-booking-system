using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.Controllers
{
    [Authorize]
    public class CoachesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoachesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IQueryable<Coach> query = _context.Coaches
                .Include(c => c.User);

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(c => c.IsActive);
            }

            var coaches = await query.ToListAsync();
            return View(coaches);
        }
        /*
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var coaches = await _context.Coaches
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .ToListAsync();

            return View(coaches);
        }
        */
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var existingCoachUserIds = await _context.Coaches
       .Select(c => c.UserId)
       .ToListAsync();

            var availableUsers = await _userManager.Users
                .Where(u => !existingCoachUserIds.Contains(u.Id))
                .ToListAsync();

            ViewBag.AvailableUsers = availableUsers;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Coach coach)
        {
            var selectedUser = await _userManager.FindByIdAsync(coach.UserId);

            if (selectedUser == null)
            {
                ModelState.AddModelError("UserId", "Selected user not found.");
            }

            var coachExists = await _context.Coaches.AnyAsync(c => c.UserId == coach.UserId);

            if (coachExists)
            {
                ModelState.AddModelError("UserId", "This user is already a coach.");
            }

            if (ModelState.IsValid)
            {
                _context.Coaches.Add(coach);
                await _context.SaveChangesAsync();

                if (!await _userManager.IsInRoleAsync(selectedUser!, "Coach"))
                {
                    await _userManager.AddToRoleAsync(selectedUser!, "Coach");
                }

                TempData["SuccessMessage"] = "Coach created successfully.";
                return RedirectToAction(nameof(Index));
            }

            var existingCoachUserIds = await _context.Coaches
                .Select(c => c.UserId)
                .ToListAsync();

            var availableUsers = await _userManager.Users
                .Where(u => !existingCoachUserIds.Contains(u.Id))
                .ToListAsync();

            ViewBag.AvailableUsers = availableUsers;

            return View(coach);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var coach = await _context.Coaches.FindAsync(id);

            if (coach == null)
            {
                TempData["ErrorMessage"] = "Coach not found.";
                return RedirectToAction(nameof(Index));
            }

            coach.IsActive = true;

            var user = await _userManager.FindByIdAsync(coach.UserId);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Coach"))
            {
                await _userManager.AddToRoleAsync(user, "Coach");
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Coach activated successfully.";
            return RedirectToAction(nameof(Index));
        }
        /*
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var coach = await _context.Coaches.FindAsync(id);

            if (coach == null)
            {
                TempData["ErrorMessage"] = "Coach not found.";
                return RedirectToAction(nameof(Index));
            }

            if (coach.IsActive)
            {
                TempData["ErrorMessage"] = "Coach is already active.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(coach.UserId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Related user not found.";
                return RedirectToAction(nameof(Index));
            }

            var isCoachInRole = await _userManager.IsInRoleAsync(user, "Coach");

            if (!isCoachInRole)
            {
                TempData["ErrorMessage"] = "This user does not have the Coach role, so activation is not allowed.";
                return RedirectToAction(nameof(Index));
            }

            coach.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Coach activated successfully.";
            return RedirectToAction(nameof(Index));
        }
        */
        /*
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var coach = await _context.Coaches.FindAsync(id);

            if (coach == null)
            {
                TempData["ErrorMessage"] = "Coach not found.";
                return RedirectToAction(nameof(Index));
            }

            coach.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Coach activated successfully.";
            return RedirectToAction(nameof(Index));
        }
        */

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches.FindAsync(id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Coach coach)
        {
            if (id != coach.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(coach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coach updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(coach);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coach = await _context.Coaches.FindAsync(id);

            if (coach == null)
            {
                TempData["ErrorMessage"] = "Coach not found.";
                return RedirectToAction(nameof(Index));
            }

            coach.IsActive = false;

            var user = await _userManager.FindByIdAsync(coach.UserId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Coach"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Coach");
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Coach deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }
        /*
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {/*
                var coach = await _context.Coaches.FindAsync(id);

                if (coach == null)
                {
                    TempData["ErrorMessage"] = "Coach not found.";
                    return RedirectToAction(nameof(Index));
                }

                coach.IsActive = false;
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(coach.UserId);
                if (user != null && await _userManager.IsInRoleAsync(user, "Coach"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Coach");
                }

                TempData["SuccessMessage"] = "Coach deactivated successfully.";
                return RedirectToAction(nameof(Index));
            }
            //
          var coach = await _context.Coaches.FindAsync(id);

            if (coach == null)
            {
                TempData["ErrorMessage"] = "Coach not found.";
                return RedirectToAction(nameof(Index));
            }

            coach.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Coach deactivated successfully.";
            return RedirectToAction(nameof(Index));
            */
    }
    }
