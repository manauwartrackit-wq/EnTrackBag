using EnTrackBag.Api.DTOs;
namespace EnTrackBag.Api.DomainComponents;
public interface IBagJourneyConfigurationDomainComponent
{
    Task<BagJourneyConfigurationDto> GetAsync(CancellationToken ct);
    Task<BagJourneyConfigurationDto> UpdateAsync(UpdateBagJourneyConfigurationDto request, CancellationToken ct);
}

