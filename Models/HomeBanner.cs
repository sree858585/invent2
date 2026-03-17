using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIVTraining_Vue.Server.Models
{
    [Table("HomeBanners")]
    public class HomeBanner
    {
        [Key]
        public int HomeBannerSysId { get; set; }

        [Required]
        [MaxLength(200)]
        public string BannerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ActionType { get; set; } = "Info"; 

        public int? CourseSysId { get; set; }

        [MaxLength(200)]
        public string? ModalTitle { get; set; }

        public string? ModalBodyHtml { get; set; }

        [MaxLength(150)]
        public string? ButtonText { get; set; }

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        public int DisplayOrder { get; set; } = 1;

        public bool Active { get; set; } = true;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime DateEntered { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}