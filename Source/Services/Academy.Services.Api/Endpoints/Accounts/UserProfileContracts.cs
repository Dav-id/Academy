using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Accounts
{
    /// <summary>
    /// Contains request, response, and validation contracts for user profile (account) API endpoints.
    /// </summary>
    public static class UserProfileContracts
    {
        /// <summary>
        /// Represents a request to create a new user profile.
        /// </summary>
        /// <param name="FirstName">The user's first name.</param>
        /// <param name="LastName">The user's last name.</param>
        /// <param name="Email">The user's email address.</param>
        public record CreateUserProfileRequest(string FirstName, string LastName, string Email);

        /// <summary>
        /// Represents a request to update an existing user profile.
        /// </summary>
        /// <param name="Id">The unique identifier of the user profile.</param>
        /// <param name="FirstName">The updated first name.</param>
        /// <param name="LastName">The updated last name.</param>
        /// <param name="Email">The updated email address.</param>
        public record UpdateUserProfileRequest(long Id, string FirstName, string LastName, string Email);

        /// <summary>
        /// Represents a user profile response object.
        /// </summary>
        /// <param name="Id">The unique identifier of the user profile.</param>
        /// <param name="FirstName">The user's first name.</param>
        /// <param name="LastName">The user's last name.</param>
        /// <param name="Email">The user's email address.</param>
        /// <param name="IsEnabled">Indicates if the user profile is enabled.</param>
        public record UserProfileResponse(long Id, string FirstName, string LastName, string Email, bool IsEnabled);

        /// <summary>
        /// Represents a response containing a list of user profiles.
        /// </summary>
        /// <param name="Users">The list of user profile response objects.</param>
        /// <param name="TotalUserCount">Total number of user profiles.</param>
        public record ListUserProfilesResponse(IReadOnlyList<UserProfileResponse> Users, int TotalUserCount);
    }

    /// <summary>
    /// Validator for <see cref="UserProfileContracts.CreateUserProfileRequest"/>.
    /// </summary>
    public sealed class CreateUserProfileValidator : AbstractValidator<UserProfileContracts.CreateUserProfileRequest>
    {
        public CreateUserProfileValidator()
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

    /// <summary>
    /// Validator for <see cref="UserProfileContracts.UpdateUserProfileRequest"/>.
    /// </summary>
    public sealed class UpdateUserProfileValidator : AbstractValidator<UserProfileContracts.UpdateUserProfileRequest>
    {
        public UpdateUserProfileValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
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