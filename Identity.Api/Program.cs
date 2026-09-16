using Identity.Api.Data;
using Identity.Api.Data.Entities;
using Identity.Api.Data.Repositories;
using Identity.Api.DomainComponents;
using Identity.Api.Middleware;
using Identity.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Services.AddDbContext<IdentityDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("BLTSMFT")));

builder.Services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
builder.Services.AddSingleton<IPassportProtector, PassportProtector>();
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IIdentityDomainComponent, IdentityDomainComponent>();
builder.Services.AddScoped<IAdministrationRepository, AdministrationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserDomainComponent, UserDomainComponent>();
builder.Services.AddScoped<IRoleDomainComponent, RoleDomainComponent>();
builder.Services.AddScoped<ISessionDomainComponent, SessionDomainComponent>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddPolicy("ui", p => p.WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:4200")
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials()));

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Issuer"]),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Audience"]),
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administration.View", policy => policy.RequireClaim("permission_access", "Administration.View:VIEW"));
    options.AddPolicy("Users.Manage", policy => policy.RequireClaim("permission_access", "Users.Manage:VIEW"));
    options.AddPolicy("Users.Create", policy => policy.RequireClaim("permission_access", "Users.Manage:CREATE"));
    options.AddPolicy("Users.Edit", policy => policy.RequireClaim("permission_access", "Users.Manage:EDIT"));
    options.AddPolicy("Users.Delete", policy => policy.RequireClaim("permission_access", "Users.Manage:DELETE"));
    options.AddPolicy("Users.Sensitive.View", policy => policy.RequireRole("Admin").RequireClaim("permission_access", "Users.Manage:VIEW"));
    options.AddPolicy("Roles.View", policy => policy.RequireClaim("permission_access", "Roles.Manage:VIEW"));
    options.AddPolicy("Roles.Edit", policy => policy.RequireClaim("permission_access", "Roles.Manage:EDIT"));
    options.AddPolicy("Sessions.View", policy => policy.RequireClaim("permission_access", "Sessions.Manage:VIEW"));
    options.AddPolicy("AuditLog.View", policy => policy.RequireClaim("permission_access", "AuditLog.View:VIEW"));
});
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("ui");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
