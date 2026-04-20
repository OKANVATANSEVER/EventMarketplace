using EventMarketplace.Application.Commands.Admin;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Admin;

public sealed class SetMemberActiveStatusCommandHandler(IUserRepository userRepository)
    : IRequestHandler<SetMemberActiveStatusCommand, bool>
{
    public Task<bool> Handle(SetMemberActiveStatusCommand request, CancellationToken cancellationToken)
        => userRepository.SetActiveStatusAsync(request.UserId, request.IsActive, cancellationToken);
}
