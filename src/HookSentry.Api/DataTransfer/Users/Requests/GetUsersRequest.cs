using HookSentry.Api.Common.DTOs;
using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Api.DataTransfer.Users.Requests;

public class GetUsersRequest : PaginationRequest
{
    public GetUsersRequest() { }

    public GetUsersRequest(int qt = 10, int pg = 1, string cpOrd = "id", SortOrder tpOrd = SortOrder.Desc)
        : base(qt, pg, cpOrd, tpOrd) { }

    public UserStatus? Status { get; set; }
    public UserRole? Role { get; set; }
}
