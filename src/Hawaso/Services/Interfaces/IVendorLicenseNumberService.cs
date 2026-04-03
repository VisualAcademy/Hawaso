using System.Collections.Generic;

namespace Hawaso.Services.Interfaces
{
    public interface IVendorLicenseNumberService
    {
        string GetLicenseNumberSuggestion();
        List<string> GetRecentLicenseNumberSuggestions(int take = 5);
    }
}
