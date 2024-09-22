using System.ComponentModel.DataAnnotations;

namespace ShiftConsultationSystem.Models
{
    public class Department
    {
        [Key] public int DepartmentId { get; set; }  // Primary key
        public string DepartmentName { get; set; }  // Name of the department
    }
}
