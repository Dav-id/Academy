using Academy.Shared.Localisation;
using FluentValidation;
using System.Collections.Generic;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment question answer endpoints.
    /// </summary>
    public static class AssessmentQuestionAnswerContracts
    {
        /// <summary>
        /// Request to create an assessment question answer, including selected options.
        /// </summary>
        public record CreateAssessmentQuestionAnswerRequest(
            long AssessmentId,
            long AssessmentQuestionId,
            IReadOnlyList<long> SelectedOptionIds
        );

        /// <summary>
        /// Request to update an assessment question answer, including selected options.
        /// </summary>
        public record UpdateAssessmentQuestionAnswerRequest(
            long Id,
            long AssessmentId,
            long AssessmentQuestionId,
            IReadOnlyList<long> SelectedOptionIds
        );

        /// <summary>
        /// Response for an assessment question answer.
        /// </summary>
        public record AssessmentQuestionAnswerResponse(
            long Id,
            long AssessmentId,
            long AssessmentQuestionId,
            IReadOnlyList<AssessmentQuestionAnswerOptionResponse> SelectedOptions
        );

        /// <summary>
        /// Response for an assessment question answer option.
        /// </summary>
        public record AssessmentQuestionAnswerOptionResponse(
            long Id,
            long AssessmentQuestionAnswerId,
            long AssessmentQuestionOptionId
        );

        /// <summary>
        /// Response for a list of assessment question answers.
        /// </summary>
        public record ListAssessmentQuestionAnswersResponse(IReadOnlyList<AssessmentQuestionAnswerResponse> Answers);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentQuestionAnswerContracts.CreateAssessmentQuestionAnswerRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentQuestionAnswerValidator : AbstractValidator<AssessmentQuestionAnswerContracts.CreateAssessmentQuestionAnswerRequest>
    {
        public CreateAssessmentQuestionAnswerValidator()
        {
            RuleFor(x => x.AssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.AssessmentQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.SelectedOptionIds).NotNull();
        }
    }

    /// <summary>
    /// Validator for <see cref="AssessmentQuestionAnswerContracts.UpdateAssessmentQuestionAnswerRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentQuestionAnswerValidator : AbstractValidator<AssessmentQuestionAnswerContracts.UpdateAssessmentQuestionAnswerRequest>
    {
        public UpdateAssessmentQuestionAnswerValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.AssessmentQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.SelectedOptionIds).NotNull();
        }
    }
}