using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Contracts for lesson endpoints.
    /// </summary>
    public static class LessonContracts
    {
        /// <summary>
        /// Request to create a lesson.
        /// </summary>
        public record CreateLessonRequest(
            long CourseModuleId,
            string Title,
            string Summary,
            int Order,
            DateTime? AvailableFrom,
            DateTime? AvailableTo
        );

        /// <summary>
        /// Request to update a lesson.
        /// </summary>
        public record UpdateLessonRequest(
            long Id,
            long CourseModuleId,
            string Title,
            string Summary,
            int Order,
            DateTime? AvailableFrom,
            DateTime? AvailableTo
        );

        /// <summary>
        /// Response for a lesson.
        /// </summary>
        public record LessonResponse(
            long Id,
            long CourseModuleId,
            string Title,
            string Summary,
            int Order,
            DateTime? AvailableFrom,
            DateTime? AvailableTo
        );

        /// <summary>
        /// Response for a list of lessons.
        /// </summary>
        public record ListLessonsResponse(IReadOnlyList<LessonResponse> Lessons, int TotalLessonCount);
    }

    /// <summary>
    /// Validator for <see cref="LessonContracts.CreateLessonRequest"/>.
    /// </summary>
    public sealed class CreateLessonValidator : AbstractValidator<LessonContracts.CreateLessonRequest>
    {
        public CreateLessonValidator()
        {
            RuleFor(x => x.CourseModuleId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Summary).MaximumLength(1000);
        }
    }

    /// <summary>
    /// Validator for <see cref="LessonContracts.UpdateLessonRequest"/>.
    /// </summary>
    public sealed class UpdateLessonValidator : AbstractValidator<LessonContracts.UpdateLessonRequest>
    {
        public UpdateLessonValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.CourseModuleId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Summary).MaximumLength(1000);
        }
    }
}