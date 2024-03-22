using Krialys.Orkestra.WebApi.Infrastructure.Validation;

namespace Krialys.Orkestra.WebApi.Services.Identity.Users.Password;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}

public class ForgotPasswordRequestValidator : CustomValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator(IStringLocalizer<ForgotPasswordRequestValidator> T) =>
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(T["Invalid Email Address."]);
}