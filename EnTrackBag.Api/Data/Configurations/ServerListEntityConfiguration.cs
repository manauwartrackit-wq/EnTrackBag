using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace EnTrackBag.Api.Data.Configurations;
public class ServerListEntityConfiguration : IEntityTypeConfiguration<ServerListEntity>
{ public void Configure(EntityTypeBuilder<ServerListEntity> e) { e.ToTable("ServerList", "dbo"); e.HasKey(x => x.ID); e.Property(x => x.ServerIP).HasMaxLength(50); } }
