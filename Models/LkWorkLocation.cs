// Models/LkWorkLocation.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIVTraining_Vue.Server.Models
{
    [Table("Lk_WorkLocations")]                  // table name, adjust if different
    public class LkWorkLocation
    {
        [Key]                                    // <-- PK required
        public int WorkLocationId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;   // e.g., "NYC", "Albany", "Remote"

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}