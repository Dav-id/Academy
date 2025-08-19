using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Courses;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Academy.Tests.Endpoints.Courses
{
    [TestClass]
    public class CourseEnrollmentEndpointsTests
    {
        private static ApplicationDbContext GetDbContextWithEnrollments()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EnrollmentsDb_" + System.Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new(options);
            db.Courses.Add(new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1" });
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 10,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentityProvider = "local",
                IdentityProviderId = "john"
            });
            db.CourseEnrollments.Add(new Shared.Data.Models.Courses.CourseEnrollment
            {
                Id = 100,
                CourseId = 1,
                UserProfileId = 10
            });
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task EnrollInCourse_EnrollsUser_WhenNotAlreadyEnrolled()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 20);
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 20,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                IdentityProvider = "local",
                IdentityProviderId = "jane"
            });
            db.SaveChanges();
            CourseEnrollmentContracts.EnrollRequest request = new(1);

            // Act
            Results<Ok<CourseEnrollmentContracts.EnrollmentResponse>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.EnrollInCourse("tenant", 1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseEnrollmentContracts.EnrollmentResponse>? okResult = result.Result as Ok<CourseEnrollmentContracts.EnrollmentResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value?.CourseId);
            Assert.AreEqual(20, okResult.Value?.UserProfileId);
        }

        [TestMethod]
        public async Task EnrollInCourse_ReturnsExisting_WhenAlreadyEnrolled()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 10);
            CourseEnrollmentContracts.EnrollRequest request = new(1);

            // Act
            Results<Ok<CourseEnrollmentContracts.EnrollmentResponse>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.EnrollInCourse("tenant", 1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseEnrollmentContracts.EnrollmentResponse>? okResult = result.Result as Ok<CourseEnrollmentContracts.EnrollmentResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(10, okResult.Value?.UserProfileId);
        }

        [TestMethod]
        public async Task EnrollInCourse_ReturnsUnauthorized_WhenNoUser()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            FakeHttpContextAccessor httpContextAccessor = new(userId: null);
            CourseEnrollmentContracts.EnrollRequest request = new(1);

            // Act
            Results<Ok<CourseEnrollmentContracts.EnrollmentResponse>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.EnrollInCourse("tenant", 1, request, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task GetCourseEnrollments_ReturnsAllEnrollments()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();

            // Act
            Results<Ok<List<CourseEnrollmentContracts.EnrollmentResponse>>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.GetCourseEnrollments("tenant", 1, db);

            // Assert
            Ok<List<CourseEnrollmentContracts.EnrollmentResponse>>? okResult = result.Result as Ok<System.Collections.Generic.List<CourseEnrollmentContracts.EnrollmentResponse>>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value?.Count);
            Assert.AreEqual(10, okResult.Value?[0].UserProfileId);
        }

        [TestMethod]
        public async Task UnenrollFromCourse_RemovesEnrollment()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 10);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.UnenrollFromCourse("tenant", 1, db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Assert.IsFalse(db.CourseEnrollments.Any(e => e.CourseId == 1 && e.UserProfileId == 10));
        }

        [TestMethod]
        public async Task UnenrollFromCourse_ReturnsNotFound_WhenNotEnrolled()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 999);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.UnenrollFromCourse("tenant", 1, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }

        // Helper: Fake IHttpContextAccessor for user simulation
        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(long? userId)
            {
                System.Security.Claims.Claim[] claims = userId.HasValue
                    ? [new System.Security.Claims.Claim("Id", userId.Value.ToString())]
                    : [];
                System.Security.Claims.ClaimsPrincipal user = new(
                    new System.Security.Claims.ClaimsIdentity(claims, "TestAuth")
                );
                HttpContext = new DefaultHttpContext { User = user };
            }
        }
    }
}