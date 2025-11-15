using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class Site
{
    public int SiteSysId { get; set; }

    public string? SiteId { get; set; }

    public int? ParentSiteId { get; set; }

    public string? SiteName { get; set; }

    public string? ShortName { get; set; }

    public string? Address { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? Ext { get; set; }

    public string? ContactEmail { get; set; }

    public string? WebUrl { get; set; }

    public bool Active { get; set; }

    public int Type { get; set; }

    public string? Description { get; set; }

    public int? RegionCode { get; set; }
}
