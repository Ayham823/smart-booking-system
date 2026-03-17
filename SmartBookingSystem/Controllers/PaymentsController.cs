using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Enums;
using SmartBookingSystem.Models;
using SmartBookingSystem.Services;

namespace SmartBookingSystem.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public PaymentsController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> MyPayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var payments = await _context.Payments
                .Include(p => p.Appointment)
                .ThenInclude(a => a!.TrainingService)
                .Include(p => p.Appointment)
                .ThenInclude(a => a!.Coach)
                .ThenInclude(c => c!.User)
                .Where(p => p.Appointment != null && p.Appointment.UserId == userId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(payments);
        }

        public async Task<IActionResult> Pay(int? appointmentId)
        {
            if (appointmentId == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .Include(a => a.TrainingService)
                .Include(a => a.Coach)
                .ThenInclude(c => c!.User)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Paid)
            {
                return RedirectToAction(nameof(MyPayments));
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayConfirmed(int appointmentId, string paymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .Include(a => a.TrainingService)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Payment == null)
            {
                var payment = new Payment
                {
                    AppointmentId = appointment.Id,
                    Amount = appointment.TrainingService?.Price ?? 0,
                    Status = PaymentStatus.Paid,
                    PaidAt = DateTime.UtcNow,
                    PaymentMethod = paymentMethod
                };

                _context.Payments.Add(payment);
            }
            else
            {
                appointment.Payment.Status = PaymentStatus.Paid;
                appointment.Payment.PaidAt = DateTime.UtcNow;
                appointment.Payment.PaymentMethod = paymentMethod;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                userId,
                "Pay",
                "Payment",
                appointment.Id.ToString(),
                $"Payment completed for AppointmentId={appointment.Id}, Method={paymentMethod}, Amount={appointment.TrainingService?.Price}"
            );
            TempData["SuccessMessage"] = "Payment completed successfully.";
            return RedirectToAction(nameof(MyPayments));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Appointment)
                .ThenInclude(a => a!.TrainingService)
                .Include(p => p.Appointment)
                .ThenInclude(a => a!.Coach)
                .ThenInclude(c => c!.User)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(payments);
        }
    }
}
