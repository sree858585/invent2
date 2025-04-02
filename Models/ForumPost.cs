using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class ForumPost
{
    public int PostId { get; set; }

    public string PostContent { get; set; } = null!;

    public DateTime PostDate { get; set; }

    public int PostTopic { get; set; }

    public int PostBy { get; set; }
}
