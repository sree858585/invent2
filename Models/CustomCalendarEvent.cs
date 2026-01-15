using System;
using System.ComponentModel.DataAnnotations;

namespace HIVTraining_Vue.Server.Models
{
    public class CustomCalendarEvent
    {
        [Key]
        public int CustomCalendarEventId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(400)]
        public string? ShortDescription { get; set; }

        public string? LongDescription { get; set; }

        // Use UTC to avoid timezone pain
        [Required]
        public DateTime StartUtc { get; set; }

        public DateTime? EndUtc { get; set; }

        public bool AllDay { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }    // Announcement / Holiday / Deadline

        [MaxLength(500)]
        public string? Url { get; set; }

        [MaxLength(10)]
        public string? Color { get; set; }       // "#d32f2f"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    }
}