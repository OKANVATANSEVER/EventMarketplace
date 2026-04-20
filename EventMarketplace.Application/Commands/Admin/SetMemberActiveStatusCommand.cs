using MediatR;

namespace EventMarketplace.Application.Commands.Admin;

public sealed record SetMemberActiveStatusCommand(string UserId, bool IsActive) : IRequest<bool>;
