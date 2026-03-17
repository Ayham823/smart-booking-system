using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBookingSystem.Models
{
    public class CoachSchedule
    {
        public int Id { get; set; }

        [Required]
        public int CoachId { get; set; }

        [Range(0, 6)]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;

        [ForeignKey(nameof(CoachId))]
        public Coach? Coach { get; set; }
    }
}
