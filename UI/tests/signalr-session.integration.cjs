// Opt-in live integration test. Uses the existing Administrator credential supplied
// through environment variables. Signing in replaces its current session.
const assert = require('node:assert/strict');
const { HubConnectionBuilder, LogLevel } = require('@microsoft/signalr');
const identity = process.env.IDENTITY_URL || 'http://localhost:5200';
const api = process.env.API_URL || 'http://localhost:5100';
async function login() {
  const response = await fetch(identity + '/api/auth/login', {method:'POST', headers:{'Content-Type':'application/json'},
    body:JSON.stringify({userName:process.env.TEST_USER,password:process.env.TEST_PASSWORD})});
  assert.equal(response.status,200,'Login status');
  return response.json();
}
async function history(session) {
  const response=await fetch(identity+'/api/administration/sessions',{headers:{Authorization:'Bearer '+session.accessToken}});
  assert.equal(response.status,200,'Session history status');return response.json();
}
(async()=>{
  assert.ok(process.env.TEST_USER && process.env.TEST_PASSWORD,'Supply TEST_USER and TEST_PASSWORD');
  let current=await login();
  const connection=new HubConnectionBuilder().withUrl(api+'/hubs/monitoring', {accessTokenFactory:()=>current.accessToken})
    .configureLogging(LogLevel.None).build();
  try {
    const before=await history(current);
    await connection.start();
    await new Promise(resolve=>setTimeout(resolve,16000));
    const after=await history(current);
    assert.equal(after.length,before.length,'SignalR must not create sessions');
    assert.equal(after.find(x=>x.sessionId===current.sessionId).lastActivityAt,
      before.find(x=>x.sessionId===current.sessionId).lastActivityAt,'Keep-alives must not update activity');
    console.log('PASS: SignalR connection and keep-alives neither create sessions nor renew activity');
    const closed=new Promise(resolve=>connection.onclose(resolve));
    current=await login();
    let timeout;
    try {
      await Promise.race([closed,new Promise((_,reject)=>{timeout=setTimeout(()=>reject(new Error('Revoked socket remained open')),10000)})]);
    } finally { clearTimeout(timeout); }
    console.log('PASS: Revoking session disconnects an established SignalR socket');
  } finally {
    await connection.stop();
    await fetch(identity+'/api/auth/logout',{method:'POST',headers:{Authorization:'Bearer '+current.accessToken}});
  }
})().catch(error=>{console.error(error.message);process.exitCode=1;});
