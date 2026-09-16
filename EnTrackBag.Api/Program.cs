using EnTrackBag.Api.Authorization;
using EnTrackBag.Api.Data;
using EnTrackBag.Api.Data.Repositories;
using EnTrackBag.Api.DomainComponents;
using EnTrackBag.Api.Hubs;
using EnTrackBag.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Services.AddDbContext<BltsmftDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("BLTSMFT")));

builder.Services.AddScoped<IReaderRepository, ReaderRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<ISlaRepository, SlaRepository>();
builder.Services.AddScoped<IBagJourneyConfigurationRepository, BagJourneyConfigurationRepository>();
builder.Services.AddScoped<IDashboardDomainComponent, DashboardDomainComponent>();
builder.Services.AddScoped<ISlaDomainComponent, SlaDomainComponent>();
builder.Services.AddScoped<IDeviceStatusDomainComponent, DeviceStatusDomainComponent>();
builder.Services.AddScoped<IBagJourneyConfigurationDomainComponent, BagJourneyConfigurationDomainComponent>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("ui", p => p.WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:4200")
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials()));

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Issuer"]),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Audience"]),
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddSingleton<IAuthorizationHandler, AccessTypeAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    options.AddAccessPolicy("Dashboard.View", "Dashboard.View");
    options.AddAccessPolicy("Dashboard.SLA.View", "Dashboard.SLA.View");
    options.AddAccessPolicy("DeviceStatus.View", "DeviceStatus.View");
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("ui");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MonitoringHub>("/hubs/monitoring");
app.Run();
