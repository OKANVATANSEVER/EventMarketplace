using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Queries.Admin;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Admin;

public sealed class GetCategoriesQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetCategoriesQuery, IList<CategoryDto>>
{
    public Task<IList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        => eventRepository.GetCategoriesAsync(cancellationToken);
}
