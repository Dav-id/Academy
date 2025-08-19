using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment question option endpoints.
    /// </summary>
    public static class AssessmentQuestionOptionContracts
    {
        /// <summary>
        /// Request to create an assessment question option.
        /// </summary>
        public record CreateAssessmentQuestionOptionRequest(
            long AssessmentQuestionId,
            string OptionText,
            double Score,
            bool? IsCorrect,
            int Order
        );

        /// <summary>
        /// Request to update an assessment question option.
        /// </summary>
        public record UpdateAssessmentQuestionOptionRequest(
            long Id,
            long AssessmentQuestionId,
            string OptionText,
            double Score,
            bool? IsCorrect,
            int Order
        );

        /// <summary>
        /// Response for an assessment question option.
        /// </summary>
        public record AssessmentQuestionOptionResponse(
            long Id,
            long AssessmentQuestionId,
            string OptionText,
            double Score,
            bool? IsCorrect,
            int Order
        );

        /// <summary>
        /// Response for a list of assessment question options.
        /// </summary>
        public record ListAssessmentQuestionOptionsResponse(IReadOnlyList<AssessmentQuestionOptionResponse> Options);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentQuestionOptionContracts.CreateAssessmentQuestionOptionRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentQuestionOptionValidator : AbstractValidator<AssessmentQuestionOptionContracts.CreateAssessmentQuestionOptionRequest>
    {
        public CreateAssessmentQuestionOptionValidator()
        {
            RuleFor(x => x.AssessmentQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.OptionText).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Validator for <see cref="AssessmentQuestionOptionContracts.UpdateAssessmentQuestionOptionRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentQuestionOptionValidator : AbstractValidator<AssessmentQuestionOptionContracts.UpdateAssessmentQuestionOptionRequest>
    {
        public UpdateAssessmentQuestionOptionValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AssessmentQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.OptionText).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        }
    }
}