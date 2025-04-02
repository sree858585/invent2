using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class PeerUser
{
    public int PeerSysId { get; set; }

    public int UserSysId { get; set; }

    public DateTime? Dob { get; set; }

    public int? Gender { get; set; }

    public string? AgencyAffilation { get; set; }

    public string? SupvrLastName { get; set; }

    public string? SupvrFirstName { get; set; }

    public string? SupvrOrgName { get; set; }

    public string? SupvrContAddr1 { get; set; }

    public string? SupvrContAddr2 { get; set; }

    public string? SupvrContCity { get; set; }

    public string? SupvrContState { get; set; }

    public string? SupvrContZip { get; set; }

    public string? SupvrContPhone { get; set; }

    public string? SupvrContEmail { get; set; }

    public bool? CertHiv { get; set; }

    public DateTime? CertHivdate { get; set; }

    public bool? CertHcv { get; set; }

    public DateTime? CertHcvdate { get; set; }

    public bool? CertHr { get; set; }

    public DateTime? CertHrdate { get; set; }

    public int? ApplicantNumber { get; set; }

    public bool? ComplPracticum { get; set; }

    public bool? ComplPracticumMin { get; set; }

    public DateTime? PracticumBdate { get; set; }

    public DateTime? PracticumEdate { get; set; }

    public string? ExperienceCommitment { get; set; }

    public string? ExperienceChallenges { get; set; }

    public string? ExperienceWhy { get; set; }

    public bool RequiredCourses { get; set; }

    public string? UserExper { get; set; }

    public bool? Hsdiploma { get; set; }

    public int? ExamStatus { get; set; }

    public DateTime? DateCompletion { get; set; }

    public DateTime? DateCert { get; set; }

    public bool? Approve { get; set; }

    public DateTime? ApprovedDt { get; set; }

    public string? ApprovedBy { get; set; }

    public bool? Disapprove { get; set; }

    public string? ReasonDisapprv { get; set; }

    public bool DisapprvEmailSent { get; set; }

    public DateTime? DisapprovedDt { get; set; }

    public string? DisapprovedBy { get; set; }

    public string? Notes { get; set; }

    public DateTime? DiscardDt { get; set; }

    public DateTime? ReenterDt { get; set; }

    public DateTime? DateCreate { get; set; }

    public DateTime? DateModify { get; set; }

    public bool? Active { get; set; }

    public bool? SelfCare { get; set; }

    public bool CertPrep { get; set; }

    public DateTime? CertPrepDate { get; set; }

    public bool CertCriminalJustice { get; set; }

    public DateTime? CertCriminalJusticeDate { get; set; }
}
