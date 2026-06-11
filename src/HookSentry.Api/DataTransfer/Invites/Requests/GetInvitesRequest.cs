using HookSentry.Api.Common.DTOs;
using HookSentry.Api.Features.Invites.Domain;

namespace HookSentry.Api.DataTransfer.Invites.Requests;

public class GetInvitesRequest : PaginationRequest
{
    public InviteTokenStatus? Status { get; set; }
}
