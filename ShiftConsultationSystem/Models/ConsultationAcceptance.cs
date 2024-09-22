using System.ComponentModel.DataAnnotations;

namespace ShiftConsultationSystem.Models
{
    public class ConsultationAcceptance
    {
        [Key] public int AcceptanceId { get; set; }  // Primary key
        public int ConsultationRequestId { get; set; }  // Foreign key to ConsultationRequest
        public ConsultationRequest ConsultationRequest { get; set; } // Navigation property
        public int DoctorId { get; set; }  // Foreign key to User (the doctor who accepted the request)
        public User Doctor { get; set; }  // Navigation property to User
        public DateTime AcceptanceDate { get; set; }  // Date the request was accepted
    }
}
