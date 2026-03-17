using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.Controllers
{
    [Authorize(Roles = "Coach")]
    public class CoachAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoachAppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (coach == null)
            {
                return NotFound();
            }

            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.TrainingService)
                .Where(a => a.CoachId == coach.Id)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }
    }
}
