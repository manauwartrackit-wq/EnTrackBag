using Identity.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Identity.Api.Data.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> e)
    {
        e.ToTable("Users", "dbo");
        e.HasKey(x => x.Id);
        e.Property(x => x.EmpCode).HasMaxLength(50);
        e.Property(x => x.UserName).HasMaxLength(256).IsRequired();
        e.Property(x => x.FirstName).HasMaxLength(100);
        e.Property(x => x.LastName).HasMaxLength(100);
        e.Property(x => x.DisplayName).HasMaxLength(256);
        e.Property(x => x.Email).HasMaxLength(256);
        e.Property(x => x.PassportNumberEncrypted).HasMaxLength(512);
        e.Property(x => x.PassportLast4).HasMaxLength(4);
        e.Property(x => x.Nationality).HasMaxLength(100);
        e.Property(x => x.Designation).HasMaxLength(150);
        e.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        e.HasIndex(x => x.UserName).IsUnique();
        e.HasIndex(x => x.EmpCode).IsUnique().HasFilter("[EmpCode] IS NOT NULL");
        e.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        e.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.NoAction);
        e.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UpdatedBy).OnDelete(DeleteBehavior.NoAction);
    }
}
