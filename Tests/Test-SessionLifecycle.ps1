param(
    [Parameter(Mandatory)][PSCredential]$Credential,
    [string]$IdentityUrl = 'http://localhost:5200',
    [string]$ApiUrl = 'http://localhost:5100'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$configuration = Get-Content (Join-Path $root 'Identity.Api/appsettings.json') -Raw | ConvertFrom-Json
$db = [System.Data.SqlClient.SqlConnection]::new($configuration.ConnectionStrings.BLTSMFT)
$db.Open()
$body = @{ userName=$Credential.UserName; password=$Credential.GetNetworkCredential().Password } | ConvertTo-Json
$created = [System.Collections.Generic.List[long]]::new()
$passed = 0
function Check($condition, $name) {
    if (-not $condition) { throw "FAIL: $name" }
    $script:passed++
    Write-Output "PASS: $name"
}
function Sql($sql, [long]$id=0) {
    $command=$db.CreateCommand()
    $command.CommandText=$sql
    [void]$command.Parameters.AddWithValue('@sid',$id)
    try { return $command.ExecuteScalar() } finally { $command.Dispose() }
}
function Login {
    $result=Invoke-RestMethod "$IdentityUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body $body
    $created.Add([long]$result.sessionId)
    return $result
}
function Request($url, $login, $method='GET', $activity=$false) {
    $headers=@{Authorization="Bearer $($login.accessToken)"}
    if($activity){$headers['X-User-Activity']='1'}
    return Invoke-WebRequest $url -Method $method -Headers $headers -SkipHttpErrorCheck
}
try {
    $initialCount=[int](Sql 'SELECT COUNT(*) FROM dbo.UserSessions')
    $first=Login
    $second=Login
    Check ((Sql "SELECT LogoutReason FROM dbo.UserSessions WHERE SessionId=@sid" $first.sessionId) -eq 'Revoked') 'Repeat login revokes previous session'
    Check ((Request "$IdentityUrl/api/auth/session" $first).StatusCode -eq 401) 'Identity rejects revoked JWT'
    Check ((Request "$ApiUrl/api/bag-journey-configuration" $first).StatusCode -eq 401) 'Operational API rejects revoked JWT'
    Check ((Request "$IdentityUrl/api/auth/session" $second).StatusCode -eq 204) 'Current session accepted'
    Check ((Request "$ApiUrl/api/bag-journey-configuration" $second).StatusCode -eq 200) 'Operational API accepts current session'

    $before=Sql 'SELECT LastActivityAt FROM dbo.UserSessions WHERE SessionId=@sid' $second.sessionId
    1..3 | ForEach-Object { [void](Request "$IdentityUrl/api/auth/session" $second); [void](Request "$ApiUrl/api/bag-journey-configuration" $second) }
    $after=Sql 'SELECT LastActivityAt FROM dbo.UserSessions WHERE SessionId=@sid' $second.sessionId
    Check ($before -eq $after) 'Background API calls do not extend activity'
    Check ((Sql 'SELECT COUNT(*) FROM dbo.UserSessions') -eq ($initialCount+2)) 'Ordinary API calls do not create sessions'
    [void](Sql 'UPDATE dbo.UserSessions SET LastActivityAt=DATEADD(second,-120,SYSUTCDATETIME()) WHERE SessionId=@sid' $second.sessionId)
    Check ((Request "$ApiUrl/api/bag-journey-configuration" $second 'GET' $true).StatusCode -eq 200) 'Foreground API activity accepted'
    Check ((Sql 'SELECT DATEDIFF(second,LastActivityAt,SYSUTCDATETIME()) FROM dbo.UserSessions WHERE SessionId=@sid' $second.sessionId) -lt 5) 'Foreground API updates last activity'
    Check ((Request "$IdentityUrl/api/auth/activity" $second 'POST' $true).StatusCode -eq 204) 'Interaction/navigation activity accepted'
    Check ((Request "$IdentityUrl/api/auth/logout" $second 'POST').StatusCode -eq 204) 'Logout endpoint succeeds'
    Check ((Sql 'SELECT LogoutReason FROM dbo.UserSessions WHERE SessionId=@sid' $second.sessionId) -eq 'Logged out') 'Logout recorded in history'
    Check ((Request "$IdentityUrl/api/auth/session" $second).StatusCode -eq 401) 'Logged-out JWT rejected'

    $expired=Login
    # Time travel ONLY the session created by this test, not existing session history.
    [void](Sql 'UPDATE dbo.UserSessions SET LastActivityAt=DATEADD(second,-300,SYSUTCDATETIME()) WHERE SessionId=@sid' $expired.sessionId)
    Check ((Request "$ApiUrl/api/bag-journey-configuration" $expired 'GET' $true).StatusCode -eq 401) 'Operational API rejects exact five-minute inactivity boundary'
    Check ((Request "$IdentityUrl/api/auth/activity" $expired 'POST' $true).StatusCode -eq 401) 'Activity cannot revive expired session'
    Check ((Sql 'SELECT LogoutReason FROM dbo.UserSessions WHERE SessionId=@sid' $expired.sessionId) -eq 'Expired') 'Idle expiration recorded'

    $abandoned=Login
    [void](Sql 'UPDATE dbo.UserSessions SET LastActivityAt=DATEADD(second,-301,SYSUTCDATETIME()) WHERE SessionId=@sid' $abandoned.sessionId)
    Start-Sleep -Seconds 12
    Check ((Sql 'SELECT IsActive FROM dbo.UserSessions WHERE SessionId=@sid' $abandoned.sessionId) -eq $false) 'Server sweep expires abandoned sessions without requests'

    $http=[System.Net.Http.HttpClient]::new()
    try {
        $tasks=1..4 | ForEach-Object { $http.PostAsync("$IdentityUrl/api/auth/login",[System.Net.Http.StringContent]::new($body,[System.Text.Encoding]::UTF8,'application/json')) }
        $parallel=@()
        foreach($task in $tasks){
            $response=$task.GetAwaiter().GetResult()
            Check ($response.IsSuccessStatusCode) 'Concurrent login succeeds'
            $login=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
            $created.Add([long]$login.sessionId)
            $parallel += $login
        }
        $accepted=@($parallel | Where-Object { (Request "$IdentityUrl/api/auth/session" $_).StatusCode -eq 204 })
        Check ($accepted.Count -eq 1) 'Exactly one concurrent login remains valid'
        $current=$accepted[0]
        Check ((Sql 'SELECT COUNT(*) FROM dbo.UserSessions WHERE UserId=(SELECT UserId FROM dbo.UserSessions WHERE SessionId=@sid) AND IsActive=1' $current.sessionId) -eq 1) 'Database contains one active session per user'
        $count=(Request "$IdentityUrl/api/administration/sessions/active-count" $current).Content | ConvertFrom-Json
        $expected=Sql 'SELECT COUNT(*) FROM dbo.UserSessions s JOIN dbo.Users u ON u.Id=s.UserId WHERE s.IsActive=1 AND s.LogoutAt IS NULL AND s.TokenExpiresAt>SYSUTCDATETIME() AND s.LastActivityAt>DATEADD(minute,-5,SYSUTCDATETIME()) AND u.IsActive=1'
        Check ($count -eq $expected) 'Administration count matches full live-session query'
        $history=(Request "$IdentityUrl/api/administration/sessions?take=100" $current).Content | ConvertFrom-Json
        Check (@($history | Where-Object status -eq 'Revoked').Count -gt 0) 'History shows Revoked'
        Check (@($history | Where-Object status -eq 'Expired').Count -gt 0) 'History shows Expired'
        Check (@($history | Where-Object status -eq 'Logged out').Count -gt 0) 'History shows Logged out'
        Check ((Sql 'SELECT COUNT(*) FROM dbo.UserSessions') -eq ($initialCount+$created.Count)) 'All original history rows preserved'
        [void](Request "$IdentityUrl/api/auth/logout" $current 'POST')
    } finally { $http.Dispose() }
    Write-Output "Passed $passed integration assertions. Test sessions retained as history."
} finally {
    # Close only still-active sessions generated by this run; never delete history.
    foreach($id in $created){ [void](Sql "UPDATE dbo.UserSessions SET IsActive=0,LogoutAt=SYSUTCDATETIME(),LogoutReason='Logged out' WHERE SessionId=@sid AND IsActive=1" $id) }
    $db.Dispose()
}
