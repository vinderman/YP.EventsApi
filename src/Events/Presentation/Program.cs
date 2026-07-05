using System.Text;
using Application.Services.EventService;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Presentation.Infrastructure;
using Presentation.Middleware;
using Shared.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPresentationServices();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationRepositories();

var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "JwtBearerScheme";
        options.DefaultChallengeScheme = "JwtBearerScheme";
    })
    .AddJwtBearer("JwtBearerScheme", options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
