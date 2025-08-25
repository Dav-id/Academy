using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Contracts for course module endpoints.
    /// </summary>
    public static class CourseModuleContracts
    {
        /// <summary>
        /// Request to create a course module.
        /// </summary>
        public record CreateModuleRequest(long CourseId, string Title, string Description, int Order);

        /// <summary>
        /// Request to update a course module.
        /// </summary>
        public record UpdateModuleRequest(long Id, long CourseId, string Title, string Description, int Order);

        /// <summary>
        /// Response for a course module.
        /// </summary>
        public record ModuleResponse(long Id, long CourseId, string Title, string Description, int Order);

        /// <summary>
        /// Response for a list of course modules.
        /// </summary>
        public record ListModulesResponse(IReadOnlyList<ModuleResponse> Modules, int TotalModuleCount);
    }

    /// <summary>
    /// Validator for <see cref="CourseModuleContracts.CreateModuleRequest"/>.
    /// </summary>
    public sealed class CreateModuleValidator : AbstractValidator<CourseModuleContracts.CreateModuleRequest>
    {
        public CreateModuleValidator()
        {
            RuleFor(x => x.CourseId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }

    /// <summary>
    /// Validator for <see cref="CourseModuleContracts.UpdateModuleRequest"/>.
    /// </summary>
    public sealed class UpdateModuleValidator : AbstractValidator<CourseModuleContracts.UpdateModuleRequest>
    {
        public UpdateModuleValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.CourseId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}