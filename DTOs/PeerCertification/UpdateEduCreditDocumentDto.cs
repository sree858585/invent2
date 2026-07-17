using System.ComponentModel.DataAnnotations;

namespace HIVTraining_Vue.Server.DTOs.PeerCertification
{
    public class UpdateEduCreditDocumentDto
    {
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Range(0, 1000)]
        public decimal? NoOfCredits { get; set; }

        [Range(0, 2)]
        public int ReviewStatus { get; set; }

        [StringLength(2000)]
        public string? AdminComments { get; set; }
    }
}