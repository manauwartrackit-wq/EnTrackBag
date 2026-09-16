using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace EnTrackBag.Api.Data.Configurations;
public class TagViewEntityConfiguration : IEntityTypeConfiguration<TagViewEntity>
{ public void Configure(EntityTypeBuilder<TagViewEntity> e) { e.HasNoKey(); e.ToView("vwTags", "dbo"); } }
