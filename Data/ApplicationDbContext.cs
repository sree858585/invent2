using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HIVTraining_Vue.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AppErrorLog> AppErrorLogs { get; set; }

        public virtual DbSet<AspnetApplication> AspnetApplications { get; set; }

        public virtual DbSet<AspnetMembership> AspnetMemberships { get; set; }

        public virtual DbSet<AspnetPath> AspnetPaths { get; set; }

        public virtual DbSet<AspnetPersonalizationAllUser> AspnetPersonalizationAllUsers { get; set; }

        public virtual DbSet<AspnetPersonalizationPerUser> AspnetPersonalizationPerUsers { get; set; }

        public virtual DbSet<AspnetProfile> AspnetProfiles { get; set; }

        public virtual DbSet<AspnetRole> AspnetRoles { get; set; }

        public virtual DbSet<AspnetSchemaVersion> AspnetSchemaVersions { get; set; }

        public virtual DbSet<AspnetUser> AspnetUsers { get; set; }

        public virtual DbSet<AspnetUsersInRole> AspnetUsersInRoles { get; set; }

        public virtual DbSet<AspnetWebEventEvent> AspnetWebEventEvents { get; set; }

        public virtual DbSet<Course> Courses { get; set; }

        public virtual DbSet<CourseList> CourseLists { get; set; }

        public virtual DbSet<CourseListCategory> CourseListCategories { get; set; }

        public virtual DbSet<CourseListing> CourseListings { get; set; }

        public virtual DbSet<ElmahError> ElmahErrors { get; set; }

        public virtual DbSet<ForumPost> ForumPosts { get; set; }

        public virtual DbSet<ForumTopic> ForumTopics { get; set; }

        public virtual DbSet<FourmCategory> FourmCategories { get; set; }

        public virtual DbSet<Instructor> Instructors { get; set; }

        public virtual DbSet<LkCategory> LkCategories { get; set; }

        public virtual DbSet<LkContractType> LkContractTypes { get; set; }

        public virtual DbSet<LkDeliverable> LkDeliverables { get; set; }

        public virtual DbSet<LkDocType> LkDocTypes { get; set; }

        public virtual DbSet<LkEducation> LkEducations { get; set; }

        public virtual DbSet<LkEthnicity> LkEthnicities { get; set; }

        public virtual DbSet<LkFormat> LkFormats { get; set; }

        public virtual DbSet<LkGender> LkGenders { get; set; }

        public virtual DbSet<LkOccupation> LkOccupations { get; set; }

        public virtual DbSet<LkPeerDocType> LkPeerDocTypes { get; set; }

        public virtual DbSet<LkRace> LkRaces { get; set; }

        public virtual DbSet<LkReferral> LkReferrals { get; set; }

        public virtual DbSet<LkRegionCnty> LkRegionCnties { get; set; }

        public virtual DbSet<LkSiteType> LkSiteTypes { get; set; }

        public virtual DbSet<LkState> LkStates { get; set; }

        public virtual DbSet<LkStatus> LkStatuses { get; set; }

        public virtual DbSet<LkWorkSetting> LkWorkSettings { get; set; }

        public virtual DbSet<LkWorkSettingOld> LkWorkSettingOlds { get; set; }

        public virtual DbSet<LkYearsCurrentOccupation> LkYearsCurrentOccupations { get; set; }

        public virtual DbSet<LkZip> LkZips { get; set; }

        public virtual DbSet<News> News { get; set; }

        public virtual DbSet<PeerAgency> PeerAgencies { get; set; }

        public virtual DbSet<PeerDoc> PeerDocs { get; set; }

        public virtual DbSet<PeerUser> PeerUsers { get; set; }

        public virtual DbSet<Site> Sites { get; set; }

        public virtual DbSet<Subject> Subjects { get; set; }

        public virtual DbSet<TempReset> TempResets { get; set; }

        public virtual DbSet<TmpCourse> TmpCourses { get; set; }

        public virtual DbSet<TmpUserCourse> TmpUserCourses { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserCourse> UserCourses { get; set; }

        public virtual DbSet<VCourseListing> VCourseListings { get; set; }

        public virtual DbSet<VwAspnetApplication> VwAspnetApplications { get; set; }

        public virtual DbSet<VwAspnetMembershipUser> VwAspnetMembershipUsers { get; set; }

        public virtual DbSet<VwAspnetProfile> VwAspnetProfiles { get; set; }

        public virtual DbSet<VwAspnetRole> VwAspnetRoles { get; set; }

        public virtual DbSet<VwAspnetUser> VwAspnetUsers { get; set; }

        public virtual DbSet<VwAspnetUsersInRole> VwAspnetUsersInRoles { get; set; }

        public virtual DbSet<VwAspnetWebPartStatePath> VwAspnetWebPartStatePaths { get; set; }

        public virtual DbSet<VwAspnetWebPartStateShared> VwAspnetWebPartStateShareds { get; set; }

        public virtual DbSet<VwAspnetWebPartStateUser> VwAspnetWebPartStateUsers { get; set; }

        public virtual DbSet<VwMembershipUser> VwMembershipUsers { get; set; }

        public DbSet<CourseSession> CourseSessions { get; set; }

        public DbSet<LkPronoun> LkPronouns { get; set; } = default!;
        public DbSet<LkWorkLocation> LkWorkLocations { get; set; } = default!;

        public virtual DbSet<ScormAiccSession> ScormAiccSessions { get; set; }
        public virtual DbSet<ScormScoesTrack> ScormScoesTracks { get; set; }

        // optional
        public virtual DbSet<Scorm> Scorms { get; set; }
        public virtual DbSet<ScormScoesData> ScormScoesDatas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important to call this for Identity support

            modelBuilder.Entity<AppErrorLog>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("App_Error_Log");

                entity.Property(e => e.DateOccur).HasColumnType("datetime");
                entity.Property(e => e.LogId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("LogID");
                entity.Property(e => e.Message).HasColumnType("text");
            });
            // Pronouns
            modelBuilder.Entity<LkPronoun>(e =>
            {
                e.HasKey(x => x.PronounId);
                e.Property(x => x.Value).IsRequired().HasMaxLength(100);
            });

            // WorkLocations
            modelBuilder.Entity<LkWorkLocation>(e =>
            {
                e.HasKey(x => x.WorkLocationId);
                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            });
            modelBuilder.Entity<AspnetApplication>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("PK__aspnet_A__C93A4C98668D0419")
                    .IsClustered(false);

                entity.ToTable("aspnet_Applications");

                entity.HasIndex(e => e.LoweredApplicationName, "UQ__aspnet_A__17477DE4D4122DBC").IsUnique();

                entity.HasIndex(e => e.ApplicationName, "UQ__aspnet_A__30910331E3120819").IsUnique();

                entity.Property(e => e.ApplicationId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.ApplicationName).HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(256);
                entity.Property(e => e.LoweredApplicationName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspnetMembership>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__aspnet_M__1788CC4D801170F6")
                    .IsClustered(false);

                entity.ToTable("aspnet_Membership");

                entity.Property(e => e.UserId).ValueGeneratedNever();
                entity.Property(e => e.Comment).HasColumnType("ntext");
                entity.Property(e => e.CreateDate).HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.FailedPasswordAnswerAttemptWindowStart).HasColumnType("datetime");
                entity.Property(e => e.FailedPasswordAttemptWindowStart).HasColumnType("datetime");
                entity.Property(e => e.LastLockoutDate).HasColumnType("datetime");
                entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
                entity.Property(e => e.LastPasswordChangedDate).HasColumnType("datetime");
                entity.Property(e => e.LoweredEmail).HasMaxLength(256);
                entity.Property(e => e.MobilePin)
                    .HasMaxLength(16)
                    .HasColumnName("MobilePIN");
                entity.Property(e => e.Password).HasMaxLength(128);
                entity.Property(e => e.PasswordAnswer).HasMaxLength(128);
                entity.Property(e => e.PasswordQuestion).HasMaxLength(256);
                entity.Property(e => e.PasswordSalt).HasMaxLength(128);

                entity.HasOne(d => d.Application).WithMany(p => p.AspnetMemberships)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Me__Appli__3D2915A8");

                entity.HasOne(d => d.User).WithOne(p => p.AspnetMembership)
                    .HasForeignKey<AspnetMembership>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Me__UserI__3E1D39E1");
            });

            modelBuilder.Entity<AspnetPath>(entity =>
            {
                entity.HasKey(e => e.PathId)
                    .HasName("PK__aspnet_P__CD67DC58A7818CC6")
                    .IsClustered(false);

                entity.ToTable("aspnet_Paths");

                entity.Property(e => e.PathId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.LoweredPath).HasMaxLength(256);
                entity.Property(e => e.Path).HasMaxLength(256);

                entity.HasOne(d => d.Application).WithMany(p => p.AspnetPaths)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Pa__Appli__3F115E1A");
            });

            modelBuilder.Entity<AspnetPersonalizationAllUser>(entity =>
            {
                entity.HasKey(e => e.PathId).HasName("PK__aspnet_P__CD67DC5997B06E07");

                entity.ToTable("aspnet_PersonalizationAllUsers");

                entity.Property(e => e.PathId).ValueGeneratedNever();
                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.PageSettings).HasColumnType("image");

                entity.HasOne(d => d.Path).WithOne(p => p.AspnetPersonalizationAllUser)
                    .HasForeignKey<AspnetPersonalizationAllUser>(d => d.PathId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Pe__PathI__40058253");
            });

            modelBuilder.Entity<AspnetPersonalizationPerUser>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__aspnet_P__3214EC06320E71C6")
                    .IsClustered(false);

                entity.ToTable("aspnet_PersonalizationPerUser");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.PageSettings).HasColumnType("image");

                entity.HasOne(d => d.Path).WithMany(p => p.AspnetPersonalizationPerUsers)
                    .HasForeignKey(d => d.PathId)
                    .HasConstraintName("FK__aspnet_Pe__PathI__40F9A68C");

                entity.HasOne(d => d.User).WithMany(p => p.AspnetPersonalizationPerUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__aspnet_Pe__UserI__41EDCAC5");
            });

            modelBuilder.Entity<AspnetProfile>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__aspnet_P__1788CC4CBFEBA5D1");

                entity.ToTable("aspnet_Profile");

                entity.Property(e => e.UserId).ValueGeneratedNever();
                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.PropertyNames).HasColumnType("ntext");
                entity.Property(e => e.PropertyValuesBinary).HasColumnType("image");
                entity.Property(e => e.PropertyValuesString).HasColumnType("ntext");

                entity.HasOne(d => d.User).WithOne(p => p.AspnetProfile)
                    .HasForeignKey<AspnetProfile>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Pr__UserI__42E1EEFE");
            });

            modelBuilder.Entity<AspnetRole>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__aspnet_R__8AFACE1B69150FBA")
                    .IsClustered(false);

                entity.ToTable("aspnet_Roles");

                entity.Property(e => e.RoleId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Description).HasMaxLength(256);
                entity.Property(e => e.LoweredRoleName).HasMaxLength(256);
                entity.Property(e => e.RoleName).HasMaxLength(256);

                entity.HasOne(d => d.Application).WithMany(p => p.AspnetRoles)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Ro__Appli__43D61337");
            });

            modelBuilder.Entity<AspnetSchemaVersion>(entity =>
            {
                entity.HasKey(e => new { e.Feature, e.CompatibleSchemaVersion }).HasName("PK__aspnet_S__5A1E6BC1C0F948FE");

                entity.ToTable("aspnet_SchemaVersions");

                entity.Property(e => e.Feature).HasMaxLength(128);
                entity.Property(e => e.CompatibleSchemaVersion).HasMaxLength(128);
            });

            modelBuilder.Entity<AspnetUser>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__aspnet_U__1788CC4D37DE908F")
                    .IsClustered(false);

                entity.ToTable("aspnet_Users");

                entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.LastActivityDate).HasColumnType("datetime");
                entity.Property(e => e.LoweredUserName).HasMaxLength(256);
                entity.Property(e => e.MobileAlias)
                    .HasMaxLength(16)
                    .HasDefaultValueSql("(NULL)");
                entity.Property(e => e.UserName).HasMaxLength(256);

                entity.HasOne(d => d.Application).WithMany(p => p.AspnetUsers)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Us__Appli__44CA3770");
            });

            modelBuilder.Entity<AspnetUsersInRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__aspnet_U__AF2760ADF9A5F05F");

                entity.ToTable("aspnet_UsersInRoles");

                entity.HasOne(d => d.Role).WithMany(p => p.AspnetUsersInRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__aspnet_Us__RoleI__45BE5BA9");
            });

            modelBuilder.Entity<AspnetWebEventEvent>(entity =>
            {
                entity.HasKey(e => e.EventId).HasName("PK__aspnet_W__7944C810DE753F38");

                entity.ToTable("aspnet_WebEvent_Events");

                entity.Property(e => e.EventId)
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.ApplicationPath).HasMaxLength(256);
                entity.Property(e => e.ApplicationVirtualPath).HasMaxLength(256);
                entity.Property(e => e.Details).HasColumnType("ntext");
                entity.Property(e => e.EventOccurrence).HasColumnType("decimal(19, 0)");
                entity.Property(e => e.EventSequence).HasColumnType("decimal(19, 0)");
                entity.Property(e => e.EventTime).HasColumnType("datetime");
                entity.Property(e => e.EventTimeUtc).HasColumnType("datetime");
                entity.Property(e => e.EventType).HasMaxLength(256);
                entity.Property(e => e.ExceptionType).HasMaxLength(256);
                entity.Property(e => e.MachineName).HasMaxLength(256);
                entity.Property(e => e.Message).HasMaxLength(1024);
                entity.Property(e => e.RequestUrl).HasMaxLength(1024);
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
            });

            modelBuilder.Entity<CourseList>(entity =>
            {
                entity.HasKey(e => e.ListId);

                entity.ToTable("CourseList");

                entity.Property(e => e.ListId).HasColumnName("ListID");
                entity.Property(e => e.CourseListSysId).HasColumnName("CourseListSysID");
                entity.Property(e => e.CreateDt).HasColumnType("datetime");
                entity.Property(e => e.SubjectSysId).HasColumnName("SubjectSysID");
                entity.Property(e => e.Title).HasMaxLength(250);
            });

            modelBuilder.Entity<CourseListCategory>(entity =>
            {
                entity.HasKey(e => e.CourseListSysId);

                entity.Property(e => e.CourseListSysId).HasColumnName("CourseListSysID");
                entity.Property(e => e.CategoryTitle).HasMaxLength(250);
                entity.Property(e => e.CreateDt).HasColumnType("datetime");
                entity.Property(e => e.PageTitle).HasMaxLength(250);
            });

            modelBuilder.Entity<CourseListing>(entity =>
            {
                entity.HasKey(e => e.ListId);

                entity.Property(e => e.ListId).HasColumnName("ListID");
                entity.Property(e => e.CreateDt).HasColumnType("datetime");
                entity.Property(e => e.SubjectSysId).HasColumnName("SubjectSysID");
                entity.Property(e => e.Title).HasMaxLength(250);
            });

            modelBuilder.Entity<ElmahError>(entity =>
            {
                entity.HasKey(e => e.ErrorId).IsClustered(false);

                entity.ToTable("ELMAH_Error");

                entity.Property(e => e.ErrorId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.AllXml).HasColumnType("ntext");
                entity.Property(e => e.Application).HasMaxLength(60);
                entity.Property(e => e.Host).HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.Sequence).ValueGeneratedOnAdd();
                entity.Property(e => e.Source).HasMaxLength(60);
                entity.Property(e => e.TimeUtc).HasColumnType("datetime");
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.User).HasMaxLength(50);
            });

            modelBuilder.Entity<ForumPost>(entity =>
            {
                entity.HasKey(e => e.PostId);

                entity.ToTable("Forum_Post");

                entity.Property(e => e.PostId).HasColumnName("Post_ID");
                entity.Property(e => e.PostBy).HasColumnName("Post_By");
                entity.Property(e => e.PostContent)
                    .HasColumnType("text")
                    .HasColumnName("Post_Content");
                entity.Property(e => e.PostDate)
                    .HasColumnType("datetime")
                    .HasColumnName("Post_Date");
                entity.Property(e => e.PostTopic).HasColumnName("Post_Topic");
            });

            modelBuilder.Entity<ForumTopic>(entity =>
            {
                entity.HasKey(e => e.TopicId);

                entity.ToTable("Forum_Topics");

                entity.Property(e => e.TopicId).HasColumnName("Topic_ID");
                entity.Property(e => e.TopicBy).HasColumnName("Topic_By");
                entity.Property(e => e.TopicCat)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("Topic_Cat");
                entity.Property(e => e.TopicDate)
                    .HasColumnType("datetime")
                    .HasColumnName("Topic_Date");
                entity.Property(e => e.TopicSubject)
                    .HasMaxLength(300)
                    .HasColumnName("Topic_Subject");
            });

            modelBuilder.Entity<FourmCategory>(entity =>
            {
                entity.HasKey(e => e.CatId);

                entity.ToTable("Fourm_Categories");

                entity.Property(e => e.CatId).HasColumnName("Cat_ID");
                entity.Property(e => e.CatName)
                    .HasMaxLength(300)
                    .HasColumnName("Cat_Name");
                entity.Property(e => e.Description).HasMaxLength(300);
            });

            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.HasKey(e => e.InstructorSysId);

                entity.Property(e => e.InstructorSysId).HasColumnName("InstructorSysID");
                entity.Property(e => e.CellPhone).HasMaxLength(20);
                entity.Property(e => e.InsNotes).HasColumnType("text");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.SiteSysId).HasColumnName("SiteSysID");
            });

            modelBuilder.Entity<LkCategory>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_Categories");

                entity.Property(e => e.Value)
                    .HasMaxLength(50)
                    .IsFixedLength();
            });

            modelBuilder.Entity<LkContractType>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_ContractType");

                entity.Property(e => e.Value)
                    .HasMaxLength(3)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LkDeliverable>(entity =>
            {
                entity.ToTable("lk_Deliverable");

                entity.Property(e => e.Value)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LkDocType>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_DocType");

                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.Code).HasMaxLength(10);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");
                entity.Property(e => e.Value).HasMaxLength(100);
            });

            modelBuilder.Entity<LkEducation>(entity =>
            {
                entity.HasKey(e => e.Code).HasName("PK_education");

                entity.ToTable("lk_Education");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkEthnicity>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_Ethnicity");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkFormat>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_Formats");

                entity.Property(e => e.Value)
                    .HasMaxLength(30)
                    .IsFixedLength();
            });

            modelBuilder.Entity<LkGender>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_Gender");

                entity.Property(e => e.Code).ValueGeneratedOnAdd();
                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkOccupation>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_Occupation");

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkPeerDocType>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_Peer_Doc_Type");

                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.DocAbbrev).HasMaxLength(50);
                entity.Property(e => e.Mandatary).HasDefaultValue(true);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.PeerDocId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PeerDocID");
            });

            modelBuilder.Entity<LkRace>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_Race");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkReferral>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_Referral");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkRegionCnty>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_Region_Cnty");

                entity.Property(e => e.Code).ValueGeneratedOnAdd();
                entity.Property(e => e.ParentId).HasColumnName("Parent_ID");
                entity.Property(e => e.Value)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LkSiteType>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_SiteType");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkState>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_State");

                entity.Property(e => e.Code).HasMaxLength(2);
                entity.Property(e => e.Value).HasMaxLength(25);
            });

            modelBuilder.Entity<LkStatus>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_Status");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkWorkSetting>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("lk_WorkSetting");

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkWorkSettingOld>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_WorkSetting_old");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkYearsCurrentOccupation>(entity =>
            {
                entity.HasKey(e => e.Code).HasName("PK_YearsCurrentOccupation");

                entity.ToTable("lk_YearsCurrentOccupation");

                entity.Property(e => e.Value).HasMaxLength(50);
            });

            modelBuilder.Entity<LkZip>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("lk_Zip");

                entity.Property(e => e.Cntycode).HasColumnName("CNTYCODE");
                entity.Property(e => e.Cntyname)
                    .HasMaxLength(255)
                    .HasColumnName("CNTYNAME");
                entity.Property(e => e.Stfips)
                    .HasMaxLength(255)
                    .HasColumnName("STFIPS");
                entity.Property(e => e.Zip)
                    .HasMaxLength(255)
                    .HasColumnName("ZIP");
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.HasKey(e => e.NewsSysId);

                entity.Property(e => e.NewsSysId).HasColumnName("NewsSysID");
                entity.Property(e => e.CreateDt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NewsAuthor)
                    .HasMaxLength(256)
                    .IsUnicode(false);
                entity.Property(e => e.NewsDate).HasColumnType("datetime");
                entity.Property(e => e.NewsExpireDt).HasColumnType("datetime");
                entity.Property(e => e.NewsHeader).IsUnicode(false);
                entity.Property(e => e.NewsHeaderShow).HasDefaultValue(true);
                entity.Property(e => e.NewsPics).HasColumnType("image");
                entity.Property(e => e.NewsPicsContentType)
                    .HasMaxLength(250)
                    .IsUnicode(false);
                entity.Property(e => e.NewsPicsLoc)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.NewsPicsThumb).HasColumnType("image");
                entity.Property(e => e.NewsSummary).HasColumnType("text");
                entity.Property(e => e.NewsSummaryShow).HasDefaultValue(true);
                entity.Property(e => e.NewsText).HasColumnType("text");
                entity.Property(e => e.PostedBy)
                    .HasMaxLength(256)
                    .IsUnicode(false);
                entity.Property(e => e.SiteSysId).HasColumnName("SiteSysID");
            });

            modelBuilder.Entity<PeerAgency>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("Peer_Agency");

                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Address2).HasMaxLength(200);
                entity.Property(e => e.Agency).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.CreateDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PeerAgencySysId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PeerAgencySysID");
                entity.Property(e => e.PeerSysId).HasColumnName("PeerSysID");
                entity.Property(e => e.State)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Zip)
                    .HasMaxLength(10)
                    .HasColumnName("ZIP");
            });

            modelBuilder.Entity<PeerDoc>(entity =>
            {
                entity.HasKey(e => e.PeerDocSysId);

                entity.ToTable("Peer_Doc");

                entity.Property(e => e.PeerDocSysId).HasColumnName("PeerDocSysID");
                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.CourseSysId).HasColumnName("CourseSysID");
                entity.Property(e => e.DateModify).HasColumnType("datetime");
                entity.Property(e => e.DateUpload)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DocPath).HasColumnType("text");
                entity.Property(e => e.PeerDocId).HasColumnName("PeerDocID");
                entity.Property(e => e.PeerSysId).HasColumnName("PeerSysID");
                entity.Property(e => e.UploadBy).HasMaxLength(100);
            });

            modelBuilder.Entity<PeerUser>(entity =>
            {
                entity.HasKey(e => e.PeerSysId);

                entity.ToTable("Peer_User");

                entity.Property(e => e.PeerSysId).HasColumnName("PeerSysID");
                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.AgencyAffilation).HasMaxLength(2000);
                entity.Property(e => e.ApplicantNumber).HasColumnName("Applicant_Number");
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.ApprovedDt).HasColumnType("datetime");
                entity.Property(e => e.CertCriminalJusticeDate).HasColumnType("datetime");
                entity.Property(e => e.CertHcv)
                    .HasDefaultValue(false)
                    .HasColumnName("CertHCV");
                entity.Property(e => e.CertHcvdate)
                    .HasColumnType("datetime")
                    .HasColumnName("CertHCVDate");
                entity.Property(e => e.CertHiv)
                    .HasDefaultValue(false)
                    .HasColumnName("CertHIV");
                entity.Property(e => e.CertHivdate)
                    .HasColumnType("datetime")
                    .HasColumnName("CertHIVDate");
                entity.Property(e => e.CertHr)
                    .HasDefaultValue(false)
                    .HasColumnName("CertHR");
                entity.Property(e => e.CertHrdate)
                    .HasColumnType("datetime")
                    .HasColumnName("CertHRDate");
                entity.Property(e => e.CertPrepDate).HasColumnType("datetime");
                entity.Property(e => e.ComplPracticum).HasDefaultValue(false);
                entity.Property(e => e.ComplPracticumMin).HasDefaultValue(false);
                entity.Property(e => e.DateCert).HasColumnType("datetime");
                entity.Property(e => e.DateCompletion).HasColumnType("datetime");
                entity.Property(e => e.DateCreate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DateModify)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DisapprovedBy).HasMaxLength(100);
                entity.Property(e => e.DisapprovedDt).HasColumnType("datetime");
                entity.Property(e => e.DiscardDt).HasColumnType("datetime");
                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");
                entity.Property(e => e.ExperienceChallenges).HasMaxLength(2500);
                entity.Property(e => e.ExperienceCommitment).HasMaxLength(2500);
                entity.Property(e => e.ExperienceWhy).HasMaxLength(2500);
                entity.Property(e => e.Hsdiploma)
                    .HasDefaultValue(false)
                    .HasColumnName("HSDiploma");
                entity.Property(e => e.Notes).HasColumnType("text");
                entity.Property(e => e.PracticumBdate)
                    .HasColumnType("datetime")
                    .HasColumnName("PracticumBDate");
                entity.Property(e => e.PracticumEdate)
                    .HasColumnType("datetime")
                    .HasColumnName("PracticumEDate");
                entity.Property(e => e.ReasonDisapprv).HasColumnType("text");
                entity.Property(e => e.ReenterDt).HasColumnType("datetime");
                entity.Property(e => e.SupvrContAddr1).HasMaxLength(300);
                entity.Property(e => e.SupvrContAddr2).HasMaxLength(300);
                entity.Property(e => e.SupvrContCity).HasMaxLength(100);
                entity.Property(e => e.SupvrContEmail).HasMaxLength(200);
                entity.Property(e => e.SupvrContPhone).HasMaxLength(13);
                entity.Property(e => e.SupvrContState).HasMaxLength(50);
                entity.Property(e => e.SupvrContZip).HasMaxLength(10);
                entity.Property(e => e.SupvrFirstName).HasMaxLength(100);
                entity.Property(e => e.SupvrLastName).HasMaxLength(100);
                entity.Property(e => e.SupvrOrgName).HasMaxLength(200);
                entity.Property(e => e.UserExper).HasColumnType("text");
                entity.Property(e => e.UserSysId).HasColumnName("UserSysID");
            });

            modelBuilder.Entity<Site>(entity =>
            {
                entity.HasKey(e => e.SiteSysId).HasName("PK_Sites_1");

                entity.Property(e => e.SiteSysId).HasColumnName("SiteSysID");
                entity.Property(e => e.Address).HasMaxLength(100);
                entity.Property(e => e.Address2).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.ContactName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.Ext)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.ParentSiteId)
                    .HasDefaultValue(0)
                    .HasColumnName("ParentSiteID");
                entity.Property(e => e.ShortName).HasMaxLength(100);
                entity.Property(e => e.SiteId)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("SiteID");
                entity.Property(e => e.SiteName).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(2);
                entity.Property(e => e.WebUrl)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("WebURL");
                entity.Property(e => e.Zip)
                    .HasMaxLength(10)
                    .HasColumnName("ZIP");
            });

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.SubjectSysId).HasName("PK_Subjects_1");

                entity.Property(e => e.SubjectSysId).HasColumnName("SubjectSysID");
                entity.Property(e => e.A3rdPartyCrseId)
                    .HasMaxLength(50)
                    .HasColumnName("A3rdPartyCrseID");
                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.Ai)
                    .HasDefaultValue(true)
                    .HasColumnName("AI");
                entity.Property(e => e.ApprovedCode).HasMaxLength(255);
                entity.Property(e => e.CertDescription).HasColumnType("text");
                entity.Property(e => e.Cnecredits).HasColumnName("CNECredits");
                entity.Property(e => e.CourseTitle).HasMaxLength(255);
                entity.Property(e => e.CreditHrs)
                    .HasMaxLength(10)
                    .IsFixedLength();
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Is3rdParty)
                    .HasComment("Indicate this is The Gaming Agency course or not")
                    .HasColumnName("is3rdParty");
                entity.Property(e => e.IsPeerCore).HasColumnName("isPeerCore");
                entity.Property(e => e.MiscCertDesc).HasColumnType("text");
                entity.Property(e => e.Oasascredits).HasColumnName("OASASCredits");
                entity.Property(e => e.VideoUrl).HasColumnName("VideoURL");
            });

            modelBuilder.Entity<TempReset>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TempRese__3214EC274AC0AF95");

                entity.ToTable("TempReset");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");
                entity.Property(e => e.Temp)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("temp");
                entity.Property(e => e.User)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("user");
            });

            modelBuilder.Entity<TmpCourse>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("tmpCourses");

                entity.Property(e => e.CancellReason).HasColumnType("text");
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.Coe).HasColumnName("COE");
                entity.Property(e => e.CourseDate).HasColumnType("datetime");
                entity.Property(e => e.CourseSysId).HasColumnName("CourseSysID");
                entity.Property(e => e.CourseTime).HasColumnType("text");
                entity.Property(e => e.DateEntered).HasColumnType("datetime");
                entity.Property(e => e.DateModified).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.Information).HasColumnType("text");
                entity.Property(e => e.RegDeadLine).HasColumnType("datetime");
                entity.Property(e => e.Rtc).HasColumnName("RTC");
                entity.Property(e => e.SiteSysId).HasColumnName("SiteSysID");
                entity.Property(e => e.SubjectSysId).HasColumnName("SubjectSysID");
                entity.Property(e => e.TrainingLocation).HasColumnType("text");
            });

            modelBuilder.Entity<TmpUserCourse>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("tmpUserCourses");

                entity.Property(e => e.Attempt).HasColumnName("attempt");
                entity.Property(e => e.CancelReason).HasColumnType("text");
                entity.Property(e => e.CourseSysId).HasColumnName("CourseSysID");
                entity.Property(e => e.DateEntered).HasColumnType("datetime");
                entity.Property(e => e.DateModified).HasColumnType("datetime");
                entity.Property(e => e.DateStatusChanged).HasColumnType("datetime");
                entity.Property(e => e.Score).HasColumnName("score");
                entity.Property(e => e.Token).HasDefaultValueSql("(newid())");
                entity.Property(e => e.UserCourseSysId).HasColumnName("UserCourseSysID");
                entity.Property(e => e.UserSysId).HasColumnName("UserSysID");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserSysId);

                entity.Property(e => e.UserSysId).HasColumnName("UserSysID");
                entity.Property(e => e.Active).HasDefaultValue(true);

                entity.Property(e => e.Adadetails)
                    .HasComment("Special accommodations under the Americans with Disability Act (ADA)")
                    .HasColumnName("ADADetails");

                entity.Property(e => e.Adaneed)
                    .HasDefaultValue(false)
                    .HasComment("Special accommodations under the Americans with Disability Act (ADA)")
                    .HasColumnName("ADANeed");

                entity.Property(e => e.Address).HasMaxLength(100);
                entity.Property(e => e.AltEmail).HasMaxLength(200);
                entity.Property(e => e.CellPhone).HasMaxLength(20);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.Country).HasMaxLength(200);
                entity.Property(e => e.DateEntered).HasColumnType("datetime");
                entity.Property(e => e.DateModified).HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Mi)
                    .HasMaxLength(1)
                    .HasColumnName("MI");

                entity.Property(e => e.Organization).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);

                entity.Property(e => e.Role)
                    .HasDefaultValueSql("([dbo].[GetRoleUniqueID]('user'))");

                entity.Property(e => e.SiteSysId)
                    .HasDefaultValue(0)
                    .HasColumnName("SiteSysID");

                entity.Property(e => e.State)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.Title).HasMaxLength(50);
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.WorkPhone).HasMaxLength(20);
                entity.Property(e => e.WorkPhoneExt).HasMaxLength(10);

                entity.Property(e => e.Zip)
                    .HasMaxLength(10)
                    .HasColumnName("ZIP");

                // ===== NEW FIELDS (add these in your User class too) =====

                // Alt phone (optional)
                entity.Property(e => e.AltPhone).HasMaxLength(20);

                // Can receive texts flags (nullable bools are fine)
                entity.Property(e => e.PrimaryCanText);
                entity.Property(e => e.AltCanText);

                // Pronouns FK
                entity.Property(e => e.PronounId);
                entity.HasOne(e => e.Pronoun)
                    .WithMany()
                    .HasForeignKey(e => e.PronounId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Work Location FK
                entity.Property(e => e.WorkLocationId);
                entity.HasOne(e => e.WorkLocation)
                    .WithMany()
                    .HasForeignKey(e => e.WorkLocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserCourse>(entity =>
            {
                entity.HasKey(e => e.UserCourseSysId);

                entity.Property(e => e.UserCourseSysId).HasColumnName("UserCourseSysID");
                entity.Property(e => e.Adadetails)
                    .HasComment("Special accommodations under the Americans with Disability Act (ADA)")
                    .HasColumnName("ADADetails");
                entity.Property(e => e.Adaneed)
                    .HasDefaultValue(false)
                    .HasComment("Special accommodations under the Americans with Disability Act (ADA)")
                    .HasColumnName("ADANeed");
                entity.Property(e => e.Attempt).HasColumnName("attempt");
                entity.Property(e => e.Attended).HasDefaultValue(false);
                entity.Property(e => e.CancelReason).HasColumnType("text");
                entity.Property(e => e.CourseSysId).HasColumnName("CourseSysID");
                entity.Property(e => e.DateEntered).HasColumnType("datetime");
                entity.Property(e => e.DateModified).HasColumnType("datetime");
                entity.Property(e => e.DateStatusChanged).HasColumnType("datetime");
                entity.Property(e => e.EmailSend).HasDefaultValue(false);
                entity.Property(e => e.Score).HasColumnName("score");
                entity.Property(e => e.UserSysId).HasColumnName("UserSysID");
            });

            modelBuilder.Entity<VCourseListing>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("v_CourseListings");

                entity.Property(e => e.CourseTitle).HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.ListId).HasColumnName("ListID");
                entity.Property(e => e.SubjectSysId).HasColumnName("SubjectSysID");
            });

            modelBuilder.Entity<VwAspnetApplication>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_Applications");

                entity.Property(e => e.ApplicationName).HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(256);
                entity.Property(e => e.LoweredApplicationName).HasMaxLength(256);
            });

            modelBuilder.Entity<VwAspnetMembershipUser>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_MembershipUsers");

                entity.Property(e => e.Comment).HasColumnType("ntext");
                entity.Property(e => e.CreateDate).HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.FailedPasswordAnswerAttemptWindowStart).HasColumnType("datetime");
                entity.Property(e => e.FailedPasswordAttemptWindowStart).HasColumnType("datetime");
                entity.Property(e => e.LastActivityDate).HasColumnType("datetime");
                entity.Property(e => e.LastLockoutDate).HasColumnType("datetime");
                entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
                entity.Property(e => e.LastPasswordChangedDate).HasColumnType("datetime");
                entity.Property(e => e.LoweredEmail).HasMaxLength(256);
                entity.Property(e => e.MobileAlias).HasMaxLength(16);
                entity.Property(e => e.MobilePin)
                    .HasMaxLength(16)
                    .HasColumnName("MobilePIN");
                entity.Property(e => e.PasswordAnswer).HasMaxLength(128);
                entity.Property(e => e.PasswordQuestion).HasMaxLength(256);
                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<VwAspnetProfile>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_Profiles");

                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwAspnetRole>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_Roles");

                entity.Property(e => e.Description).HasMaxLength(256);
                entity.Property(e => e.LoweredRoleName).HasMaxLength(256);
                entity.Property(e => e.RoleName).HasMaxLength(256);
            });

            modelBuilder.Entity<VwAspnetUser>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_Users");

                entity.Property(e => e.LastActivityDate).HasColumnType("datetime");
                entity.Property(e => e.LoweredUserName).HasMaxLength(256);
                entity.Property(e => e.MobileAlias).HasMaxLength(16);
                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<VwAspnetUsersInRole>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_UsersInRoles");
            });

            modelBuilder.Entity<VwAspnetWebPartStatePath>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_WebPartState_Paths");

                entity.Property(e => e.LoweredPath).HasMaxLength(256);
                entity.Property(e => e.Path).HasMaxLength(256);
            });

            modelBuilder.Entity<VwAspnetWebPartStateShared>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_WebPartState_Shared");

                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwAspnetWebPartStateUser>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_aspnet_WebPartState_User");

                entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwMembershipUser>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("vw_MembershipUsers");

                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .HasColumnName("email");
                entity.Property(e => e.Firstname)
                    .HasMaxLength(50)
                    .HasColumnName("firstname");
                entity.Property(e => e.Lastname)
                    .HasMaxLength(50)
                    .HasColumnName("lastname");
                entity.Property(e => e.Password)
                    .HasMaxLength(128)
                    .HasColumnName("password");
                entity.Property(e => e.Username)
                    .HasMaxLength(256)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<ScormAiccSession>(entity =>
            {
                entity.ToTable("Scorm_aicc_session");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Userid).HasColumnName("userid");
                entity.Property(e => e.Scormid).HasColumnName("scormid");
                entity.Property(e => e.Hacpsession).HasColumnName("hacpsession");
                entity.Property(e => e.Scoid).HasColumnName("scoid");
                entity.Property(e => e.Scormmode).HasColumnName("scormmode");
                entity.Property(e => e.Scormstatus).HasColumnName("scormstatus");
                entity.Property(e => e.Attempt).HasColumnName("attempt");
                entity.Property(e => e.Lessonstatus).HasColumnName("lessonstatus");
                entity.Property(e => e.Sessiontime).HasColumnName("sessiontime");
                entity.Property(e => e.Timecreated).HasColumnName("timecreated");
                entity.Property(e => e.Timemodified).HasColumnName("timemodified");
            });

            modelBuilder.Entity<ScormScoesTrack>(entity =>
            {
                entity.ToTable("Scorm_scoes_track");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Userid).HasColumnName("userid");
                entity.Property(e => e.Scormid).HasColumnName("scormid");
                entity.Property(e => e.Scoid).HasColumnName("scoid");
                entity.Property(e => e.Attempt).HasColumnName("attempt");
                entity.Property(e => e.Element).HasColumnName("element");
                entity.Property(e => e.Value).HasColumnName("value");
                entity.Property(e => e.Timemodified).HasColumnName("timemodified");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

