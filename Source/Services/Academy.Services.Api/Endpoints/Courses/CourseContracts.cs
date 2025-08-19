using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Contains request, response, and validation contracts for course-related API endpoints.
    /// </summary>
    public static class CourseContracts
    {
        /// <summary>
        /// Represents a request to create a new course.
        /// </summary>
        /// <param name="Title">The title of the course.</param>
        /// <param name="Description">The description of the course.</param>
        public record CreateCourseRequest(string Title, string Description);

        /// <summary>
        /// Represents a request to update an existing course.
        /// </summary>
        /// <param name="Id">The unique identifier of the course.</param>
        /// <param name="Title">The updated title of the course.</param>
        /// <param name="Description">The updated description of the course.</param>
        public record UpdateCourseRequest(long Id, string Title, string Description);

        /// <summary>
        /// Represents a course response object.
        /// </summary>
        /// <param name="Id">The unique identifier of the course.</param>
        /// <param name="Title">The title of the course.</param>
        /// <param name="Description">The description of the course.</param>
        public record CourseResponse(long Id, string Title, string Description);

        /// <summary>
        /// Represents a response containing a list of courses.
        /// </summary>
        /// <param name="Courses">The list of course response objects.</param>
        public record ListCoursesResponse(IReadOnlyList<CourseResponse> Courses);
    }

    /// <summary>
    /// Validator for <see cref="CourseContracts.CreateCourseRequest"/>.
    /// </summary>
    public sealed class CreateCourseValidator : AbstractValidator<CourseContracts.CreateCourseRequest>
    {
        public CreateCourseValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .MaximumLength(200).WithMessage(_ => ModelTranslation.Global__Field__MaxLength);
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(_ => ModelTranslation.Global__Field__MaxLength);
        }
    }

    /// <summary>
    /// Validator for <see cref="CourseContracts.UpdateCourseRequest"/>.
    /// </summary>
    public sealed class UpdateCourseValidator : AbstractValidator<CourseContracts.UpdateCourseRequest>
    {
        public UpdateCourseValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required)
                .MaximumLength(200).WithMessage(_ => ModelTranslation.Global__Field__MaxLength);
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(_ => ModelTranslation.Global__Field__MaxLength);
        }
    }
}