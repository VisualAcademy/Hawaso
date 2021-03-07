using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
{
    [Table("Incidents")]
    public class Incident
    {
        public int Id { get; set; }

        [Display(Name = "Case #")]
        public string CaseNumber { get; set; }

        //public DailyLog DailyLogs { get; set; }
        public int? DailyLogId { get; set; }

        [Display(Name = "Daily Log #")]
        public string DailyLogNumber { get; set; }

        [Display(Name = "Open")]
        public DateTime? OpenedDate { get; set; }

        [Display(Name = "Occurred")]
        public DateTime? Occurred { get; set; }
        public DateTime? Closed { get; set; }

        public Property Properties { get; set; }
        public int? PropertyId { get; set; }
        public string Property { get; set; }

        public ReportType ReportTypes { get; set; }
        [Display(Name = "Incident Type")]
        public int? ReportTypeId { get; set; }

        public ReportSpecific ReportSpecific { get; set; }
        public int? ReportSpecificId { get; set; }
        public string Specific { get; set; }
        public int? SpecificId { get; set; }

        public CaseStatus CaseStatuses { get; set; }
        public int? CaseStatusId { get; set; }

        public Location Locations { get; set; }
        public int? LocationId { get; set; }
        public string Location { get; set; }

        public Sublocation Sublocations { get; set; }
        public int? SublocationId { get; set; }
        public string Sublocation { get; set; }

        public Department Departments { get; set; }
        public int? DepartmentId { get; set; }
        public string Department { get; set; }

        public string Summary { get; set; }
        public string Details { get; set; }
        public string Resolution { get; set; }
        public string Reference { get; set; }
        public int? SecondaryOperatorId { get; set; }
        public string SecondaryOperator { get; set; }

        public bool Custodial { get; set; }

        [Display(Name = "Use of Force")]
        public bool UseForce { get; set; }

        public bool Medical { get; set; }

        [Display(Name = "Risk Management")]
        public bool RiskManagement { get; set; }

        public bool Active { get; set; }
        public string Priority { get; set; }

        public string AgentName { get; set; }
        public string SupervisorName { get; set; }
        public string ManagerName { get; set; }
        public string DirectorName { get; set; }

        public string AgentSignature { get; set; }
        public string SupervisorSignature { get; set; }
        public string ManagerSignature { get; set; }
        public string DirectorSignature { get; set; }

        public DateTime? AgentTime { get; set; }
        public DateTime? SupervisorTime { get; set; }
        public DateTime? ManagerTime { get; set; }
        public DateTime? DirectorTime { get; set; }


        public int? CaseTypeId { get; set; }
        public int? InvestigatorId { get; set; }
        public int? ShiftId { get; set; }
        public int? ImmediateSupervisorId { get; set; }
        public int? GamingClassId { get; set; }
        public int? SurveillanceNotified { get; set; }
        public int? SurveillanceObserverId { get; set; }
        public string InitialContact { get; set; }
        public string InspectorSig { get; set; }
        public string DeputyDirectorSig { get; set; }
        public string DirectorSig { get; set; }
        public string SupervisorSig { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? TgaforwardDate { get; set; }
        public DateTime? TgoreturnDate { get; set; }
        public string Citation { get; set; }
        public string ViolationNature { get; set; }
        public decimal? Variance { get; set; }
        public bool? Employee { get; set; }
        public int? ManagerId { get; set; }
        public string TapeIdentification { get; set; }
        public string ActionTaken { get; set; }
        public string SuspectPhoto { get; set; }
        public string ExclusionInfo { get; set; }
        public string Notification { get; set; }
        public bool? PoliceContacted { get; set; }
        public string PoliceContact { get; set; }


        public DateTime? InvestigatorSigTs { get; set; }
        public DateTime? SupervisorSigTs { get; set; }
        public DateTime? DeputyDirectorSigTs { get; set; }
        public DateTime? DirectorSigTs { get; set; }
        public string Tgoresponse { get; set; }
        public string CreatedBy { get; set; }
        public string ClosedBy { get; set; }





        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? DispatchCallId { get; set; }


        public int? AuditId { get; set; }
        public bool? SavingsOrLosses { get; set; }
        public bool? DirectorOnly { get; set; }
        public string CaseType { get; set; }
        public string Status { get; set; }
        public string Agent { get; set; }
        public string AgentSigFile { get; set; }
        public string SupervisorSigFile { get; set; }
        public string ManagerSigFile { get; set; }
        public string DirectorSigFile { get; set; }
        public byte[] AgentImage { get; set; }
        public byte[] SupervisorImage { get; set; }
        public byte[] ManagerImage { get; set; }
        public byte[] DirectorImage { get; set; }

        public string RemarksTitle1 { get; set; }
        public string RemarksMemos1 { get; set; }
        public string RemarksTitle2 { get; set; }
        public string RemarksMemos2 { get; set; }
        public string RemarksTitle3 { get; set; }
        public string RemarksMemos3 { get; set; }
        public string RemarksTitle4 { get; set; }
        public string RemarksMemos4 { get; set; }
        public string RemarksTitle5 { get; set; }
        public string RemarksMemos5 { get; set; }


    }
}
