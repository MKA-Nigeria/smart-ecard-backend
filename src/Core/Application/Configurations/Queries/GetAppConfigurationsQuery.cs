using Application.Common.Models;
using Application.Lookups.Dtos;
using Domain.Entities;
using Mapster;

namespace Application.Configurations.Dto;
public class GetAppConfigurationsQuery : PaginationFilter, IRequest<PaginationResponse<AppConfigurationDto>>;

public class GetAppConfigurationsQueryHandler(IRepository<AppConfiguration> configRepo) : IRequestHandler<GetAppConfigurationsQuery, PaginationResponse<AppConfigurationDto>>
{
    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    public async Task<PaginationResponse<AppConfigurationDto>> Handle(GetAppConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var appConfigurations = await _configRepo.ListAsync(cancellationToken);
        var appConfigurationResult = appConfigurations.Adapt<List<AppConfigurationDto>>();

        return new PaginationResponse<AppConfigurationDto>(appConfigurationResult, appConfigurationResult.Count, request.PageNumber, request.PageSize);
    }
}

