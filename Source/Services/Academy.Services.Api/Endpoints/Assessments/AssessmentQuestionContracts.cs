using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Contracts for assessment question endpoints.
    /// </summary>
    public static class AssessmentQuestionContracts
    {
        /// <summary>
        /// Request to create an assessment question.
        /// </summary>
        public record CreateAssessmentQuestionRequest(
            long AssessmentId,
            string QuestionText,
            QuizQuestionType QuestionType,
            int Order,
            int? MinimumOptionChoiceSelections,
            int? MaximumOptionChoiceSelections
        );

        /// <summary>
        /// Request to update an assessment question.
        /// </summary>
        public record UpdateAssessmentQuestionRequest(
            long Id,
            long AssessmentId,
            string QuestionText,
            QuizQuestionType QuestionType,
            int Order,
            int? MinimumOptionChoiceSelections,
            int? MaximumOptionChoiceSelections
        );

        /// <summary>
        /// Response for an assessment question.
        /// </summary>
        public record AssessmentQuestionResponse(
            long Id,
            long AssessmentId,
            string QuestionText,
            QuizQuestionType QuestionType,
            int Order,
            int? MinimumOptionChoiceSelections,
            int? MaximumOptionChoiceSelections
        );

        /// <summary>
        /// Response for a list of assessment questions.
        /// </summary>
        public record ListAssessmentQuestionsResponse(IReadOnlyList<AssessmentQuestionResponse> Questions, int TotalQuestionCount);
    }

    /// <summary>
    /// Validator for <see cref="AssessmentQuestionContracts.CreateAssessmentQuestionRequest"/>.
    /// </summary>
    public sealed class CreateAssessmentQuestionValidator : AbstractValidator<AssessmentQuestionContracts.CreateAssessmentQuestionRequest>
    {
        public CreateAssessmentQuestionValidator()
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
    /// Validator for <see cref="AssessmentQuestionContracts.UpdateAssessmentQuestionRequest"/>.
    /// </summary>
    public sealed class UpdateAssessmentQuestionValidator : AbstractValidator<AssessmentQuestionContracts.UpdateAssessmentQuestionRequest>
    {
        public UpdateAssessmentQuestionValidator()
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
    public enum QuizQuestionType
    {
        SingleChoice = 0,
        MultipleChoice = 1,
        TrueFalse = 2,
        ShortAnswer = 3,
        LongAnswer = 4
    }
}