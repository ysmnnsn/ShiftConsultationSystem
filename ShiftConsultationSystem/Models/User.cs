using System.ComponentModel.DataAnnotations;

namespace ShiftConsultationSystem.Models
{
    public class User
    {
        [Key] public int UserId { get; set; }  // Primary key
        public string Username { get; set; }  // Login username
        public string Password { get; set; }  // Login password
        public string Name { get; set; }  // Full name of the doctor
        public string Role { get; set; }  // "doctor" or "admin"
        public int DepartmentId { get; set; }  // Foreign key to Department
        public Department Department { get; set; }  // Navigation property
        public int HospitalId { get; set; }  // Foreign key to Hospital
        public Hospital Hospital { get; set; }  // Navigation property
    }

}
