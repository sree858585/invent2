using System;

namespace HIVTraining_Vue.Server.Models;

public partial class PeerDoc
{
    public int PeerDocSysId { get; set; }

    public int PeerSysId { get; set; }

    public int PeerDocId { get; set; }

    // Actual Azure Blob path. Do not change this when admin renames a document.
    public string DocPath { get; set; } = null!;

    public int DocType { get; set; }

    public DateTime DateUpload { get; set; }

    public DateTime? DateModify { get; set; }

    public bool? Active { get; set; }

    public string? UploadBy { get; set; }

    // Keep temporarily for old functionality.
    public bool Reviewed { get; set; }

    public int? CourseSysId { get; set; }

    public decimal? NoOfCredits { get; set; }

    // 0 = Pending, 1 = Approved, 2 = Rejected
    public int ReviewStatus { get; set; }

    // Friendly name displayed in the application.
    public string? DisplayFileName { get; set; }

    // Comments visible to the user.
    public string? AdminComments { get; set; }
}