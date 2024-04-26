using Application.Common.Validation;
using FluentValidation;

namespace Application.Identity.Users;

public class UpdateUserRequestValidator : CustomValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(IUserService userService)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(25);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(25);

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage("Invalid Email Address.")
            .MustAsync(async (user, email, _) => !await userService.ExistsWithEmailAsync(email, user.Id))
                .WithMessage((_, email) => $"Email {email} is already registered.");
    }
}