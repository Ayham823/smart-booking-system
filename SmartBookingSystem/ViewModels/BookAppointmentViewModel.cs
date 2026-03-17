using System.ComponentModel.DataAnnotations;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.ViewModels
{
    public class BookAppointmentViewModel
    {
        [Required]
        [Display(Name = "Service")]
        public int TrainingServiceId { get; set; }

        [Required]
        [Display(Name = "Coach")]
        public int CoachId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<TrainingService> Services { get; set; } = new();
        public List<Coach> Coaches { get; set; } = new();
    }
}
