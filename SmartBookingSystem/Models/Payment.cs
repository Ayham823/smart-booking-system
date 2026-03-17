using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartBookingSystem.Enums;

namespace SmartBookingSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Range(0, 100000)]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime? PaidAt { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment? Appointment { get; set; }
    }
}
