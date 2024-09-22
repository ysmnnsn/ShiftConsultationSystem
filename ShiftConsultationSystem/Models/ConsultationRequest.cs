using System.ComponentModel.DataAnnotations;

namespace ShiftConsultationSystem.Models
{
    public class ConsultationRequest
    {
        [Key] public int ConsultationRequestId { get; set; }  // Primary key
        public int RequesterId { get; set; }  // Foreign key to User (the doctor who made the request)
        public User Requester { get; set; }  // Navigation property to User
        
        [Required(ErrorMessage = "Department is required.")]
        public int DepartmentId { get; set; }  // Foreign key to Department
        public Department Department { get; set; }  // Navigation property

        [Required(ErrorMessage = "Hospital is required.")] 
        public int HospitalId { get; set; }  // Foreign key to Hospital
        public Hospital Hospital { get; set; }  // Navigation property

        [Required(ErrorMessage = "Patient name is required.")] 
        public string PatientName { get; set; }  // Name of the patient

        [Required(ErrorMessage = "Room number is required.")]
        public string RoomNumber { get; set; }  // Room number of the patient

        // Add a field for the consultation note
        public string ConsultationNote { get; set; }  // Reason for requesting consultation
        public bool IsHandled { get; set; }  // Whether the consultation request has been handled

        public ICollection<ConsultationAcceptance> Acceptances { get; set; } // Navigation proper
    }

}
