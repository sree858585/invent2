namespace HIVTraining_Vue.Server.DTOs.PeerCertification
{
    public class EduCreditDocumentDto
    {
        public int PeerDocSysId { get; set; }

        public int PeerSysId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public decimal? NoOfCredits { get; set; }

        public DateTime DateUpload { get; set; }

        public int ReviewStatus { get; set; }

        public string ReviewStatusText { get; set; } = string.Empty;

        public string? AdminComments { get; set; }
    }
}