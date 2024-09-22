using System.ComponentModel.DataAnnotations;

namespace ShiftConsultationSystem.Models
{
    public class Shift
    {
        [Key] public int ShiftId { get; set; }  // Primary key
        public DateTime ShiftDate { get; set; }  // Date of the shift
        public bool IsFilled { get; set; }  // Whether the shift has been filled
        public int? UserId { get; set; }  // Foreign key to User (nullable)
        public User User { get; set; }  // Navigation property to User
    }

}
