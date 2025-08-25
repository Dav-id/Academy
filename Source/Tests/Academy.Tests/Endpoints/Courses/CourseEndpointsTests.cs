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
    public class CourseEndpointsTests
    {
        private static ApplicationDbContext GetDbContextWithCourses()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CoursesDb_" + System.Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new(options);
            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant", Description = "Desc", IsDeleted = false });
            db.Courses.AddRange(
                new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1", Description = "Desc 1", TenantId = 1 },
                new Shared.Data.Models.Courses.Course { Id = 2, Title = "Course 2", Description = "Desc 2", TenantId = 1 }
            );
            db.SaveChanges();
            db.SetTenant(1);
            return db;
        }


        [TestMethod]
        public async Task GetCourses_ReturnsAllCourses_ForInstructor()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);

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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);

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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);

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
            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant", Description = "Desc", IsDeleted = false });
            db.SetTenant(1);
            db.SaveChanges();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);
            CourseContracts.CreateCourseRequest request = new CourseContracts.CreateCourseRequest("New Course", "A new course");

            // Act
            Results<Ok<CourseContracts.CourseResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.CreateCourse("tenant", request, db, httpContextAccessor);

            // Assert
            Ok<CourseContracts.CourseResponse>? okResult = result.Result as Ok<CourseContracts.CourseResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("New Course", okResult.Value?.Title);
            Assert.AreEqual(1, db.Courses.Count(c => c.Title == "New Course"));
        }

        [TestMethod]
        public async Task UpdateCourse_UpdatesCourse_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithCourses();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);
            CourseContracts.UpdateCourseRequest request = new CourseContracts.UpdateCourseRequest(1, "Updated Course", "Updated Desc");

            // Act
            Results<Ok<CourseContracts.CourseResponse>, BadRequest<ErrorResponse>> result = await CourseEndpoints.UpdateCourse("tenant", 1, request, db, httpContextAccessor);

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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isInstructor: true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await CourseEndpoints.DeleteCourse("tenant", 1, db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Shared.Data.Models.Courses.Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == 1);
            Assert.IsNull(course);
        }
    }    
}