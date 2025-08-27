using Academy.Shared.Localisation;

using FluentValidation;

using static Academy.Services.Api.Endpoints.Lessons.LessonSectionContracts;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Contracts for lesson section endpoints.
    /// </summary>
    public static class LessonSectionContracts
    {
        /// <summary>
        /// Request to create a lesson section.
        /// </summary>
        public record CreateLessonSectionRequest(
            long LessonId,
            string Description,
            int Order
        );

        /// <summary>
        /// Request to update a lesson section.
        /// </summary>
        public record UpdateLessonSectionRequest(
            long Id,
            long LessonId,
            string Description,
            int Order
        );

        /// <summary>
        /// Response for a lesson section.
        /// </summary>
        public record LessonSectionResponse(
            long Id,
            long LessonId,
            string Description,
            int Order
        );

        /// <summary>
        /// Response for a list of lesson sections.
        /// </summary>
        public record ListLessonSectionsResponse(IReadOnlyList<LessonSectionResponse> Sections, int TotalLessonSectionCount);
    }

    /// <summary>
    /// Validator for <see cref="CreateLessonSectionRequest"/>.
    /// </summary>
    public sealed class CreateLessonSectionValidator : AbstractValidator<CreateLessonSectionRequest>
    {
        public CreateLessonSectionValidator()
        {
            RuleFor(x => x.LessonId)
                .GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .MaximumLength(500);
            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Validator for <see cref="UpdateLessonSectionRequest"/>.
    /// </summary>
    public sealed class UpdateLessonSectionValidator : AbstractValidator<UpdateLessonSectionRequest>
    {
        public UpdateLessonSectionValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.LessonId)
                .GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .MaximumLength(500);
            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0);
        }
    }
}