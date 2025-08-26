using Academy.Shared.Localisation;
using FluentValidation;
using System.Collections.Generic;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment question answer endpoints.
    /// </summary>
    public static class AssessmentSectionQuestionAnswerContracts
    {
        /// <summary>
        /// Request to create an assessment question answer, including selected options.
        /// </summary>
        public record CreateAssessmentSectionQuestionAnswerRequest(
            long AssessmentId,
            long AssessmentSectionQuestionId,
            IReadOnlyList<long> SelectedOptionIds
        );

        /// <summary>
        /// Request to update an assessment question answer, including selected options.
        /// </summary>
        public record UpdateAssessmentSectionQuestionAnswerRequest(
            long Id,
            long AssessmentId,
            long AssessmentSectionQuestionId,
            IReadOnlyList<long> SelectedOptionIds
        );

        /// <summary>
        /// Response for an assessment question answer.
        /// </summary>
        public record AssessmentSectionQuestionAnswerResponse(
            long Id,
            long AssessmentId,
            long AssessmentSectionQuestionId,
            IReadOnlyList<AssessmentSectionQuestionAnswerOptionResponse> SelectedOptions
        );

        /// <summary>
        /// Response for an assessment question answer option.
        /// </summary>
        public record AssessmentSectionQuestionAnswerOptionResponse(
            long Id,
            long AssessmentSectionQuestionAnswerId,
            long AssessmentSectionQuestionOptionId
        );

        /// <summary>
        /// Response for a list of assessment question answers.
        /// </summary>
        public record ListAssessmentSectionQuestionAnswersResponse(IReadOnlyList<AssessmentSectionQuestionAnswerResponse> Answers, int TotalAnswerCount);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentSectionQuestionAnswerContracts.CreateAssessmentSectionQuestionAnswerRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentSectionQuestionAnswerValidator : AbstractValidator<AssessmentSectionQuestionAnswerContracts.CreateAssessmentSectionQuestionAnswerRequest>
    {
        public CreateAssessmentSectionQuestionAnswerValidator()
        {
            RuleFor(x => x.AssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.AssessmentSectionQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.SelectedOptionIds).NotNull();
        }
    }

    /// <summary>
    /// Validator for <see cref="AssessmentSectionQuestionAnswerContracts.UpdateAssessmentSectionQuestionAnswerRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentSectionQuestionAnswerValidator : AbstractValidator<AssessmentSectionQuestionAnswerContracts.UpdateAssessmentSectionQuestionAnswerRequest>
    {
        public UpdateAssessmentSectionQuestionAnswerValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.AssessmentSectionQuestionId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.SelectedOptionIds).NotNull();
        }
    }
}