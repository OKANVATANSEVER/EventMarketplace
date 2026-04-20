using MediatR;

namespace EventMarketplace.Application.Commands.Admin;

public sealed record CreateMemberCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role) : IRequest<(bool Success, string Error)>;
