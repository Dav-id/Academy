using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Courses;
using Academy.Shared.Data.Contexts;
using Academy.Tests.Extensions;

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
            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant", Description = "Desc", IsDeleted = false });
            db.Courses.Add(new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1", TenantId = 1 });
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 10,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentityProvider = "local",
                IdentityProviderId = "john",
                TenantId = 1
            });
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 20,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                IdentityProvider = "local",
                IdentityProviderId = "jane",
                TenantId = 1
            });
            db.CourseEnrollments.Add(new Shared.Data.Models.Courses.CourseEnrollment
            {
                Id = 100,
                CourseId = 1,
                UserProfileId = 10,
                TenantId = 1
            });
            db.SetTenant(1);
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task EnrollInCourse_EnrollsUser_WhenNotAlreadyEnrolled()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);
            CourseEnrollmentContracts.EnrollRequest request = new CourseEnrollmentContracts.EnrollRequest(1, 10);

            // Act
            Results<Ok<CourseEnrollmentContracts.EnrollmentResponse>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.EnrollInCourse("tenant", 1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseEnrollmentContracts.EnrollmentResponse>? okResult = result.Result as Ok<CourseEnrollmentContracts.EnrollmentResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value?.CourseId);
            Assert.AreEqual(10, okResult.Value?.UserProfileId);
            Assert.AreEqual(100, okResult.Value?.Id);
        }

        [TestMethod]
        public async Task EnrollInCourse_ReturnsExisting_WhenAlreadyEnrolled()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor:true);
            CourseEnrollmentContracts.EnrollRequest request = new CourseEnrollmentContracts.EnrollRequest(1, 10);

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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(null);
            CourseEnrollmentContracts.EnrollRequest request = new CourseEnrollmentContracts.EnrollRequest(1, 0);

            // Act
            Results<Ok<CourseEnrollmentContracts.EnrollmentResponse>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.EnrollInCourse("tenant", 1, request, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status403Forbidden, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task GetCourseEnrollments_ReturnsAllEnrollments()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);

            // Act
            Results<Ok<CourseEnrollmentContracts.ListEnrollmentsResponse>, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.GetCourseEnrollments("tenant", 1, db, httpContextAccessor);

            // Assert
            Ok<CourseEnrollmentContracts.ListEnrollmentsResponse>? okResult = result.Result as Ok<CourseEnrollmentContracts.ListEnrollmentsResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value?.Enrollments.Count);
            Assert.AreEqual(100, okResult.Value?.Enrollments[0].Id);
        }

        [TestMethod]
        public async Task UnenrollFromCourse_RemovesEnrollment()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithEnrollments();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(10, isInstructor: true);

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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(999, isInstructor:true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await CourseEnrollmentEndpoints.UnenrollFromCourse("tenant", 1, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }
    }

}