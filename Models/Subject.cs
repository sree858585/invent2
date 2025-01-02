using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HIVTraining_Vue.Models;

public partial class Subject
{
    public Subject()
    {
        CourseListings = new HashSet<CourseListing>();
        Courses = new HashSet<Course>(); // New navigation property

    }

    public int SubjectSysId { get; set; }

    public string? CourseTitle { get; set; }

    public string? Description { get; set; }

    public int? Category { get; set; }

    public bool Ai { get; set; }

    public bool Active { get; set; }

    public string? ApprovedCode { get; set; }

    public string? CertDescription { get; set; }

    public string? MiscCertDesc { get; set; }

    public string? CreditHrs { get; set; }

    public bool Cnecredits { get; set; }

    public bool Oasascredits { get; set; }

    public bool PeerCertCredits { get; set; }

    public string? VideoUrl { get; set; }

    public bool Is3rdParty { get; set; }

    public string? A3rdPartyCrseId { get; set; }

    public bool IsPeerCore { get; set; }

    // Navigation property to CourseListings
    public ICollection<CourseListing> CourseListings { get; set; }

    public ICollection<Course> Courses { get; set; } // Add the missing navigation property

}