using HookSentry.Api.Common.DTOs;

namespace HookSentry.Api.DataTransfer.Events.Requests;

public class GetEventsRequest : PaginationRequest
{
    public GetEventsRequest() { }

    public GetEventsRequest(int qt = 10, int pg = 1, string cpOrd = "acceptedAt", SortOrder tpOrd = SortOrder.Desc)
        : base(qt, pg, cpOrd, tpOrd) { }

    public string? Status { get; set; }
    public Guid? DestinationUrlId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}
