using Application.Configurations.Dto;
using Domain.Entities;
using Mapster;

namespace Application.Configurations.Queries;
public record GetAppConfigurationsQuery : IRequest<List<AppConfigurationDto>>;

public class GetAppConfigurationsQueryHandler(IRepository<AppConfiguration> ConfigRepo) : IRequestHandler<GetAppConfigurationsQuery, List<AppConfigurationDto>>
{
    public async Task<List<AppConfigurationDto>> Handle(GetAppConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var appConfigs = await ConfigRepo.ListAsync(cancellationToken);

        return appConfigs.Adapt<List<AppConfigurationDto>>();
    }
}

