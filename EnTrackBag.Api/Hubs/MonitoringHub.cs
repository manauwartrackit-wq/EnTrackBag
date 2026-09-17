using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using EnTrackBag.Sessions;

namespace EnTrackBag.Api.Hubs;

[Authorize(Policy = "Dashboard")]
public class MonitoringHub : Hub
{
    private readonly SessionRepository _sessions;
    public MonitoringHub(SessionRepository sessions) { _sessions = sessions; }

    public override async Task OnConnectedAsync()
    {
        var context = Context;
        var sid = long.Parse(context.User!.FindFirstValue("session_id")!);
        var uid = int.Parse(context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // An established WebSocket doesn't pass through JWT validation again.
        // Check its session without treating keep-alives as user activity.
        _ = WatchSessionAsync(context, sid, uid);
        await base.OnConnectedAsync();
    }

    private async Task WatchSessionAsync(HubCallerContext context, long sid, int uid)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(context.ConnectionAborted))
                if (!await _sessions.ValidateAsync(sid, uid, context.ConnectionAborted))
                { context.Abort(); return; }
        }
        catch (OperationCanceledException) when (context.ConnectionAborted.IsCancellationRequested) { }
        catch { context.Abort(); } // Fail closed if session validation is unavailable.
    }
}
