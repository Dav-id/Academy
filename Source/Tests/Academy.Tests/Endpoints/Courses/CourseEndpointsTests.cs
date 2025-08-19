using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Courses;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Academy.Tests.Endpoints.Courses
{
    [TestClass]
    public class CourseEndpointsTests
    {
        private static ApplicationDbContext GetDbContextWithCourses()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CoursesDb_" + System.Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new(options);
            db.Courses.AddRange(
                new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1", Description = "Desc 1" },
                new Shared.Data.Models.Courses.Course { Id = 2, Title = "Course 2", Description = "Desc 2" }
            );
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task GetCourses_ReturnsAllCourses_ForInstructor()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            FakeHttpContextAccessor httpContextAccessor = new(isInstructor: true);

            // Act
            Results<Ok<CourseContracts.ListCoursesResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.GetCourses("tenant", db, httpContextAccessor);

            // Assert
            Ok<CourseContracts.ListCoursesResponse>? okResult = result.Result as Ok<CourseContracts.ListCoursesResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value?.Courses.Count);
        }

        [TestMethod]
        public async Task GetCourse_ReturnsCourse_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            FakeHttpContextAccessor httpContextAccessor = new(isInstructor: true);

            // Act
            Results<Ok<CourseContracts.CourseResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.GetCourse("tenant", 1, db, httpContextAccessor);

            // Assert
            Ok<CourseContracts.CourseResponse>? okResult = result.Result as Ok<CourseContracts.CourseResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Course 1", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task GetCourse_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            FakeHttpContextAccessor httpContextAccessor = new(isInstructor: true);

            // Act
            Results<Ok<CourseContracts.CourseResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.GetCourse("tenant", 999, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task CreateCourse_AddsCourse()
        {
            // Arrange
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CreateCourseDb_" + System.Guid.NewGuid())
                .Options;
            ApplicationDbContext db = new(options);
            FakeHttpContextAccessor httpContextAccessor = new(isInstructor: true);
            CourseContracts.CreateCourseRequest request = new("New Course", "A new course");

            // Act
            Results<Ok<CourseContracts.CourseResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.CreateCourse(request, db, httpContextAccessor);

            // Assert
            Ok<CourseContracts.CourseResponse>? okResult = result.Result as Ok<CourseContracts.CourseResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("New Course", okResult.Value?.Title);
            Assert.AreEqual(1, db.Courses.Count());
        }

        [TestMethod]
        public async Task UpdateCourse_UpdatesCourse_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            FakeHttpContextAccessor httpContextAccessor = new(isInstructor: true);
            CourseContracts.UpdateCourseRequest request = new(1, "Updated Course", "Updated Desc");

            // Act
            Results<Ok<CourseContracts.CourseResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.UpdateCourse(1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseContracts.CourseResponse>? okResult = result.Result as Ok<CourseContracts.CourseResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Updated Course", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task DeleteCourse_SoftDeletesCourse()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            FakeHttpContextAccessor httpContextAccessor = new(isInstructor: true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await CourseEndpoints.DeleteCourse(1, db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Shared.Data.Models.Courses.Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == 1);
            Assert.IsNull(course);
        }

        // Helper: Fake IHttpContextAccessor for role simulation
        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(bool isInstructor = false)
            {
                System.Security.Claims.ClaimsPrincipal user = new(
                    new System.Security.Claims.ClaimsIdentity(
                        isInstructor
                            ? new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Instructor") }
                            : [],
                        "TestAuth"
                    )
                );
                HttpContext = new DefaultHttpContext { User = user };
            }
        }
    }
}