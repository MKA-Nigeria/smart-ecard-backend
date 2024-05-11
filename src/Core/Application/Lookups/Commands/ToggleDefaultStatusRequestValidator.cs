using Application.Lookups.Validators;
using Domain.Entities;

namespace Application.Lookups.Commands;

public class ToggleDefaultStatusRequestValidator : AbstractValidator<ToggleDefaultStatusCommand>
{
    public ToggleDefaultStatusRequestValidator(IReadRepository<Lookup> repository)
    {
        RuleFor(x => x.LookupId)
            .NotEmpty()
            .MustAsync(async (lookupId, _) =>
            {
                var card = await repository.FirstOrDefaultAsync(x => x.Id == lookupId, _);
                return card != null;
            }).WithMessage("Invalid lookup Id");

        RuleFor(p => p.IsDefault)
            .CustomAsync(async (isDefault, context, cancellationToken) =>
            {
                // Retrieve the current state of IsDefault from the database or some other source
                bool currentState = await GetCurrentIsDefaultStateFromDatabase(context.InstanceToValidate.LookupId, repository, cancellationToken);

                // Check if the current state matches the new value being set
                if (currentState == isDefault)
                {
                    context.AddFailure("IsDefault", "The new value must be different from the current value.");
                }
            });
    }

    private async Task<bool> GetCurrentIsDefaultStateFromDatabase(DefaultIdType lookupId, IReadRepository<Lookup> repository, CancellationToken cancellationToken)
    {
        var lookup = await repository.FirstOrDefaultAsync(x => x.Id == lookupId, cancellationToken);

        return lookup?.IsDefault ?? default;
    }
}
