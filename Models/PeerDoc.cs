using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class PeerDoc
{
    public int PeerDocSysId { get; set; }

    public int PeerSysId { get; set; }

    public int PeerDocId { get; set; }

    public string DocPath { get; set; } = null!;

    public int DocType { get; set; }

    public DateTime DateUpload { get; set; }

    public DateTime? DateModify { get; set; }

    public bool? Active { get; set; }

    public string? UploadBy { get; set; }

    public bool Reviewed { get; set; }

    public int? CourseSysId { get; set; }
}
