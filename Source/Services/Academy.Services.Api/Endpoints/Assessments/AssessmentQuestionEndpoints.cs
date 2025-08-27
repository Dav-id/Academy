using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentSectionQuestionContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessment questions.
    /// </summary>
    public static class AssessmentSectionQuestionEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            // Updated: courseId is now part of the route hierarchy
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions", GetAssessmentSectionQuestions)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{id}", GetAssessmentSectionQuestion)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{id}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions", CreateAssessmentSectionQuestion)
                .Validate<RouteHandlerBuilder, CreateAssessmentSectionQuestionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions");

            app.MapPut("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{id}", UpdateAssessmentSectionQuestion)
                .Validate<RouteHandlerBuilder, UpdateAssessmentSectionQuestionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("PUT: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{id}", DeleteAssessmentSectionQuestion)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{id}");
        }

        /// <summary>
        /// Gets all questions for an assessment section.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentSectionQuestionsResponse>, BadRequest<ErrorResponse>>> GetAssessmentSectionQuestions(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool isInstructor = ((user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false));
            long? userId = user?.GetUserId();

            // Validate assessment belongs to course
            var assessment = await db.Assessments
                .Include(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.CourseModule != null && a.CourseModule.CourseId == courseId);
            if (assessment == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment with Id {assessmentId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Only select questions that belong to the given section and assessment
            IQueryable<Shared.Data.Models.Assessments.AssessmentSectionQuestion> query = db.AssessmentSectionQuestions
                .AsNoTracking()
                .Where(q => q.SectionId == sectionId && q.Section != null && q.Section.AssessmentId == assessmentId);

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not enrolled on this course.",
                        null,
                        httpContextAccessor?.HttpContext?.TraceIdentifier
                    ));
                }

                // Only include assessments where the user is enrolled in the parent course
                query = query
                    .Where(a =>
                        a.Section != null &&
                        a.Section.Assessment != null &&
                        a.Section.Assessment.CourseModule != null &&
                        a.Section.Assessment.CourseModule.Course != null &&
                        a.Section.Assessment.CourseModule.Course.Enrollments.Any(e => e.UserProfile != null && e.UserProfile.Id == userId.Value)
                    );
            }

            int totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<AssessmentSectionQuestionResponse> questions = await query
                .OrderBy(q => q.Order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new AssessmentSectionQuestionResponse(
                    q.Id,
                    q.Section != null ? q.Section.AssessmentId : 0,
                    q.QuestionText,
                    q.QuestionType.ConvertAssessmentQuestionType(),
                    q.Order,
                    q.MinimumOptionChoiceSelections,
                    q.MaximumOptionChoiceSelections
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentSectionQuestionsResponse(questions, totalCount));
        }

        /// <summary>
        /// Gets a specific question for an assessment section.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionResponse>, BadRequest<ErrorResponse>>> GetAssessmentSectionQuestion(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool isInstructor = ((user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false));
            long? userId = user?.GetUserId();

            // Validate assessment belongs to course
            var assessment = await db.Assessments
                .Include(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.CourseModule != null && a.CourseModule.CourseId == courseId);
            if (assessment == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment with Id {assessmentId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            IQueryable<Shared.Data.Models.Assessments.AssessmentSectionQuestion> query = db.AssessmentSectionQuestions
                .AsNoTracking()
                .Where(q => q.Id == id && q.SectionId == sectionId && q.Section != null && q.Section.AssessmentId == assessmentId);

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not enrolled on this course.",
                        null,
                        httpContextAccessor?.HttpContext?.TraceIdentifier
                    ));
                }

                // Only include assessments where the user is enrolled in the parent course
                query = query
                    .Where(a =>
                        a.Section != null &&
                        a.Section.Assessment != null &&
                        a.Section.Assessment.CourseModule != null &&
                        a.Section.Assessment.CourseModule.Course != null &&
                        a.Section.Assessment.CourseModule.Course.Enrollments.Any(e => e.UserProfile != null && e.UserProfile.Id == userId.Value)
                    );
            }

            AssessmentSectionQuestionResponse? question = await query
                .Select(q => new AssessmentSectionQuestionResponse(
                    q.Id,
                    q.Section != null ? q.Section.AssessmentId : 0,
                    q.QuestionText,
                    q.QuestionType.ConvertAssessmentQuestionType(),
                    q.Order,
                    q.MinimumOptionChoiceSelections,
                    q.MaximumOptionChoiceSelections
                ))
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question with Id {id} not found in section {sectionId} and assessment {assessmentId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(question);
        }

        /// <summary>
        /// Creates a new question for an assessment section.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionResponse>, BadRequest<ErrorResponse>>> CreateAssessmentSectionQuestion(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            CreateAssessmentSectionQuestionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to create assessment questions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Ensure section exists and belongs to the assessment and course
            var section = await db.AssessmentSections
                .Include(s => s.Assessment)
                    .ThenInclude(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.AssessmentId == assessmentId && s.Assessment != null && s.Assessment.CourseModule != null && s.Assessment.CourseModule.CourseId == courseId);
            if (section == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Section with Id {sectionId} does not belong to assessment {assessmentId} in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentSectionQuestion question = new()
            {
                SectionId = sectionId,
                QuestionText = request.QuestionText,
                QuestionType = request.QuestionType.ConvertAssessmentQuestionType(),
                Order = request.Order,
                MinimumOptionChoiceSelections = request.MinimumOptionChoiceSelections,
                MaximumOptionChoiceSelections = request.MaximumOptionChoiceSelections,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.AssessmentSectionQuestions.Add(question);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentSectionQuestionResponse(
                question.Id,
                assessmentId,
                question.QuestionText,
                question.QuestionType.ConvertAssessmentQuestionType(),
                question.Order,
                question.MinimumOptionChoiceSelections,
                question.MaximumOptionChoiceSelections
            ));
        }

        /// <summary>
        /// Updates an existing question for an assessment section.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionResponse>, BadRequest<ErrorResponse>>> UpdateAssessmentSectionQuestion(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long id,
            UpdateAssessmentSectionQuestionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to update assessment questions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Ensure question exists and belongs to the section, assessment, and course
            var question = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .FirstOrDefaultAsync(q =>
                    q.Id == id &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );
            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question with Id {id} not found in section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            question.QuestionText = request.QuestionText;
            question.QuestionType = request.QuestionType.ConvertAssessmentQuestionType();
            question.Order = request.Order;
            question.MinimumOptionChoiceSelections = request.MinimumOptionChoiceSelections;
            question.MaximumOptionChoiceSelections = request.MaximumOptionChoiceSelections;
            question.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            question.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentSectionQuestionResponse(
                question.Id,
                assessmentId,
                question.QuestionText,
                question.QuestionType.ConvertAssessmentQuestionType(),
                question.Order,
                question.MinimumOptionChoiceSelections,
                question.MaximumOptionChoiceSelections
            ));
        }

        /// <summary>
        /// Deletes an assessment question by ID.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteAssessmentSectionQuestion(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to delete assessment questions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Ensure question exists and belongs to the section, assessment, and course
            var question = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .FirstOrDefaultAsync(q =>
                    q.Id == id &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );
            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question with Id {id} not found in section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            question.IsDeleted = true;
            question.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            question.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}