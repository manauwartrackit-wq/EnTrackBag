using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
namespace EnTrackBag.Api.Data;
public class BltsmftDbContext : DbContext
{
    public BltsmftDbContext(DbContextOptions<BltsmftDbContext> options) : base(options) { }
    public DbSet<ReaderEntity> Readers => Set<ReaderEntity>(); public DbSet<AntennaEntity> Antennas=>Set<AntennaEntity>(); public DbSet<ControllerEntity> Controllers=>Set<ControllerEntity>(); public DbSet<LogicalDeviceEntity> LogicalDevices=>Set<LogicalDeviceEntity>(); public DbSet<LogicalDeviceTypeEntity> LogicalDeviceTypes=>Set<LogicalDeviceTypeEntity>(); public DbSet<LogicalDeviceMapEntity> LogicalDeviceMaps=>Set<LogicalDeviceMapEntity>(); public DbSet<ReaderControllerMapEntity> ReaderControllerMaps=>Set<ReaderControllerMapEntity>(); public DbSet<ServerListEntity> Servers=>Set<ServerListEntity>(); public DbSet<SuspectBagEntity> SuspectBags=>Set<SuspectBagEntity>(); public DbSet<AlarmListEntity> AlarmList=>Set<AlarmListEntity>(); public DbSet<DeviceLogViewEntity> DeviceLog=>Set<DeviceLogViewEntity>(); public DbSet<TagViewEntity> Tags=>Set<TagViewEntity>(); public DbSet<SystemSettingEntity> SystemSettings=>Set<SystemSettingEntity>(); public DbSet<EnTrackBagExceptionEntity> EnTrackBagExceptions=>Set<EnTrackBagExceptionEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder){ base.OnModelCreating(modelBuilder); modelBuilder.ApplyConfigurationsFromAssembly(typeof(BltsmftDbContext).Assembly); }
}
