using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment question option endpoints.
    /// </summary>
    public static class AssessmentSectionQuestionOptionContracts
    {
        /// <summary>
        /// Request to create an assessment question option.
        /// </summary>
        public record CreateAssessmentSectionQuestionOptionRequest(
            long AssessmentSectionQuestionId,
            string OptionText,
            double Score,
            bool? IsCorrect,
            int Order
        );

        /// <summary>
        /// Request to update an assessment question option.
        /// </summary>
        public record UpdateAssessmentSectionQuestionOptionRequest(
            long Id,
            long AssessmentSectionQuestionId,
            string OptionText,
            double Score,
            bool? IsCorrect,
            int Order
        );

        /// <summary>
        /// Response for an assessment question option.
        /// </summary>
        public record AssessmentSectionQuestionOptionResponse(
            long Id,
            long AssessmentSectionQuestionId,
            string OptionText,
            double Score,
            bool? IsCorrect,
            int Order
        );

        /// <summary>
        /// Response for a list of assessment question options.
        /// </summary>
        public record ListAssessmentSectionQuestionOptionsResponse(IReadOnlyList<AssessmentSectionQuestionOptionResponse> Options);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentSectionQuestionOptionContracts.CreateAssessmentSectionQuestionOptionRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentSectionQuestionOptionValidator : AbstractValidator<AssessmentSectionQuestionOptionContracts.CreateAssessmentSectionQuestionOptionRequest>
    {
        public CreateAssessmentSectionQuestionOptionValidator()
        {
            RuleFor(x => x.AssessmentSectionQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.OptionText).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Validator for <see cref="AssessmentSectionQuestionOptionContracts.UpdateAssessmentSectionQuestionOptionRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentSectionQuestionOptionValidator : AbstractValidator<AssessmentSectionQuestionOptionContracts.UpdateAssessmentSectionQuestionOptionRequest>
    {
        public UpdateAssessmentSectionQuestionOptionValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AssessmentSectionQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.OptionText).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        }
    }
}