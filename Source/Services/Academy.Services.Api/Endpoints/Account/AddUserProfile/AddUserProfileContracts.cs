using Academy.Shared.Localisation;

using FluentValidation;

using static Academy.Services.Api.Endpoints.Account.AddUserProfile.AddUserProfileContracts;

namespace Academy.Services.Api.Endpoints.Account.AddUserProfile
{
    public static class AddUserProfileContracts
    {
        public record AddUserProfileRequest(string FirstName,
                              string LastName,
                              string Email);

        public record AddUserProfileResponse(string Id,
                               string FirstName,
                               string LastName,
                               string Email,
                               bool IsEnabled);
    }


    public sealed class CreateUserValidator : AbstractValidator<AddUserProfileRequest>
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
