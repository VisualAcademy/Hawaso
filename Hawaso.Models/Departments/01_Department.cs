using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hawaso.Models.Departments
{
    [Table("Departments")]
    public class Department
    {
        public long Id { get; set; }
        public bool? Active { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string Name { get; set; }
    }
}
