using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hawaso.Web.Components.Pages.DivisionPages.Models
{
    public class DivisionModelConfiguration : IEntityTypeConfiguration<DivisionModel>
    {
        public void Configure(EntityTypeBuilder<DivisionModel> builder)
        {
            builder.ToTable("Divisions")
                   .HasKey(dm => dm.Id);

            builder.Property(dm => dm.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(dm => dm.CreatedAt)
                   .HasDefaultValueSql("GetDate()");

            builder.Property(dm => dm.Name)
                   .IsRequired()
                   .HasMaxLength(255);
        }
    }
}
