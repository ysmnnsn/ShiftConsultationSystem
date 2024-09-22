namespace ShiftConsultationSystem.Models
{
    public class ConsultationHistoryViewModel
    {
        public List<ConsultationRequest> UserRequests { get; set; } // Requests made by the user
        public List<ConsultationAcceptance> AcceptedRequests { get; set; } // Requests the user accepted
    }

}
