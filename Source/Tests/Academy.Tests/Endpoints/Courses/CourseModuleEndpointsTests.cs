using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Courses;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace Academy.Tests.Endpoints.Courses
{
    [TestClass]
    public class CourseModuleEndpointsTests
    {
        private ApplicationDbContext GetDbContextWithModules()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestModulesDb_" + System.Guid.NewGuid())
                .Options;
            ApplicationDbContext db = new(options);
            db.SetTenant(1);

            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 1,
                IdentityProvider = "local",
                IdentityProviderId = Guid.Parse("36947696-2fd9-45c0-b408-eb4249e13eb8").ToString(),
                FirstName = "John",
                LastName = "Doe",
                Email = "test@email.com"
            });

            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant 1", Description = "Desc 1", IsDeleted = false });

            Shared.Data.Models.Courses.Course c = new()
            {
                Id = 1,
                Title = "Course 1",
                Description = "Desc 1",
                TenantId = 1,
                IsDeleted = false
            };

            c.CourseModules.Add(new Shared.Data.Models.Courses.CourseModule
            {
                Id = 101,
                Title = "Module 1",
                Description = "Desc 1",
                Order = 1,
                TenantId = 1,
            });

            c.CourseModules.Add(new Shared.Data.Models.Courses.CourseModule
            {
                Id = 102,
                Title = "Module 2",
                Description = "Desc 2",
                Order = 2,
                TenantId = 1,
            });

            c.Enrollments.Add(new Shared.Data.Models.Courses.CourseEnrollment { Id = 1, UserProfileId = 1, TenantId = 1, IsDeleted = false });

            db.Courses.Add(c);



            db.SaveChanges();

            return db;
        }

        [TestMethod]
        public async Task GetCourseModules_ReturnsAllModules()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithModules();

            
            FakeHttpContextAccessor httpContextAccessor = new(1, isInstructor: true);

            // Act
            Results<Ok<CourseModuleContracts.ListModulesResponse>, BadRequest<ErrorResponse>> result = await CourseModuleEndpoints.GetModules("tenant", 1, db, httpContextAccessor);

            // Assert
            Ok<CourseModuleContracts.ListModulesResponse>? okResult = result.Result as Ok<CourseModuleContracts.ListModulesResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value?.Modules.Count);
        }

        [TestMethod]
        public async Task GetCourseModule_ReturnsModule_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithModules();

            FakeHttpContextAccessor httpContextAccessor = new(1, isInstructor: true);

            // Act
            Results<Ok<CourseModuleContracts.ModuleResponse>, BadRequest<ErrorResponse>> result = await CourseModuleEndpoints.GetModule("tenant", 1, 101, db, httpContextAccessor);

            // Assert
            Ok<CourseModuleContracts.ModuleResponse>? okResult = result.Result as Ok<CourseModuleContracts.ModuleResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Module 1", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task GetCourseModule_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithModules();

            FakeHttpContextAccessor httpContextAccessor = new(1, isInstructor: true);

            // Act
            Results<Ok<CourseModuleContracts.ModuleResponse>, BadRequest<ErrorResponse>> result = await CourseModuleEndpoints.GetModule("tenant", 1, 999, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task CreateCourseModule_AddsModule()
        {
            // Arrange
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CreateModuleDb_" + System.Guid.NewGuid())
                .Options;
            ApplicationDbContext db = new(options);
            db.Tenants.Add(new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant", Title = "Tenant 1", Description = "Desc 1", IsDeleted = false });
            db.Courses.Add(new Shared.Data.Models.Courses.Course { Id = 1, Title = "Course 1", TenantId = 1 });
            db.SaveChanges();
            CourseModuleContracts.CreateModuleRequest request = new(1, "New Module", "A new module", 3);

            Guid userId = Guid.Parse("36947696-2fd9-45c0-b408-eb4249e13eb8");
            FakeHttpContextAccessor httpContextAccessor = new(1, isInstructor: true);

            // Act
            Results<Ok<CourseModuleContracts.ModuleResponse>, BadRequest<ErrorResponse>> result = await CourseModuleEndpoints.CreateModule("tenant", 1, request, db, httpContextAccessor);

            // Assert
            Ok<CourseModuleContracts.ModuleResponse>? okResult = result.Result as Ok<CourseModuleContracts.ModuleResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("New Module", okResult.Value?.Title);
            Assert.AreEqual(1, okResult.Value?.CourseId);
            Assert.AreEqual(1, db.CourseModules.Count());
        }

        [TestMethod]
        public async Task UpdateCourseModule_UpdatesModule_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithModules();
            CourseModuleContracts.UpdateModuleRequest request = new(101, 1, "Updated Module", "Updated Desc", 5);

            FakeHttpContextAccessor httpContextAccessor = new(1, isInstructor: true);

            // Act
            Results<Ok<CourseModuleContracts.ModuleResponse>, BadRequest<ErrorResponse>> result = await CourseModuleEndpoints.UpdateModule("tenant", 1, 101, request, db, httpContextAccessor);

            // Assert
            Ok<CourseModuleContracts.ModuleResponse>? okResult = result.Result as Ok<CourseModuleContracts.ModuleResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Updated Module", okResult.Value?.Title);
            Assert.AreEqual(5, okResult.Value?.Order);
        }

        [TestMethod]
        public async Task DeleteCourseModule_RemovesModule()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithModules();
            FakeHttpContextAccessor httpContextAccessor = new(1, isInstructor: true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await CourseModuleEndpoints.DeleteModule("tenant", 1, 101, db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Assert.IsFalse(db.CourseModules.Any(m => m.Id == 101));
        }

        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(long? userId, bool isInstructor = false)
            {
                List<System.Security.Claims.Claim> claims = userId.HasValue
                    ? [new(ClaimTypes.NameIdentifier, userId.Value.ToString())]
                    : [];

                if (isInstructor)
                {
                    claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Instructor"));
                }

                System.Security.Claims.ClaimsPrincipal user = new(
                    new System.Security.Claims.ClaimsIdentity(
                        claims,
                        "TestAuth"
                    )
                );
                HttpContext = new DefaultHttpContext { User = user };
            }
        }
    }
}