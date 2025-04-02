using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class News
{
    public int NewsSysId { get; set; }

    public int? NewsCategory { get; set; }

    public string? NewsHeader { get; set; }

    public string? NewsAuthor { get; set; }

    public DateTime? NewsDate { get; set; }

    public DateTime? NewsExpireDt { get; set; }

    public string? NewsSummary { get; set; }

    public string? NewsText { get; set; }

    public byte[]? NewsPics { get; set; }

    public string? NewsPicsContentType { get; set; }

    public byte[]? NewsPicsThumb { get; set; }

    public string? NewsPicsLoc { get; set; }

    public string? PostedBy { get; set; }

    public int? SiteSysId { get; set; }

    public bool Published { get; set; }

    public bool Active { get; set; }

    public DateTime? CreateDt { get; set; }

    public bool NewsHeaderShow { get; set; }

    public bool NewsSummaryShow { get; set; }
}
