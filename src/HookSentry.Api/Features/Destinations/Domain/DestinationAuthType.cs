namespace HookSentry.Api.Features.Destinations.Domain;

public enum DestinationAuthType
{
    ApiKey = 0,
    BearerToken = 1,
    JwtBearer = 2,
    BasicAuth = 3
}
