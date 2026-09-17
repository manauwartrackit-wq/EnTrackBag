using EnTrackBag.Api.DomainComponents;
using EnTrackBag.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnTrackBag.Api.Controllers;
[ApiController]
[Route("api/bag-journey-configuration")]
[Authorize(Policy = "BagJourney.Configuration")]
public sealed class BagJourneyConfigurationController : ControllerBase
{
    private readonly IBagJourneyConfigurationDomainComponent _domainComponent;
    public BagJourneyConfigurationController(IBagJourneyConfigurationDomainComponent domainComponent) { _domainComponent = domainComponent; }
    [HttpGet]
    public async Task<ActionResult<BagJourneyConfigurationDto>> Get(CancellationToken ct) => Ok(await _domainComponent.GetAsync(ct));
    [HttpPut]
    [Authorize(Policy = "BagJourney.Configuration:EDIT")]
    public async Task<ActionResult<BagJourneyConfigurationDto>> Update(UpdateBagJourneyConfigurationDto request, CancellationToken ct)
    {
        try { return Ok(await _domainComponent.UpdateAsync(request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
