using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace EnTrackBag.Api.Data.Configurations;
public class DeviceLogViewEntityConfiguration : IEntityTypeConfiguration<DeviceLogViewEntity>
{ public void Configure(EntityTypeBuilder<DeviceLogViewEntity> e) { e.HasNoKey(); e.ToView("vwDeviceLog", "dbo"); } }
