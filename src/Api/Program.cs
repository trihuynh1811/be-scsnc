using Api;
using Api.Extensions;
using Api.Services;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationDbContext(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddServices();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", c =>
    {
        c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
        c.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudiences = builder.Configuration["Auth0:ApiIdentifier"].Split(";"),
            ValidateIssuer = true,
            ValidIssuer = $"https://{builder.Configuration["Auth0:Domain"]}"
        };
    });

    builder.Services.AddAuthorization(o =>
    {
        o.AddPolicy("read-data", policy =>
          policy.RequireClaim("permissions", "read:data"));
    });

    //builder.Services.AddJwtConfig(builder.Configuration);
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        IDBService dBService = new DBService();
        //dBService.CreateDropDb(builder.Configuration);
    }

    app.UseInfrastructure(builder.Configuration);
    app.UseSwaggerFeatures(builder.Configuration);
    app.UseAuthentication();
    app.UseAuthorization();

    //app.UseRenewToken();
    app.Run();
}
catch (Exception ex)
{
    // Handle an error related to .NET 6
    // https://github.com/dotnet/runtime/issues/60600
    var error = ex.GetType().Name;
    if (error.Equals("HostAbortedException", StringComparison.Ordinal))
    {
        throw;
    }
}