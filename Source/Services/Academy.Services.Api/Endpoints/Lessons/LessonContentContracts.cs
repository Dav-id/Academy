using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Contracts for lesson content endpoints.
    /// </summary>
    public static class LessonContentContracts
    {
        /// <summary>
        /// Request to create lesson content.
        /// </summary>
        public record CreateLessonContentRequest(
            long LessonId,
            string ContentType,
            string ContentData
        );

        /// <summary>
        /// Request to update lesson content.
        /// </summary>
        public record UpdateLessonContentRequest(
            long Id,
            long LessonId,
            string ContentType,
            string ContentData
        );

        /// <summary>
        /// Response for lesson content.
        /// </summary>
        public record LessonContentResponse(
            long Id,
            long LessonId,
            string ContentType,
            string ContentData
        );

        /// <summary>
        /// Response for a list of lesson contents.
        /// </summary>
        public record ListLessonContentsResponse(IReadOnlyList<LessonContentResponse> Contents, int TotalLessonContentCount);
    }

    /// <summary>
    /// Validator for <see cref="LessonContentContracts.CreateLessonContentRequest"/>.
    /// </summary>
    public sealed class CreateLessonContentValidator : AbstractValidator<LessonContentContracts.CreateLessonContentRequest>
    {
        public CreateLessonContentValidator()
        {
            RuleFor(x => x.LessonId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ContentData).NotEmpty();
        }
    }

    /// <summary>
    /// Validator for <see cref="LessonContentContracts.UpdateLessonContentRequest"/>.
    /// </summary>
    public sealed class UpdateLessonContentValidator : AbstractValidator<LessonContentContracts.UpdateLessonContentRequest>
    {
        public UpdateLessonContentValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.LessonId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ContentData).NotEmpty();
        }
    }
}