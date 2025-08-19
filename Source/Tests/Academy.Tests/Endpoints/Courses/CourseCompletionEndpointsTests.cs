using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Courses;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Academy.Tests.Endpoints.Courses
{
    [TestClass]
    public class CourseCompletionEndpointsTests
    {
        private ApplicationDbContext GetDbContextWithCompletions()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CompletionsDb_" + Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new ApplicationDbContext(options);
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
            db.CourseCompletions.Add(new Shared.Data.Models.Courses.CourseCompletion
            {
                Id = 100,
                CourseId = 1,
                UserProfileId = 10,
                SubmittedOn = DateTime.UtcNow,
                IsPassed = true,
                FinalScore = 95.5,
                Feedback = "Great course!"
            });
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task SubmitCompletion_CreatesCompletion_WhenNotExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCompletions();
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
            FakeHttpContextAccessor httpContextAccessor = new FakeHttpContextAccessor(userId: 20);
            CourseCompletionContracts.SubmitCompletionRequest request = new CourseCompletionContracts.SubmitCompletionRequest
            (
                CourseId: 1,
                UserProfileId: 20,
                IsPassed: true,
                FinalScore: 88.0,
                Feedback: "Well structured."
            );

            // Act
            Results<Ok<CourseCompletionContracts.CompletionResponse>, BadRequest<ErrorResponse>> result = await CourseCompletionEndpoints.SubmitCompletion("tenant", 1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseCompletionContracts.CompletionResponse>? okResult = result.Result as Ok<CourseCompletionContracts.CompletionResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value?.CourseId);
            Assert.AreEqual(20, okResult.Value?.UserProfileId);
            Assert.AreEqual(88.0, okResult.Value?.FinalScore);
        }

        [TestMethod]
        public async Task SubmitCompletion_ReturnsExisting_WhenAlreadyCompleted()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCompletions();
            FakeHttpContextAccessor httpContextAccessor = new FakeHttpContextAccessor(userId: 10);
            CourseCompletionContracts.SubmitCompletionRequest request = new CourseCompletionContracts.SubmitCompletionRequest
            (
                CourseId: 1,
                UserProfileId: 10,
                IsPassed: true,
                FinalScore: 95.5,
                Feedback: "Great course!"
            );

            // Act
            Results<Ok<CourseCompletionContracts.CompletionResponse>, BadRequest<ErrorResponse>> result = await CourseCompletionEndpoints.SubmitCompletion("tenant", 1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseCompletionContracts.CompletionResponse>? okResult = result.Result as Ok<CourseCompletionContracts.CompletionResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(10, okResult.Value?.UserProfileId);
            Assert.AreEqual(95.5, okResult.Value?.FinalScore);
        }

        [TestMethod]
        public async Task SubmitCompletion_ReturnsUnauthorized_WhenNoUser()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCompletions();
            FakeHttpContextAccessor httpContextAccessor = new FakeHttpContextAccessor(userId: null);
            CourseCompletionContracts.SubmitCompletionRequest request = new CourseCompletionContracts.SubmitCompletionRequest
            (
                CourseId: 1,
                UserProfileId: 10,
                IsPassed: true,
                FinalScore: 95.5,
                Feedback: "Great course!"
            );

            // Act
            Results<Ok<CourseCompletionContracts.CompletionResponse>, BadRequest<ErrorResponse>> result = await CourseCompletionEndpoints.SubmitCompletion("tenant", 1, request, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task SubmitCompletion_ReturnsNotFound_WhenUserProfileMissing()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCompletions();
            FakeHttpContextAccessor httpContextAccessor = new FakeHttpContextAccessor(userId: 999);
            CourseCompletionContracts.SubmitCompletionRequest request = new CourseCompletionContracts.SubmitCompletionRequest
            (
                CourseId: 1,
                UserProfileId: 999,
                IsPassed: true,
                FinalScore: 80.0,
                Feedback: "Missing user."
            );

            // Act
            Results<Ok<CourseCompletionContracts.CompletionResponse>, BadRequest<ErrorResponse>> result = await CourseCompletionEndpoints.SubmitCompletion("tenant", 1, request, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task GetCourseCompletions_ReturnsAllCompletions()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCompletions();

            // Act
            Results<Ok<CourseCompletionContracts.ListCompletionsResponse>, BadRequest<ErrorResponse>> result = await CourseCompletionEndpoints.GetCourseCompletions("tenant", 1, db);

            // Assert
            Ok<CourseCompletionContracts.ListCompletionsResponse>? okResult = result.Result as Ok<CourseCompletionContracts.ListCompletionsResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value?.Completions.Count);
            Assert.AreEqual(10, okResult.Value?.Completions[0].UserProfileId);
        }

        // Helper: Fake IHttpContextAccessor for user simulation
        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(long? userId)
            {
                System.Security.Claims.Claim[] claims = userId.HasValue
                    ? new[] { new System.Security.Claims.Claim("Id", userId.Value.ToString()) }
                    : new System.Security.Claims.Claim[0];
                System.Security.Claims.ClaimsPrincipal user = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(claims, "TestAuth")
                );
                HttpContext = new DefaultHttpContext { User = user };
            }
        }
    }
}