using EnTrackBag.Api.Data.Entities;
using EnTrackBag.Api.Data.Repositories;
using EnTrackBag.Api.DTOs;
namespace EnTrackBag.Api.DomainComponents;

public sealed class BagJourneyConfigurationDomainComponent : IBagJourneyConfigurationDomainComponent
{
    private sealed record Definition(string Code, string From, string To, string NormalSetting, string DelaySetting);
    private static readonly Definition[] Definitions =
    [
        new("tag-read-point", "Tagging Station", "Tagging Read Point", "TimeTagToReadPoint", "TimeTagToReadPoint_Delay"),
        new("read-point-dog-air", "Tagging Read Point", "Dog House Airside", "TimeRPToDGAir", "TimeRPToDGAir_Delay"),
        new("dog-air-land", "Dog House Airside", "Dog House Landside", "TimeDGAirToLand", "TimeDGAirToLand_Delay"),
        new("dog-land-exit", "Dog House Landside", "Exit Gate", "TimeDGLandToExit", "TimeDGLandToExit_Delay"),
        new("return-feed-dog-air", "BHS Return Feed", "Dog House Airside", "TimeRFToDGAir", "TimeRFToDGAir_Delay"),
        new("lounge-in-out", "Inside Lounge", "Exit Lounge", "TimeLInToLOut", "TimeLInToLOut_Delay"),
        new("lounge-out-exit", "Exit Lounge", "Exit Gate", "TimeLOutToExit", "TimeLOutToExit_Delay"),
        new("exit-recheck", "Exit Gate", "Recheck Station", "TimeExitToRecheck", "TimeExitToRecheck_Delay")
    ];
    private static readonly string[] SettingNames = Definitions
        .SelectMany(x => new[] { x.NormalSetting, x.DelaySetting }).Append("CurrentPrimaryIP").Distinct().ToArray();
    private readonly IBagJourneyConfigurationRepository _repository;
    public BagJourneyConfigurationDomainComponent(IBagJourneyConfigurationRepository repository) { _repository = repository; }

    public async Task<BagJourneyConfigurationDto> GetAsync(CancellationToken ct) =>
        Map(await _repository.GetSettingsAsync(SettingNames, false, ct));

    public async Task<BagJourneyConfigurationDto> UpdateAsync(UpdateBagJourneyConfigurationDto request, CancellationToken ct)
    {
        var requested = request.Thresholds.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        if (requested.Keys.Except(Definitions.Select(x => x.Code), StringComparer.OrdinalIgnoreCase).Any())
            throw new ArgumentException("One or more journey threshold codes are invalid.");
        if (requested.Values.Any(x => x.NormalSeconds < 1 || x.DelaySeconds < 1 || x.NormalSeconds > 86400 || x.DelaySeconds > 86400))
            throw new ArgumentException("Thresholds must be between 1 and 86400 seconds.");

        var settings = await _repository.GetSettingsAsync(SettingNames, true, ct);
        var byName = settings.Where(x => x.SettingName != null).ToDictionary(x => x.SettingName!, StringComparer.OrdinalIgnoreCase);
        foreach (var definition in Definitions)
        {
            if (!requested.TryGetValue(definition.Code, out var update)) continue;
            if (!byName.TryGetValue(definition.NormalSetting, out var normal) || !byName.TryGetValue(definition.DelaySetting, out var delay))
                throw new InvalidOperationException($"Required MFT settings for {definition.From} to {definition.To} are missing.");
            normal.SettingValue = update.NormalSeconds.ToString(); normal.LastChanged = DateTime.Now;
            delay.SettingValue = update.DelaySeconds.ToString(); delay.LastChanged = DateTime.Now;
        }
        await _repository.SaveChangesAsync(ct);
        return Map(settings);
    }

    private static BagJourneyConfigurationDto Map(SystemSettingEntity[] settings)
    {
        var byName = settings.Where(x => x.SettingName != null).ToDictionary(x => x.SettingName!, StringComparer.OrdinalIgnoreCase);
        int Value(string name) => byName.TryGetValue(name, out var item) && int.TryParse(item.SettingValue, out var value) ? value : 0;
        var thresholds = Definitions.Select(x => new BagJourneyThresholdDto(x.Code, x.From, x.To, Value(x.NormalSetting), Value(x.DelaySetting))).ToArray();
        byName.TryGetValue("CurrentPrimaryIP", out var primary);
        return new BagJourneyConfigurationDto(primary?.SettingValue, primary?.LastChanged, thresholds);
    }
}

