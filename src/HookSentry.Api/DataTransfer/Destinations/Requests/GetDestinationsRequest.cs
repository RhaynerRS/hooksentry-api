using HookSentry.Api.Common.DTOs;

namespace HookSentry.Api.DataTransfer.Destinations.Requests;

public class GetDestinationsRequest : PaginationRequest
{
    public GetDestinationsRequest() { }

    public GetDestinationsRequest(int qt = 10, int pg = 1, string cpOrd = "id", SortOrder tpOrd = SortOrder.Desc)
        : base(qt, pg, cpOrd, tpOrd) { }
}
