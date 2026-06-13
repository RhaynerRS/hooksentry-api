using HookSentry.Api.Common.DTOs;

namespace HookSentry.Api.DataTransfer.ApiKeys.Requests;

public class GetApiKeysRequest : PaginationRequest
{
    public GetApiKeysRequest() { }

    public GetApiKeysRequest(int qt = 10, int pg = 1, string cpOrd = "id", SortOrder tpOrd = SortOrder.Desc)
        : base(qt, pg, cpOrd, tpOrd) { }

    public bool? IsActive { get; set; }
}
