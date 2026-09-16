using EnTrackBag.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
namespace EnTrackBag.Api.Data.Repositories;
public sealed class BagJourneyConfigurationRepository : IBagJourneyConfigurationRepository
{
    private readonly BltsmftDbContext _db;
    public BagJourneyConfigurationRepository(BltsmftDbContext db) { _db = db; }
    public Task<SystemSettingEntity[]> GetSettingsAsync(string[] names, bool tracked, CancellationToken ct)
    {
        IQueryable<SystemSettingEntity> query = _db.SystemSettings.Where(x => x.SettingName != null && names.Contains(x.SettingName));
        if (!tracked) query = query.AsNoTracking();
        return query.ToArrayAsync(ct);
    }
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}

