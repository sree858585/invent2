using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class ForumTopic
{
    public int TopicId { get; set; }

    public string TopicSubject { get; set; } = null!;

    public DateTime TopicDate { get; set; }

    public string TopicCat { get; set; } = null!;

    public int TopicBy { get; set; }
}
