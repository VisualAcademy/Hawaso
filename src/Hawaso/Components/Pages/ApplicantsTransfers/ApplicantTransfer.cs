using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisualAcademy.Components.Pages.ApplicantsTransfers;

public class ApplicantTransfer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    public string? TenantName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? BirthCity { get; set; }
    public string? BirthCountry { get; set; }

    [StringLength(2)]
    public string? BirthState { get; set; }
    public string? City { get; set; }
    public string? DOB { get; set; }

    [StringLength(254)]
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string? CellPhone { get; set; }
    public string? HomePhone { get; set; }
    public string? WorkPhone { get; set; }

    public DateTime? LicenseDate { get; set; }
    public DateTime? OriginalLicenseDate { get; set; }
    public DateTime? LicenseExpirationDate { get; set; }
    public DateTime? LicenseRenewalDate { get; set; }
    public long? LicenseTypeID { get; set; }
    public string? PostalCode { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }

    [StringLength(2)]
    public string? State { get; set; }

    public DateTime? RehireDate { get; set; }
    public string? SSN { get; set; }
    public DateTime? TerminationDate { get; set; }
    public long? PositionID { get; set; }
    public string? DriverLicenseNumber { get; set; }

    [StringLength(2)]
    public string? DriverLicenseState { get; set; }

    public long? EmploymentStatusID { get; set; }
    public string? LicenseNumber { get; set; }
    public long? LicenseStatusID { get; set; }
    public long? ProcessStatusID { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Photo { get; set; }
    public long? DepartmentID { get; set; }
    public string? NigcClassification { get; set; }
    public string? StateClassification { get; set; }

    public string? BadgePhotoVarBinary { get; set; }
    public string? BadgePhotoImage { get; set; }
    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Custom3 { get; set; }
    public string? Custom4 { get; set; }
    public string? Custom5 { get; set; }
    public string? Custom6 { get; set; }

    public bool? Confidential { get; set; }
    public bool? Group1 { get; set; }
    public bool? Group2 { get; set; }
    public bool? Group3 { get; set; }
    public byte? InvestigationTypeID { get; set; }
    public string? InvestigationType { get; set; }
    public string? BadgeNumber { get; set; }
    public string? Location { get; set; }
    public string? EyeColor { get; set; }
    public string? TribalMembershipInfo { get; set; }
    public string? BadgingAuthorizationNumber { get; set; }

    public DateTime? BadgeDate { get; set; }

    [StringLength(50)]
    public string? Citizenship { get; set; }

    [StringLength(50)]
    public string? Ethnicity { get; set; }

    public string? SRCRRelatives { get; set; }

    [StringLength(100)]
    public string? HighestEducationCompleted { get; set; }

    public string? FormData { get; set; }
    public string? DisplayName { get; set; }

    public string? DepartmentName { get; set; }
    public string? EmployeeId { get; set; }
    public string? Address2 { get; set; }
    public string? ZipCode { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? EmploymentStatus { get; set; }
    public string? DateSeniority { get; set; }
    public string? DefaultJobsHR { get; set; }
    public string? EmployeeStatus { get; set; }
    public string? TribalNation { get; set; }
    public string? GamingLicenseType { get; set; }
    public string? DateRehired { get; set; }
    public string? DateTerminated { get; set; }
}
