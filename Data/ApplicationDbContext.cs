using System;
using System.Collections.Generic;
using HIVTraining_Vue.Models;
using Microsoft.EntityFrameworkCore;

namespace HIVTraining_Vue.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<CourseListing> CourseListings { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Course> Courses { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure CourseListing entity
        modelBuilder.Entity<CourseListing>(entity =>
        {
            entity.ToTable("CourseListings");

            entity.HasKey(e => e.ListId); // Define primary key

            entity.Property(e => e.ListId)
                .HasColumnName("ListID");

            entity.Property(e => e.SubjectSysId)
                .HasColumnName("SubjectSysID");

            entity.HasOne(d => d.Subject)
                .WithMany(p => p.CourseListings)
                .HasForeignKey(d => d.SubjectSysId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Subject entity
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectSysId)
                .HasName("PK_Subjects_1");

            entity.Property(e => e.SubjectSysId)
                .HasColumnName("SubjectSysID");

            entity.Property(e => e.A3rdPartyCrseId)
                .HasMaxLength(50)
                .HasColumnName("A3rdPartyCrseID");

            entity.Property(e => e.Active)
                .HasDefaultValue(true);

            entity.Property(e => e.Ai)
                .HasDefaultValue(true)
                .HasColumnName("AI");

            entity.Property(e => e.ApprovedCode)
                .HasMaxLength(255);

            entity.Property(e => e.CertDescription)
                .HasColumnType("text");

            entity.Property(e => e.Cnecredits)
                .HasColumnName("CNECredits");

            entity.Property(e => e.CourseTitle)
                .HasMaxLength(255);

            entity.Property(e => e.CreditHrs)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.Property(e => e.Description)
                .HasColumnType("text");

            entity.Property(e => e.Is3rdParty)
                .HasComment("Indicate this is The Gaming Agency course or not")
                .HasColumnName("is3rdParty");

            entity.Property(e => e.IsPeerCore)
                .HasColumnName("isPeerCore");

            entity.Property(e => e.MiscCertDesc)
                .HasColumnType("text");

            entity.Property(e => e.Oasascredits)
                .HasColumnName("OASASCredits");

            entity.Property(e => e.VideoUrl)
                .HasColumnName("VideoURL");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseSysId);

            entity.Property(e => e.CourseSysId).HasColumnName("CourseSysID");
            entity.Property(e => e.ApproveDt).HasColumnType("datetime");
            entity.Property(e => e.CancellReason).HasColumnType("text");
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Coe)
                .HasDefaultValue(false)
                .HasColumnName("COE");
            entity.Property(e => e.CourseDate).HasColumnType("datetime");
            entity.Property(e => e.CourseTime).HasColumnType("text");
            entity.Property(e => e.DateEntered).HasColumnType("datetime");
            entity.Property(e => e.DateModified).HasColumnType("datetime");
            entity.Property(e => e.DisApprvNotes).HasColumnType("text");
            entity.Property(e => e.DisapproveDt).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Information).HasColumnType("text");
            entity.Property(e => e.Information2).HasColumnType("text");
            entity.Property(e => e.OtherFund).HasDefaultValue(false);
            entity.Property(e => e.RegDeadLine).HasColumnType("datetime");
            entity.Property(e => e.Rtc)
                .HasDefaultValue(false)
                .HasColumnName("RTC");
            entity.Property(e => e.SiteSysId).HasColumnName("SiteSysID");
            entity.Property(e => e.SubjectSysId).HasColumnName("SubjectSysID");
            entity.Property(e => e.TrainingLocation).HasColumnType("text");
            entity.Property(e => e.WebinarInst).HasColumnType("text");

            entity.HasOne(e => e.Subject)
               .WithMany(s => s.Courses) // Correct mapping
               .HasForeignKey(e => e.SubjectSysId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_Courses_Subjects");
        });


        base.OnModelCreating(modelBuilder);
    }

}
