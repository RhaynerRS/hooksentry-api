using HookSentry.Api.Common.DTOs;
using HookSentry.Domain.Invites;

namespace HookSentry.Api.DataTransfer.Invites.Requests;

public class GetInvitesRequest : PaginationRequest
{
    public InviteTokenStatus? Status { get; set; }
}
