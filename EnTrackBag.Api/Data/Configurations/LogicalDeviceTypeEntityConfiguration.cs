using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace EnTrackBag.Api.Data.Configurations;
public class LogicalDeviceTypeEntityConfiguration : IEntityTypeConfiguration<LogicalDeviceTypeEntity>
{ public void Configure(EntityTypeBuilder<LogicalDeviceTypeEntity> e) { e.ToTable("LogicalDeviceType", "dbo"); e.HasKey(x => x.DeviceType); e.Property(x => x.DeviceType).ValueGeneratedNever(); e.Property(x => x.DeviceDesc).HasMaxLength(50); } }
