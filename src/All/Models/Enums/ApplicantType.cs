using System;
using System.ComponentModel;

namespace All.Models.Enums
{
    [Flags]
    public enum ApplicantType
    {
        None = 0,
        Vendor = 1,      // 0001
        Employee = 2,    // 0010
        Manager = 4,     // 0100
        Admin = 8        // 1000
    }
}
