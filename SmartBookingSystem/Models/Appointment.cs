using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartBookingSystem.Enums;

namespace SmartBookingSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CoachId { get; set; }

        [Required]
        public int TrainingServiceId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [ForeignKey(nameof(CoachId))]
        public Coach? Coach { get; set; }

        [ForeignKey(nameof(TrainingServiceId))]
        public TrainingService? TrainingService { get; set; }

        public Payment? Payment { get; set; }
    }
}
