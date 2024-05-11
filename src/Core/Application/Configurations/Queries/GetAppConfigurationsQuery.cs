using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Models;
using Application.Configurations.Dto;
using Domain.Entities;
using Mapster;

namespace Application.Configurations.Queries;
public class GetAppConfigurationsQuery : PaginationFilter, IRequest<PaginationResponse<AppConfigurationDto>>;

public class GetAppConfigurationsQueryHandler(IRepository<AppConfiguration> ConfigRepo) : IRequestHandler<GetAppConfigurationsQuery, PaginationResponse<AppConfigurationDto>>
{
    public async Task<PaginationResponse<AppConfigurationDto>> Handle(GetAppConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var appConfigurations = await ConfigRepo.ListAsync(cancellationToken);
        var appConfigurationResult = appConfigurations.Adapt<List<AppConfigurationDto>>();

        return new PaginationResponse<AppConfigurationDto>(appConfigurationResult, appConfigurationResult.Count, request.PageNumber, request.PageSize);
    }
}

