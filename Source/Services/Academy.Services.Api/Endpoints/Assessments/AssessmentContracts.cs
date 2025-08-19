using FluentValidation;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment endpoints.
    /// </summary>
    public static class AssessmentContracts
    {
        /// <summary>
        /// Request to create an assessment.
        /// </summary>
        public record CreateAssessmentRequest(
            string Title,
            string Description,
            AssessmentType AssessmentType,
            long? CourseModuleId
        );

        /// <summary>
        /// Request to update an assessment.
        /// </summary>
        public record UpdateAssessmentRequest(
            long Id,
            string Title,
            string Description,
            AssessmentType AssessmentType,
            long? CourseModuleId
        );

        /// <summary>
        /// Response for an assessment.
        /// </summary>
        public record AssessmentResponse(
            long Id,
            string Title,
            string Description,
            AssessmentType AssessmentType,
            long? CourseModuleId
        );

        /// <summary>
        /// Response for a list of assessments.
        /// </summary>
        public record ListAssessmentsResponse(IReadOnlyList<AssessmentResponse> Assessments);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentContracts.CreateAssessmentRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentValidator : AbstractValidator<AssessmentContracts.CreateAssessmentRequest>
    {
        public CreateAssessmentValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.AssessmentType).IsInEnum();
        }
    }

    /// <summary>
    /// Validator for <see cref="AssessmentContracts.UpdateAssessmentRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentValidator : AbstractValidator<AssessmentContracts.UpdateAssessmentRequest>
    {
        public UpdateAssessmentValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.AssessmentType).IsInEnum();
        }
    }

    /// <summary>
    /// Enum for assessment types.
    /// </summary>
    public enum AssessmentType
    {
        Quiz = 0,
        Exam = 1,
        Survey = 2
    }
}