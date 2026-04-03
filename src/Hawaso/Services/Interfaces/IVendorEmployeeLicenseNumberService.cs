using System.Collections.Generic;

namespace Hawaso.Services.Interfaces
{
    public interface IVendorEmployeeLicenseNumberService
    {
        string GetLicenseNumberSuggestion();
        List<string> GetRecentLicenseNumberSuggestions(int take = 5);
    }
}