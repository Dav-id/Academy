using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Contracts for lesson prerequisite assessment endpoints.
    /// </summary>
    public static class LessonPrerequisiteAssessmentContracts
    {
        /// <summary>
        /// Request to create a lesson prerequisite assessment (an assessment required before a lesson).
        /// </summary>
        public record CreateLessonPrerequisiteAssessmentRequest(
            long LessonId,
            long PrerequisiteAssessmentId
        );

        /// <summary>
        /// Response for a lesson prerequisite assessment.
        /// </summary>
        public record LessonPrerequisiteAssessmentResponse(            
            long LessonId,
            long PrerequisiteAssessmentId
        );

        /// <summary>
        /// Response for a list of lesson prerequisite assessments.
        /// </summary>
        public record ListLessonPrerequisiteAssessmentsResponse(IReadOnlyList<LessonPrerequisiteAssessmentResponse> Prerequisites);
    }

    /// <summary>
    /// Validator for <see cref="LessonPrerequisiteAssessmentContracts.CreateLessonPrerequisiteAssessmentRequest"/>.
    /// </summary>
    public sealed class CreateLessonPrerequisiteAssessmentValidator : AbstractValidator<LessonPrerequisiteAssessmentContracts.CreateLessonPrerequisiteAssessmentRequest>
    {
        public CreateLessonPrerequisiteAssessmentValidator()
        {
            RuleFor(x => x.LessonId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.PrerequisiteAssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x).Must(x => x.LessonId != x.PrerequisiteAssessmentId)
                .WithMessage(_ => "A lesson cannot have itself as a prerequisite assessment.");
        }
    }
}