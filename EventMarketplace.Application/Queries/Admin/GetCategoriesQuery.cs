using EventMarketplace.Application.DTOs;
using MediatR;

namespace EventMarketplace.Application.Queries.Admin;

public sealed record GetCategoriesQuery : IRequest<IList<CategoryDto>>;
