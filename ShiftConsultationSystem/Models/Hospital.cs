using System.ComponentModel.DataAnnotations;

namespace ShiftConsultationSystem.Models
{
    public class Hospital
    {
        [Key] public int HospitalId { get; set; }  // Primary key
        public string HospitalName { get; set; }  // Name of the hospital
    }
}
