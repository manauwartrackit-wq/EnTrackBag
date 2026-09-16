namespace EnTrackBag.Api.DTOs;

public sealed record BagJourneyThresholdDto(
    string Code,
    string From,
    string To,
    int NormalSeconds,
    int DelaySeconds);

public sealed record BagJourneyConfigurationDto(
    string? CurrentPrimaryServer,
    DateTime? LastUpdated,
    BagJourneyThresholdDto[] Thresholds);

public sealed record UpdateBagJourneyThresholdDto(string Code, int NormalSeconds, int DelaySeconds);
public sealed record UpdateBagJourneyConfigurationDto(UpdateBagJourneyThresholdDto[] Thresholds);

