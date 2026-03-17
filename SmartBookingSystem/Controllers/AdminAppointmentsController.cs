using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Enums;
using SmartBookingSystem.Services;

namespace SmartBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AdminAppointmentsController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Coach)
                .ThenInclude(c => c.User)
                .Include(a => a.TrainingService)
                .Include(a => a.Payment)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Coach)
                .ThenInclude(c => c.User)
                .Include(a => a.TrainingService)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = status;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                null,
                "UpdateStatus",
                "Appointment",
                appointment.Id.ToString(),
                $"Admin changed appointment status to {status}"
            );

            TempData["SuccessMessage"] = "Appointment status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
