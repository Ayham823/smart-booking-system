using Microsoft.AspNetCore.Identity;

namespace SmartBookingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
