using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace HIVTraining_Vue.Server.Models;

public partial class User
{
    public int UserSysId { get; set; }

    public Guid? UserId { get; set; }

    public string? FirstName { get; set; }

    public string? Mi { get; set; }

    public string? LastName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Phone { get; set; }

    public string? CellPhone { get; set; }

    public string? Email { get; set; }

    public string? AltEmail { get; set; }

    public string? Title { get; set; }

    public string? Organization { get; set; }

    public string? Country { get; set; }

    public int? WorkSetting { get; set; }

    public int? Education { get; set; }

    public int? Ethnicity { get; set; }

    public int? Race { get; set; }

    public int? Occupation { get; set; }

    public int? YearsCurrentOccupation { get; set; }

    public DateTime? DateEntered { get; set; }

    public DateTime? DateModified { get; set; }

    public Guid? Role { get; set; }

    public bool Active { get; set; }

    public int? SiteSysId { get; set; }

    public string? WorkPhone { get; set; }

    public string? WorkPhoneExt { get; set; }

    /// <summary>
    /// Special accommodations under the Americans with Disability Act (ADA)
    /// </summary>
    public bool? Adaneed { get; set; }

    /// <summary>
    /// Special accommodations under the Americans with Disability Act (ADA)
    /// </summary>
    public string? Adadetails { get; set; }
}
