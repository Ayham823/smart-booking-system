using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBookingSystem.Models
{
    public class Coach
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public ICollection<CoachSchedule> CoachSchedules { get; set; } = new List<CoachSchedule>();

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
