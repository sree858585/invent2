// Models/LkPronoun.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIVTraining_Vue.Server.Models
{
    [Table("Lk_Pronouns")]                 // table name, adjust if different
    public class LkPronoun
    {
        [Key]                               // <-- PK required
        public int PronounId { get; set; }  // identity/int PK

        [Required, MaxLength(100)]
        public string Value { get; set; } = null!;   // e.g., "She/Her", "He/Him"

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}