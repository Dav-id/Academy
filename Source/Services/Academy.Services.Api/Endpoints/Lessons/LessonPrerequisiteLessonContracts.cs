using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Contracts for lesson prerequisite lesson endpoints.
    /// </summary>
    public static class LessonPrerequisiteLessonContracts
    {
        /// <summary>
        /// Request to create a lesson prerequisite (a lesson required before another lesson).
        /// </summary>
        public record CreateLessonPrerequisiteLessonRequest(
            long LessonId,
            long PrerequisiteLessonId
        );

        /// <summary>
        /// Response for a lesson prerequisite lesson.
        /// </summary>
        public record LessonPrerequisiteLessonResponse(
            long LessonId,
            long PrerequisiteLessonId
        );

        /// <summary>
        /// Response for a list of lesson prerequisite lessons.
        /// </summary>
        public record ListLessonPrerequisiteLessonsResponse(IReadOnlyList<LessonPrerequisiteLessonResponse> Prerequisites);
    }

    /// <summary>
    /// Validator for <see cref="LessonPrerequisiteLessonContracts.CreateLessonPrerequisiteLessonRequest"/>.
    /// </summary>
    public sealed class CreateLessonPrerequisiteLessonValidator : AbstractValidator<LessonPrerequisiteLessonContracts.CreateLessonPrerequisiteLessonRequest>
    {
        public CreateLessonPrerequisiteLessonValidator()
        {
            RuleFor(x => x.LessonId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.PrerequisiteLessonId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x).Must(x => x.LessonId != x.PrerequisiteLessonId)
                .WithMessage(_ => "A lesson cannot be a prerequisite of itself.");
        }
    }
}