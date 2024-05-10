using Domain.Entities;
using Newtonsoft.Json.Linq;
using Application.Common.Exceptions;
using Application.Configurations.Dto;

namespace Application.Configurations.Commands;
public record UpdateAppConfigurationsCommand(AppConfigurationDto AppConfigurations) : IRequest<DefaultIdType>;

public class UpdateAppConfigurationsRequestValidator : CustomValidator<UpdateAppConfigurationsCommand>
{
    public UpdateAppConfigurationsRequestValidator()
    {
        RuleFor(x => x.AppConfigurations)
            .NotEmpty().WithMessage("Invalid Value provided")
            .SetValidator(new AppConfigurationDtoValidator());
    }

}

public class UpdateAppConfigurationsCommandHandler(IRepository<AppConfiguration> configRepo) : IRequestHandler<UpdateAppConfigurationsCommand, DefaultIdType>
{
    private readonly IRepository<AppConfiguration> _configRepo = configRepo;

    public async Task<DefaultIdType> Handle(UpdateAppConfigurationsCommand request, CancellationToken cancellationToken)
    {
        var appConfig = await _configRepo.FirstOrDefaultAsync(x => x.Key == request.AppConfigurations.Key, cancellationToken);

        _ = appConfig ?? throw new NotFoundException($"Card request with Key {appConfig.Key} not found");
        appConfig.Update(request.AppConfigurations.Key, request.AppConfigurations.Value);

        await _configRepo.UpdateAsync(appConfig, cancellationToken);
        await _configRepo.SaveChangesAsync(cancellationToken);

        return appConfig.Id;
    }
}

public class AppConfigurationDtoValidator : AbstractValidator<AppConfigurationDto>
{
    public AppConfigurationDtoValidator()
    {
        RuleFor(x => x.Value).Custom((value, context) =>
        {
            // Check if the value is a valid JSON
            if (IsJson(value))
            {
                try
                {
                    // Attempt to parse the JSON
                    JToken.Parse(value);
                }
                catch (Exception)
                {
                    context.AddFailure("Value", "Invalid JSON format");
                }
            }

            // Check if the value is a valid URL
            else if (IsUrl(value))
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                {
                    context.AddFailure("Value", "Invalid URL format");
                }
            }

            // If it's not JSON or URL, then consider it as a normal string
        });
    }

    // Function to check if a string is a valid JSON
    private bool IsJson(string value)
    {
        try
        {
            JToken.Parse(value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Function to check if a string is a valid URL
    private bool IsUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}