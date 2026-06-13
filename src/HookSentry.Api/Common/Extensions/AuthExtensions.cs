using System.Text;
using HookSentry.Api.Common.Auth;
using HookSentry.Infrastructure.ApiKeys;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HookSentry.Api.Common.Extensions;

public static class AuthExtensions
{
    public const string ApiKeyScheme = "ApiKey";
    public const string RefreshKeyPrefix = "auth:refresh:";
    public static readonly TimeSpan RefreshTokenTtl = TimeSpan.FromDays(7);

    public static IServiceCollection AddJwtAndApiKeyAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IApiKeyCacheService, ApiKeyCacheService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyScheme, _ => { });

        services.AddAuthorization();

        return services;
    }
}
