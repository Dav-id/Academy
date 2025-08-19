using FluentValidation;

namespace Academy.Services.Api.Endpoints.Roles
{
    /// <summary>
    /// Contracts for external role mapping endpoints.
    /// </summary>
    public static class ExternalRoleMappingContracts
    {
        /// <summary>
        /// Request to create an external role mapping.
        /// </summary>
        public record CreateExternalRoleMappingRequest(
            string Issuer,
            string ExternalClaimType,
            string ExternalClaimValue,
            string AppRole
        );

        /// <summary>
        /// Request to update an external role mapping.
        /// </summary>
        public record UpdateExternalRoleMappingRequest(
            long Id,
            string Issuer,
            string ExternalClaimType,
            string ExternalClaimValue,
            string AppRole
        );

        /// <summary>
        /// Response for an external role mapping.
        /// </summary>
        public record ExternalRoleMappingResponse(
            long Id,
            string Issuer,
            string ExternalClaimType,
            string ExternalClaimValue,
            string AppRole
        );

        /// <summary>
        /// Response for a list of external role mappings.
        /// </summary>
        public record ListExternalRoleMappingsResponse(IReadOnlyList<ExternalRoleMappingResponse> Mappings);
    }

    /// <summary>
    /// Validator for <see cref="ExternalRoleMappingContracts.CreateExternalRoleMappingRequest"/>.
    /// </summary>
    public sealed class CreateExternalRoleMappingValidator : AbstractValidator<ExternalRoleMappingContracts.CreateExternalRoleMappingRequest>
    {
        public CreateExternalRoleMappingValidator()
        {
            RuleFor(x => x.Issuer).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ExternalClaimType).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ExternalClaimValue).NotEmpty().MaximumLength(200);
            RuleFor(x => x.AppRole).NotEmpty().MaximumLength(100);
        }
    }

    /// <summary>
    /// Validator for <see cref="ExternalRoleMappingContracts.UpdateExternalRoleMappingRequest"/>.
    /// </summary>
    public sealed class UpdateExternalRoleMappingValidator : AbstractValidator<ExternalRoleMappingContracts.UpdateExternalRoleMappingRequest>
    {
        public UpdateExternalRoleMappingValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Issuer).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ExternalClaimType).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ExternalClaimValue).NotEmpty().MaximumLength(200);
            RuleFor(x => x.AppRole).NotEmpty().MaximumLength(100);
        }
    }
}