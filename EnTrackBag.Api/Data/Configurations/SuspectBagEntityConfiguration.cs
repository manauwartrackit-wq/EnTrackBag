using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace EnTrackBag.Api.Data.Configurations;
public class SuspectBagEntityConfiguration : IEntityTypeConfiguration<SuspectBagEntity>
{
    public void Configure(EntityTypeBuilder<SuspectBagEntity> e)
    {
        e.ToTable("SuspectBags", "dbo");
        e.HasKey(x => x.ID);
        e.Property(x => x.TagID).HasMaxLength(50).IsRequired();
        e.Property(x => x.GlobalID).HasMaxLength(20);
        e.Property(x => x.ScanResult).HasMaxLength(10);
        e.Property(x => x.IATACode).HasMaxLength(50);
    }
}
