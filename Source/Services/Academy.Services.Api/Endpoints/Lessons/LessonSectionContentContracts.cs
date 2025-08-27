using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Contracts for lesson content endpoints.
    /// </summary>
    public static class LessonSectionContentContracts
    {
        /// <summary>
        /// Request to create lesson content.
        /// </summary>
        public record CreateLessonSectionContentRequest(
            long LessonId,
            string ContentType,
            string ContentData
        );

        /// <summary>
        /// Request to update lesson content.
        /// </summary>
        public record UpdateLessonSectionContentRequest(
            long Id,
            long LessonId,
            string ContentType,
            string ContentData
        );

        /// <summary>
        /// Response for lesson content.
        /// </summary>
        public record LessonContentSectionResponse(
            long Id,
            long LessonId,
            string ContentType,
            string ContentData
        );

        /// <summary>
        /// Response for a list of lesson contents.
        /// </summary>
        public record ListLessonContentsResponse(IReadOnlyList<LessonContentSectionResponse> Contents, int TotalLessonContentCount);
    }

    /// <summary>
    /// Validator for <see cref="LessonSectionContentContracts.CreateLessonSectionContentRequest"/>.
    /// </summary>
    public sealed class CreateLessonContentValidator : AbstractValidator<LessonSectionContentContracts.CreateLessonSectionContentRequest>
    {
        public CreateLessonContentValidator()
        {
            RuleFor(x => x.LessonId).GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ContentData).NotEmpty();
        }
    }

    /// <summary>
    /// Validator for <see cref="LessonSectionContentContracts.UpdateLessonSectionContentRequest"/>.
    /// </summary>
    public sealed class UpdateLessonContentValidator : AbstractValidator<LessonSectionContentContracts.UpdateLessonSectionContentRequest>
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