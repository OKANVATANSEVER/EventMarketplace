using EventMarketplace.Application.Commands.Admin;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Admin;

public sealed class CreateMemberCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateMemberCommand, (bool Success, string Error)>
{
    public Task<(bool Success, string Error)> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
        => userRepository.CreateMemberAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Role,
            cancellationToken);
}
