using Microsoft.EntityFrameworkCore;

namespace Azunt.Geography
{
    [Owned]
    public class Address
    {
        public string Line1 { get; set; } = "";
        public string? Line2 { get; set; }
        public string City { get; set; } = "";                 // Locality
        public string PostalCode { get; set; } = "";
        public string CountryCode { get; set; } = "US";        // FK -> Country
        public string? SubdivisionCode { get; set; }           // FK part -> Subdivision
        public string? RegionFreeText { get; set; }            // Subdivision 미보유 국가 대비
    }
}
