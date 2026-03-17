using System.ComponentModel.DataAnnotations;

namespace SmartBookingSystem.Models
{
    public class TrainingService
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, 1000)]
        public int DurationInMinutes { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
