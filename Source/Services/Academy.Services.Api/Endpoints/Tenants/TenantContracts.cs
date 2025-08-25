using FluentValidation;

namespace Academy.Services.Api.Endpoints.Tenants
{
    /// <summary>
    /// Contracts for tenant endpoints.
    /// </summary>
    public static class TenantContracts
    {
        /// <summary>
        /// Request to create a tenant.
        /// </summary>
        public record CreateTenantRequest(
            string UrlStub,
            string Title,
            string? Description,
            string TenantAccountOwnerFirstName,
            string TenantAccountOwnerLastName,
            string TenantAccountOwnerEmail
        );

        /// <summary>
        /// Request to update a tenant.
        /// </summary>
        public record UpdateTenantRequest(
            string UrlStub,
            string Title,
            string? Description
        );

        /// <summary>
        /// Response for a tenant.
        /// </summary>
        public record TenantResponse(
            long Id,
            string UrlStub,
            string Title,
            string? Description
        );

        /// <summary>
        /// Response for a list of tenants.
        /// </summary>
        public record ListTenantsResponse(IReadOnlyList<TenantResponse> Tenants, int TotalTenantCount);
    }

    /// <summary>
    /// Validator for <see cref="TenantContracts.CreateTenantRequest"/>.
    /// </summary>
    public sealed class CreateTenantValidator : AbstractValidator<TenantContracts.CreateTenantRequest>
    {
        public CreateTenantValidator()
        {
            RuleFor(x => x.UrlStub).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);

            RuleFor(x => x.TenantAccountOwnerFirstName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.TenantAccountOwnerLastName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.TenantAccountOwnerEmail).NotEmpty().EmailAddress();
        }
    }

    /// <summary>
    /// Validator for <see cref="TenantContracts.UpdateTenantRequest"/>.
    /// </summary>
    public sealed class UpdateTenantValidator : AbstractValidator<TenantContracts.UpdateTenantRequest>
    {
        public UpdateTenantValidator()
        {
            RuleFor(x => x.UrlStub).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}