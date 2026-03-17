using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Enums;
using SmartBookingSystem.Models;
using SmartBookingSystem.ViewModels;
using SmartBookingSystem.Services;

namespace SmartBookingSystem.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AppointmentsController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }


        public async Task<IActionResult> Book()
        {
            var model = new BookAppointmentViewModel
            {
                Date = DateTime.Today,
                Services = await _context.TrainingServices
                    .Where(s => s.IsActive)
                    .ToListAsync(),
                Coaches = await _context.Coaches
                    .Include(c => c.User)
                    .Where(c => c.IsActive)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            model.Services = await _context.TrainingServices
                .Where(s => s.IsActive)
                .ToListAsync();

            model.Coaches = await _context.Coaches
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var service = await _context.TrainingServices
                .FirstOrDefaultAsync(s => s.Id == model.TrainingServiceId && s.IsActive);

            if (service == null)
            {
                ModelState.AddModelError("", "Selected service is invalid.");
                return View(model);
            }

            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == model.CoachId && c.IsActive);

            if (coach == null)
            {
                ModelState.AddModelError("", "Selected coach is invalid.");
                return View(model);
            }

            var startDateTime = model.Date.Date + model.StartTime;
            var endDateTime = startDateTime.AddMinutes(service.DurationInMinutes);

            if (startDateTime < DateTime.Now)
            {
                ModelState.AddModelError("", "You cannot book an appointment in the past.");
                return View(model);
            }

            var dayOfWeek = (int)startDateTime.DayOfWeek;
            var startTime = startDateTime.TimeOfDay;
            var endTime = endDateTime.TimeOfDay;

            var coachSchedule = await _context.CoachSchedules
                .FirstOrDefaultAsync(cs =>
                    cs.CoachId == model.CoachId &&
                    cs.DayOfWeek == dayOfWeek &&
                    cs.IsAvailable &&
                    cs.StartTime <= startTime &&
                    cs.EndTime >= endTime);

            if (coachSchedule == null)
            {
                ModelState.AddModelError("", "The selected time is outside the coach's available schedule.");
                return View(model);
            }

            var hasConflict = await _context.Appointments
    .AnyAsync(a =>
        a.CoachId == model.CoachId &&
        a.Status != AppointmentStatus.Cancelled &&
        startDateTime < a.EndDateTime &&
        endDateTime > a.StartDateTime);

            if (hasConflict)
            {
                ModelState.AddModelError("", "This time slot is already booked.");
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userHasConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    startDateTime < a.EndDateTime &&
                    endDateTime > a.StartDateTime);

            if (userHasConflict)
            {
                ModelState.AddModelError("", "You already have another appointment at this time.");
                return View(model);
            }

            var appointment = new Appointment
            {
                UserId = userId!,
                CoachId = model.CoachId,
                TrainingServiceId = model.TrainingServiceId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Status = AppointmentStatus.Pending,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync(
    userId,
    "Create",
    "Appointment",
    appointment.Id.ToString(),
    $"Appointment created with CoachId={appointment.CoachId}, ServiceId={appointment.TrainingServiceId}, Start={appointment.StartDateTime}"
);
            TempData["SuccessMessage"] = "Appointment booked successfully.";


            return RedirectToAction(nameof(MyAppointments));
        }

        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _context.Appointments
                .Include(a => a.Coach)
                .ThenInclude(c => c.User)
                .Include(a => a.TrainingService)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> MyAppointmentsPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _context.Appointments
                .Include(a => a.Coach)
                .ThenInclude(c => c.User)
                .Include(a => a.TrainingService)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return PartialView("_MyAppointmentsTablePartial", appointments);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int coachId, int serviceId, DateTime date)
        {
            var service = await _context.TrainingServices
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);

            if (service == null)
            {
                return PartialView("_AvailableSlotsPartial", new List<AvailableSlotViewModel>());
            }

            var dayOfWeek = (int)date.DayOfWeek;

            var schedules = await _context.CoachSchedules
                .Where(cs =>
                    cs.CoachId == coachId &&
                    cs.DayOfWeek == dayOfWeek &&
                    cs.IsAvailable)
                .ToListAsync();

            var slots = new List<AvailableSlotViewModel>();

            foreach (var schedule in schedules)
            {
                var current = schedule.StartTime;

                while (current.Add(TimeSpan.FromMinutes(service.DurationInMinutes)) <= schedule.EndTime)
                {
                    var startDateTime = date.Date + current;
                    var endDateTime = startDateTime.AddMinutes(service.DurationInMinutes);

                    if (startDateTime >= DateTime.Now)
                    {
                        var hasConflict = await _context.Appointments
                            .AnyAsync(a =>
                                a.CoachId == coachId &&
                                a.Status != AppointmentStatus.Cancelled &&
                                startDateTime < a.EndDateTime &&
                                endDateTime > a.StartDateTime);

                        if (!hasConflict)
                        {
                            slots.Add(new AvailableSlotViewModel
                            {
                                Value = current.ToString(@"hh\:mm"),
                                Text = current.ToString(@"hh\:mm")
                            });
                        }
                    }

                    current = current.Add(TimeSpan.FromMinutes(30));
                }
            }

            return PartialView("_AvailableSlotsPartial", slots);
        }

        [HttpPost]
        public async Task<IActionResult> AjaxBook([FromBody] AjaxBookAppointmentRequest model)
        {
            var service = await _context.TrainingServices
                .FirstOrDefaultAsync(s => s.Id == model.TrainingServiceId && s.IsActive);

            if (service == null)
            {
                return BadRequest(new { success = false, message = "Selected service is invalid." });
            }

            var coach = await _context.Coaches
                .FirstOrDefaultAsync(c => c.Id == model.CoachId && c.IsActive);

            if (coach == null)
            {
                return BadRequest(new { success = false, message = "Selected coach is invalid." });
            }

            if (!TimeSpan.TryParse(model.StartTime, out var parsedStartTime))
            {
                return BadRequest(new { success = false, message = "Invalid time format." });
            }

            var startDateTime = model.Date.Date + parsedStartTime;
            var endDateTime = startDateTime.AddMinutes(service.DurationInMinutes);

            if (startDateTime < DateTime.Now)
            {
                return BadRequest(new { success = false, message = "You cannot book an appointment in the past." });
            }

            var dayOfWeek = (int)startDateTime.DayOfWeek;
            var startTime = startDateTime.TimeOfDay;
            var endTime = endDateTime.TimeOfDay;

            var coachSchedule = await _context.CoachSchedules
                .FirstOrDefaultAsync(cs =>
                    cs.CoachId == model.CoachId &&
                    cs.DayOfWeek == dayOfWeek &&
                    cs.IsAvailable &&
                    cs.StartTime <= startTime &&
                    cs.EndTime >= endTime);

            if (coachSchedule == null)
            {
                return BadRequest(new { success = false, message = "The selected time is outside the coach's available schedule." });
            }

            var hasConflict = await _context.Appointments
    .AnyAsync(a =>
        a.CoachId == model.CoachId &&
        a.Status != AppointmentStatus.Cancelled &&
        startDateTime < a.EndDateTime &&
        endDateTime > a.StartDateTime);

            if (hasConflict)
            {
                return BadRequest(new { success = false, message = "This time slot is already booked." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userHasConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    startDateTime < a.EndDateTime &&
                    endDateTime > a.StartDateTime);

            if (userHasConflict)
            {
                return BadRequest(new { success = false, message = "You already have another appointment at this time." });
            }

            var appointment = new Appointment
            {
                UserId = userId!,
                CoachId = model.CoachId,
                TrainingServiceId = model.TrainingServiceId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Status = AppointmentStatus.Pending,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync(
                userId,
                "Create",
                "Appointment",
                appointment.Id.ToString(),
                $"Appointment created via Ajax with CoachId={appointment.CoachId}, ServiceId={appointment.TrainingServiceId}, Start={appointment.StartDateTime}"
            );



            return Ok(new { success = true, message = "Appointment booked successfully." });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .Include(a => a.Coach)
                .ThenInclude(c => c.User)
                .Include(a => a.TrainingService)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .Include(a => a.Coach)
                .ThenInclude(c => c.User)
                .Include(a => a.TrainingService)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync(
    userId,
    "Cancel",
    "Appointment",
    appointment.Id.ToString(),
    $"Appointment cancelled. Start={appointment.StartDateTime}"
);

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";

            return RedirectToAction(nameof(MyAppointments));
        }

        [HttpPost]
        public async Task<IActionResult> AjaxCancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Appointment not found." });
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync(
    userId,
    "Cancel",
    "Appointment",
    appointment.Id.ToString(),
    $"Appointment cancelled via Ajax. Start={appointment.StartDateTime}"
);



            return Ok(new { success = true, message = "Appointment cancelled successfully." });
        }
    }

    public class AjaxBookAppointmentRequest
    {
        public int TrainingServiceId { get; set; }
        public int CoachId { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
