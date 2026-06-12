using HookSentry.Api.Common.DTOs;

namespace HookSentry.Api.DataTransfer.Senders.Requests;

public class GetSendersRequest : PaginationRequest
{
    public GetSendersRequest() { }

    public GetSendersRequest(int qt = 10, int pg = 1, string cpOrd = "id", SortOrder tpOrd = SortOrder.Desc)
        : base(qt, pg, cpOrd, tpOrd) { }
}
