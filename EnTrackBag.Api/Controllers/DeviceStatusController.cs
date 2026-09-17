using EnTrackBag.Api.DomainComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnTrackBag.Api.Controllers;

[ApiController]
[Route("api/device-status")]
[Authorize(Policy = "DeviceStatus")]
public class DeviceStatusController : ControllerBase
{
    private readonly IDeviceStatusDomainComponent _deviceStatusDomainComponent;

    public DeviceStatusController(IDeviceStatusDomainComponent deviceStatusDomainComponent)
    {
        _deviceStatusDomainComponent = deviceStatusDomainComponent;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) => Ok(await _deviceStatusDomainComponent.GetSummaryAsync(ct));

    [HttpGet("details")]
    public async Task<IActionResult> GetDetails([FromQuery] string? category, CancellationToken ct)
        => Ok(await _deviceStatusDomainComponent.GetDetailsAsync(category, ct));
}
