using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Accounts.AddUserProfile
{
    public static class AddUserProfileContracts
    {
        public record AddUserProfileRequest(string FirstName,
                              string LastName,
                              string Email);

        public record AddUserProfileResponse(long Id,
                               string FirstName,
                               string LastName,
                               string Email,
                               bool IsEnabled);
    }


    public sealed class CreateUserValidator : AbstractValidator<AddUserProfileContracts.AddUserProfileRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .EmailAddress().WithMessage(_ => ModelTranslation.Accounts__UserProfile__InvalidEmail);
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .MaximumLength(100).WithMessage(_ => ModelTranslation.Global__Field__MaxLength);
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .MaximumLength(100).WithMessage(_ => ModelTranslation.Global__Field__MaxLength);
        }
    }
}
