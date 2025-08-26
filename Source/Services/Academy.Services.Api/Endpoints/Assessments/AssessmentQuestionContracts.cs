using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment question endpoints.
    /// </summary>
    public static class AssessmentSectionQuestionContracts
    {
        /// <summary>
        /// Request to create an assessment question.
        /// </summary>
        public record CreateAssessmentSectionQuestionRequest(
            long AssessmentId,
            string QuestionText,
            AssessmentQuestionType QuestionType,
            int Order,
            int? MinimumOptionChoiceSelections,
            int? MaximumOptionChoiceSelections
        );

        /// <summary>
        /// Request to update an assessment question.
        /// </summary>
        public record UpdateAssessmentSectionQuestionRequest(
            long Id,
            long AssessmentId,
            string QuestionText,
            AssessmentQuestionType QuestionType,
            int Order,
            int? MinimumOptionChoiceSelections,
            int? MaximumOptionChoiceSelections
        );

        /// <summary>
        /// Response for an assessment question.
        /// </summary>
        public record AssessmentSectionQuestionResponse(
            long Id,
            long AssessmentId,
            string QuestionText,
            AssessmentQuestionType QuestionType,
            int Order,
            int? MinimumOptionChoiceSelections,
            int? MaximumOptionChoiceSelections
        );

        /// <summary>
        /// Response for a list of assessment questions.
        /// </summary>
        public record ListAssessmentSectionQuestionsResponse(IReadOnlyList<AssessmentSectionQuestionResponse> Questions, int TotalQuestionCount);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentSectionQuestionContracts.CreateAssessmentSectionQuestionRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentSectionQuestionValidator : AbstractValidator<AssessmentSectionQuestionContracts.CreateAssessmentSectionQuestionRequest>
    {
        public CreateAssessmentSectionQuestionValidator()
        {
            RuleFor(x => x.AssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.QuestionType).IsInEnum();
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MinimumOptionChoiceSelections).GreaterThanOrEqualTo(0).When(x => x.MinimumOptionChoiceSelections.HasValue);
            RuleFor(x => x.MaximumOptionChoiceSelections).GreaterThanOrEqualTo(0).When(x => x.MaximumOptionChoiceSelections.HasValue);
        }
    }

    /// <summary>
    /// Validator for <see cref="AssessmentSectionQuestionContracts.UpdateAssessmentSectionQuestionRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentSectionQuestionValidator : AbstractValidator<AssessmentSectionQuestionContracts.UpdateAssessmentSectionQuestionRequest>
    {
        public UpdateAssessmentSectionQuestionValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AssessmentId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.QuestionType).IsInEnum();
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MinimumOptionChoiceSelections).GreaterThanOrEqualTo(0).When(x => x.MinimumOptionChoiceSelections.HasValue);
            RuleFor(x => x.MaximumOptionChoiceSelections).GreaterThanOrEqualTo(0).When(x => x.MaximumOptionChoiceSelections.HasValue);
        }
    }

    /// <summary>
    /// Enum for quiz question types.
    /// </summary>
    public enum AssessmentQuestionType
    {
        SingleChoice = 0,
        MultipleChoice = 1,
        Boolean = 2,
        ShortAnswer = 3,
        LongAnswer = 4,
        IntegerAnswer = 5,
        DecimalAnswer = 6
    }
}