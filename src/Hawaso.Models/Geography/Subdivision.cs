namespace Azunt.Geography
{
    public class Subdivision
    {
        // ISO 3166-2: CountryCode + Code가 유니크
        public string CountryCode { get; set; } // "US"
        public string Code { get; set; }         // "CA"(California) / "KR-11"(Seoul) 등 나라별 포맷 상이
        public string Name { get; set; }
        public string Type { get; set; } = "State";       // State/Province/Region/Prefecture/시·도...
        public int Level { get; set; } = 1;               // 계층 레벨 (1=광역, 2=기초 등)
        public string Abbrev { get; set; }               // "CA", "NY", "ON" 등의 관용 약어(있으면)
    }
}