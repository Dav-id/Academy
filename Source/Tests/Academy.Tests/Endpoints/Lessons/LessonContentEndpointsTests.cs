using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Lessons;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Academy.Tests.Endpoints.Lessons
{
    [TestClass]
    public class LessonContentEndpointsTests
    {
        private ApplicationDbContext GetDbContextWithLessonContent()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "LessonContentDb_" + System.Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new(options);
            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant 1", Description = "Desc 1", IsDeleted = false });
            db.Courses.Add(new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1", TenantId = 1 });
            db.CourseModules.Add(new Shared.Data.Models.Courses.CourseModule { Id = 10, Title = "Module 1", CourseId = 1, TenantId = 1, Order = 1 });
            db.Lessons.Add(new Shared.Data.Models.Lessons.Lesson { Id = 100, CourseModuleId = 10, Title = "Lesson 1", Summary = "Summary 1", Order = 1, TenantId = 1 });
            db.LessonContents.AddRange(
                new Shared.Data.Models.Lessons.LessonContent { Id = 1000, LessonId = 100, ContentType = "Text", ContentData = "Content 1", TenantId = 1 },
                new Shared.Data.Models.Lessons.LessonContent { Id = 1001, LessonId = 100, ContentType = "Video", ContentData = "Content 2", TenantId = 1 }
            );

            db.SetTenant(1);
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task GetLessonContents_ReturnsAllContents()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessonContent();

            // Act
            Results<Ok<LessonContentContracts.ListLessonContentsResponse>, BadRequest<Services.Api.Endpoints.ErrorResponse>> result = await LessonContentEndpoints.GetLessonContents("tenant", 100, db);

            // Assert
            Ok<LessonContentContracts.ListLessonContentsResponse>? okResult = result.Result as Ok<LessonContentContracts.ListLessonContentsResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value?.Contents.Count);
        }

        [TestMethod]
        public async Task GetLessonContent_ReturnsContent_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessonContent();

            // Act
            Results<Ok<LessonContentContracts.LessonContentResponse>, BadRequest<Services.Api.Endpoints.ErrorResponse>> result = await LessonContentEndpoints.GetLessonContent("tenant", 100, 1000, db);

            // Assert
            Ok<LessonContentContracts.LessonContentResponse>? okResult = result.Result as Ok<LessonContentContracts.LessonContentResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Text", okResult.Value?.ContentType);
        }

        [TestMethod]
        public async Task GetLessonContent_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessonContent();

            // Act
            Results<Ok<LessonContentContracts.LessonContentResponse>, BadRequest<Services.Api.Endpoints.ErrorResponse>> result = await LessonContentEndpoints.GetLessonContent("tenant", 100, 9999, db);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task CreateLessonContent_AddsContent()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessonContent();
            LessonContentContracts.CreateLessonContentRequest request = new(100, "PDF", "Content 3");
            FakeHttpContextAccessor httpContextAccessor = new("instructor", isInstructor: true);

            // Act
            Results<Ok<LessonContentContracts.LessonContentResponse>, BadRequest<Services.Api.Endpoints.ErrorResponse>> result = await LessonContentEndpoints.CreateLessonContent("tenant", 100, request, db, httpContextAccessor);

            // Assert
            Ok<LessonContentContracts.LessonContentResponse>? okResult = result.Result as Ok<LessonContentContracts.LessonContentResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("PDF", okResult.Value?.ContentType);
            Assert.AreEqual(3, db.LessonContents.Count());
        }

        [TestMethod]
        public async Task UpdateLessonContent_UpdatesContent_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessonContent();
            LessonContentContracts.UpdateLessonContentRequest request = new(1000, 100, "Text", "Updated Content");
            FakeHttpContextAccessor httpContextAccessor = new("instructor", isInstructor: true);

            // Act
            Results<Ok<LessonContentContracts.LessonContentResponse>, BadRequest<Services.Api.Endpoints.ErrorResponse>> result = await LessonContentEndpoints.UpdateLessonContent("tenant", 100, 1000, request, db, httpContextAccessor);

            // Assert
            Ok<LessonContentContracts.LessonContentResponse>? okResult = result.Result as Ok<LessonContentContracts.LessonContentResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Updated Content", okResult.Value?.ContentData);
        }

        [TestMethod]
        public async Task DeleteLessonContent_RemovesContent()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithLessonContent();
            FakeHttpContextAccessor httpContextAccessor = new("instructor", isInstructor: true);

            // Act
            Results<Ok, BadRequest<Services.Api.Endpoints.ErrorResponse>> result = await LessonContentEndpoints.DeleteLessonContent("tenant", 100, 1000, db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, db.LessonContents.Count());
        }

        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(string? userName, bool isInstructor = false)
            {
                List<System.Security.Claims.Claim> claims = [];
                if (!string.IsNullOrEmpty(userName))
                {
                    claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, userName));
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