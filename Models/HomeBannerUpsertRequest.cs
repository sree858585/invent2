using Microsoft.AspNetCore.Http;
using System;

namespace HIVTraining_Vue.Server.Models
{
    public class HomeBannerUpsertRequest
    {
        public string BannerName { get; set; } = string.Empty;
        public string ActionType { get; set; } = "Info"; // Info / Course
        public int? CourseSysId { get; set; }
        public string? ModalTitle { get; set; }
        public string? ModalBodyHtml { get; set; }
        public string? ButtonText { get; set; }
        public int DisplayOrder { get; set; } = 1;
        public bool Active { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IFormFile? File { get; set; }
    }
}