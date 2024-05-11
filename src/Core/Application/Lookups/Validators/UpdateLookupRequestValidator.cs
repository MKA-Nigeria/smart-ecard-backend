using Application.Lookups.Commands;
using Domain.Entities;

namespace Application.Lookups.Validators;

public class UpdateLookupRequestValidator : CustomValidator<UpdateLookupCommand>
{
    public UpdateLookupRequestValidator(IReadRepository<Lookup> repository)
    {
        RuleFor(x => x.LookupId).NotEmpty().MustAsync(async (lookupId, _) =>
        await repository.FirstOrDefaultAsync(x => x.Id == lookupId, _) is Lookup card).WithMessage("Invalid lookup Id");

        RuleFor(p => p.Value)
           .NotEmpty().WithMessage("Lookup value can not be empty");
    }
}
