using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Data;
using SmartBookingSystem.Models;
using SmartBookingSystem.ViewModels;

namespace SmartBookingSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (User.IsInRole("Admin"))
            {
                model.TotalServices = await _context.TrainingServices.CountAsync();
                model.TotalCoaches = await _context.Coaches.CountAsync();
                model.TotalAppointments = await _context.Appointments.CountAsync();
                model.TotalPayments = await _context.Payments.CountAsync();
                model.TotalAuditLogs = await _context.AuditLogs.CountAsync();
                model.TotalRevenue = await _context.Payments
                    .Where(p => p.Status == Enums.PaymentStatus.Paid)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                return View("AdminDashboard", model);
            }

            if (User.IsInRole("Coach"))
            {
                var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (coach != null)
                {
                    model.CoachAppointments = await _context.Appointments.CountAsync(a => a.CoachId == coach.Id);
                    model.CoachSchedules = await _context.CoachSchedules.CountAsync(s => s.CoachId == coach.Id);
                }

                return View("CoachDashboard", model);
            }

            model.MyAppointments = await _context.Appointments.CountAsync(a => a.UserId == user.Id);
            model.MyPayments = await _context.Payments.CountAsync(p => p.Appointment != null && p.Appointment.UserId == user.Id);

            return View("UserDashboard", model);
        }
    }
}
