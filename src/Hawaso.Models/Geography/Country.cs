namespace Azunt.Geography
{
    public class Country
    {
        // ISO 3166-1 alpha-2
        public string Code { get; set; } = null!; // "US", "CA", "KR"
        public string Name { get; set; } = null!;
        public string? FullName { get; set; }       // 정식 명칭(선택)
        public string? Alpha3 { get; set; }         // ISO 3166-1 alpha-3 (선택)
        public int? NumericCode { get; set; }       // 840, 124, 410...
    }
}
