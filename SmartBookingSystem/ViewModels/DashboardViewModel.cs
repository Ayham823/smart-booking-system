namespace SmartBookingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalServices { get; set; }
        public int TotalCoaches { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalPayments { get; set; }
        public int TotalAuditLogs { get; set; }
        public decimal TotalRevenue { get; set; }

        public int MyAppointments { get; set; }
        public int MyPayments { get; set; }

        public int CoachAppointments { get; set; }
        public int CoachSchedules { get; set; }
    }
}
