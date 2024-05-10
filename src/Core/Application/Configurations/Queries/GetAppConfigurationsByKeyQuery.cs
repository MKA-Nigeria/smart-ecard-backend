using Application.Configurations.Dto;
using Domain.Entities;
using Mapster;

namespace Application.Configurations.Queries;

public record GetAppConfigurationsByKeyQuery(string Key) : IRequest<AppConfigurationDto>;

public class GetAppConfigurationsByKeyQueryHandler(IRepository<AppConfiguration> configRepo) : IRequestHandler<GetAppConfigurationsByKeyQuery, AppConfigurationDto>
{
    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    public async Task<AppConfigurationDto> Handle(GetAppConfigurationsByKeyQuery request, CancellationToken cancellationToken)
    {
        var appConfig = await _configRepo.FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken);

        return appConfig.Adapt<AppConfigurationDto>();
    }
}
