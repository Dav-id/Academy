using Academy.Shared.Localisation;
using FluentValidation;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Contracts for course completion endpoints.
    /// </summary>
    public static class CourseCompletionContracts
    {
        /// <summary>
        /// Request to submit a course completion.
        /// </summary>
        public record SubmitCompletionRequest(long CourseId, long UserProfileId, double FinalScore, bool IsPassed, string Feedback);

        /// <summary>
        /// Response for a course completion.
        /// </summary>
        public record CompletionResponse(
            long Id,
            long CourseId,
            long UserProfileId,
            DateTime SubmittedOn,
            bool IsPassed,
            double FinalScore,
            string Feedback);

        /// <summary>
        /// Response for a list of course completions.
        /// </summary>
        public record ListCompletionsResponse(IReadOnlyList<CompletionResponse> Completions, int TotalCompletionCount);
    }

    /// <summary>
    /// Validator for <see cref="CourseCompletionContracts.SubmitCompletionRequest"/>.
    /// </summary>
    public sealed class SubmitCompletionValidator : AbstractValidator<CourseCompletionContracts.SubmitCompletionRequest>
    {
        public SubmitCompletionValidator()
        {
            RuleFor(x => x.CourseId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.UserProfileId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.FinalScore).InclusiveBetween(0, 100);
            RuleFor(x => x.Feedback).MaximumLength(1000);
        }
    }
}