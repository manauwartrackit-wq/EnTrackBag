using EnTrackBag.Api.Data.Entities;
namespace EnTrackBag.Api.Data.Repositories;
public interface IBagJourneyConfigurationRepository
{
    Task<SystemSettingEntity[]> GetSettingsAsync(string[] names, bool tracked, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

