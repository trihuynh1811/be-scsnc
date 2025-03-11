using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Api.MIddleware;
using Api.Policies;
using Api.Services;
using Api.Swagger;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllers(opt =>
        {
            opt.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
        }).AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        //services.AddSwaggerGen(options =>
        //{
        //    // options.SwaggerDoc("v1", new OpenApiInfo()
        //    // {
        //    //     Version = "v1",
        //    //     Title = "SCSnC API",
        //    //     Description = "API for SCSnC application",
        //    // });
        //    //
        //    // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        //    // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        //});

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "API Documentation",
                Version = "v1.0",
                Description = ""
            });
            options.ResolveConflictingActions(x => x.First());
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                BearerFormat = "JWT",
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri($"https://{configuration["Auth0:Domain"]}/oauth/token"),
                        AuthorizationUrl = new Uri($"https://{configuration["Auth0:Domain"]}/authorize?audience={configuration["Auth0:ApiIdentifier"]}"),
                        Scopes = new Dictionary<string, string>
                  {
                      { "openid", "OpenId" },

                  }
                    }
                }
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
              },
              new[] { "openid" }
          }
      });


        });
    }
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }

    public static void AddJwtConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:issuer"],
                ValidAudience = configuration["Jwt:audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:secret"])),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context?.Request?.Cookies != null &&
                        context.Request.Cookies.ContainsKey("X-Access-Token"))
                    {
                        context.Token = context.Request.Cookies["X-Access-Token"];
                        Debug.WriteLine(context.Token ?? "No token found in cookie.");
                    }
                    else
                    {
                        Debug.WriteLine("No cookies or token found.");
                    }

                    return Task.CompletedTask;
                }
            };
        });
    }

    public static IApplicationBuilder UseSwaggerFeatures(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseSwagger();
        app.UseSwaggerUI(settings =>
        {
            settings.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0");
            settings.OAuthClientId(configuration["Auth0:ClientId"]);
            settings.OAuthClientSecret(configuration["Auth0:ClientSecret"]);
            settings.OAuthUsePkce();
        });
        return app;
    }
}