namespace HookSentry.Api.Features.Events.Domain;

public enum EventStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    WaitingRetry = 4,
    CriticalFailure = 5,
    Cancelled = 6,
    AuthenticationFailed = 7
}
