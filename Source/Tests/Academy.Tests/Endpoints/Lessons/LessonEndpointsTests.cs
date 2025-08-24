using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Lessons;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Academy.Tests.Endpoints.Lessons
{
    [TestClass]
    public class LessonEndpointsTests
    {
        private ApplicationDbContext GetDbContextWithLessons()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "LessonsDb_" + System.Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new(options);
            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant 1", Description = "Desc 1", IsDeleted = false });
            db.Courses.Add(new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1", TenantId = 1 });
            db.CourseModules.Add(new Shared.Data.Models.Courses.CourseModule { Id = 10, Title = "Module 1", CourseId = 1, TenantId = 1, Order = 1 });
            db.Lessons.AddRange(
                new Shared.Data.Models.Lessons.Lesson { Id = 100, CourseModuleId = 10, Title = "Lesson 1", Summary = "Summary 1", Order = 1, TenantId = 1 },
                new Shared.Data.Models.Lessons.Lesson { Id = 101, CourseModuleId = 10, Title = "Lesson 2", Summary = "Summary 2", Order = 2, TenantId = 1 }
            );
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@email.com",
                IdentityProvider = "local",
                IdentityProviderId = "john"
            });
            db.CourseEnrollments.Add(new Shared.Data.Models.Courses.CourseEnrollment
            {
                Id = 1,
                CourseId = 1,
                UserProfileId = 1,
                TenantId = 1
            });
            db.SetTenant(1);
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task GetLessons_ReturnsAllLessons_ForInstructor()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessons();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 1, isInstructor: true);

            // Act
            Results<Ok<LessonContracts.ListLessonsResponse>, BadRequest<ErrorResponse>> result = await LessonEndpoints.GetLessons("tenant", 10, db, httpContextAccessor);

            // Assert
            Ok<LessonContracts.ListLessonsResponse>? okResult = result.Result as Ok<LessonContracts.ListLessonsResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value?.Lessons.Count);
        }

        [TestMethod]
        public async Task GetLesson_ReturnsLesson_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessons();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 1, isInstructor: true);

            // Act
            Results<Ok<LessonContracts.LessonResponse>, BadRequest<ErrorResponse>> result = await LessonEndpoints.GetLesson("tenant", 10, 100, db, httpContextAccessor);

            // Assert
            Ok<LessonContracts.LessonResponse>? okResult = result.Result as Ok<LessonContracts.LessonResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Lesson 1", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task GetLesson_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessons();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 1, isInstructor: true);

            // Act
            Results<Ok<LessonContracts.LessonResponse>, BadRequest<ErrorResponse>> result = await LessonEndpoints.GetLesson("tenant", 10, 999, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task CreateLesson_AddsLesson()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessons();
            LessonContracts.CreateLessonRequest request = new(10, "New Lesson", "A new lesson", 3, null, null);
            FakeHttpContextAccessor httpContextAccessor = new(userId: 1, isInstructor: true);

            // Act
            Results<Ok<LessonContracts.LessonResponse>, BadRequest<ErrorResponse>> result = await LessonEndpoints.CreateLesson("tenant", 10, request, db, httpContextAccessor);

            // Assert
            Ok<LessonContracts.LessonResponse>? okResult = result.Result as Ok<LessonContracts.LessonResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("New Lesson", okResult.Value?.Title);
            Assert.AreEqual(3, okResult.Value?.Order);
            Assert.AreEqual(3, db.Lessons.Count());
        }

        [TestMethod]
        public async Task UpdateLesson_UpdatesLesson_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessons();
            LessonContracts.UpdateLessonRequest request = new(100, 10, "Updated Lesson", "Updated Summary", 5, null, null);
            FakeHttpContextAccessor httpContextAccessor = new(userId: 1, isInstructor: true);

            // Act
            Results<Ok<LessonContracts.LessonResponse>, BadRequest<ErrorResponse>> result = await LessonEndpoints.UpdateLesson("tenant", 10, 100, request, db, httpContextAccessor);

            // Assert
            Ok<LessonContracts.LessonResponse>? okResult = result.Result as Ok<LessonContracts.LessonResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Updated Lesson", okResult.Value?.Title);
            Assert.AreEqual(5, okResult.Value?.Order);
        }

        [TestMethod]
        public async Task DeleteLesson_RemovesLesson()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessons();
            FakeHttpContextAccessor httpContextAccessor = new(userId: 1, isInstructor: true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await LessonEndpoints.DeleteLesson("tenant", 10, 100, db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, db.Lessons.Count());
        }

        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(long? userId, bool isInstructor = false)
            {
                List<System.Security.Claims.Claim> claims = [];
                if (userId.HasValue)
                {
                    claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.Value.ToString()));
                }
                if (isInstructor)
                {
                    claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Instructor"));
                }

                System.Security.Claims.ClaimsPrincipal user = new(
                    new System.Security.Claims.ClaimsIdentity(claims, "TestAuth")
                );
                HttpContext = new DefaultHttpContext { User = user };
            }
        }
    }
}