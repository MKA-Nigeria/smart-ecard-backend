using Application.Lookups.Commands;
using Domain.Entities;

namespace Application.Lookups.Validators;

public class CreateLookupRequestValidator : CustomValidator<CreateLookupCommand>
{
    public CreateLookupRequestValidator(IReadRepository<Lookup> repository)
    {
        RuleFor(p => p.Type)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (type, _) => await repository.FirstOrDefaultAsync(c => c.Type == type, _) is null)
                .WithMessage((_, type) => $"{type} Lookup aleady exist");

        RuleFor(p => p.Value)
           .NotEmpty().WithMessage("Lookup value can not be empty");

        RuleFor(p => p.Name)
           .NotEmpty().WithMessage("Lookup name cannot be empty")
           .MustAsync(async (name, _) => await repository.FirstOrDefaultAsync(c => c.Name == name, _) is null)
               .WithMessage((_, name) => $"{name} Lookup name already exists");
    }
}
