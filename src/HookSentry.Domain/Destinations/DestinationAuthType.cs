namespace HookSentry.Domain.Destinations;

public enum DestinationAuthType
{
    ApiKey = 0,
    BearerToken = 1,
    JwtBearer = 2,
    BasicAuth = 3
}
