using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Contracts for course enrollment endpoints.
    /// </summary>
    public static class CourseEnrollmentContracts
    {
        /// <summary>
        /// Request to enroll a user in a course.
        /// </summary>
        public record EnrollRequest(long CourseId);

        /// <summary>
        /// Response for a course enrollment.
        /// </summary>
        public record EnrollmentResponse(long Id, long CourseId, long UserProfileId, DateTime EnrolledOn, bool IsCompleted);

        /// <summary>
        /// Response for a list of course enrollments.
        /// </summary>        
        public record ListEnrollmentsResponse(IReadOnlyList<EnrollmentResponse> Enrollments, int TotalEnrollmentCount);
    }

    /// <summary>
    /// Validator for <see cref="CourseEnrollmentContracts.EnrollRequest"/>.
    /// </summary>
    public sealed class EnrollRequestValidator : AbstractValidator<CourseEnrollmentContracts.EnrollRequest>
    {
        public EnrollRequestValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage(_ => ModelTranslation.Global__Field__Required);
        }
    }
}