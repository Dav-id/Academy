using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Tenants;
using Academy.Shared.Data.Contexts;
using Academy.Tests.Fakes;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Academy.Tests.Endpoints.Tenants
{
    [TestClass]
    public class TenantEndpointsTests
    {
        private static ApplicationDbContext GetDbContextWithTenants()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TenantsDb_" + System.Guid.NewGuid())
                .Options;

            ApplicationDbContext db = new(options);
            db.Tenants.AddRange(
                new Shared.Data.Models.Tenants.Tenant { Id = 1, UrlStub = "tenant1", Title = "Tenant 1", Description = "Desc 1", IsDeleted = false },
                new Shared.Data.Models.Tenants.Tenant { Id = 2, UrlStub = "tenant2", Title = "Tenant 2", Description = "Desc 2", IsDeleted = false }
            );
            db.SaveChanges();
            return db;
        }

        [TestMethod]
        public async Task GetTenants_ReturnsAllTenants()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();

            // Act
            FakeHttpContextAccessor httpContextAccessor = new(isAdministrator: true);
            Results<Ok<TenantContracts.ListTenantsResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.GetTenants(db, httpContextAccessor);

            // Assert
            Ok<TenantContracts.ListTenantsResponse>? okResult = result.Result as Ok<TenantContracts.ListTenantsResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value?.Tenants.Count);
        }

        [TestMethod]
        public async Task GetTenant_ReturnsTenant_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();

            // Act
            FakeHttpContextAccessor httpContextAccessor = new(isAdministrator: true);
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.GetTenant("tenant1", db, httpContextAccessor);

            // Assert
            Ok<TenantContracts.TenantResponse>? okResult = result.Result as Ok<TenantContracts.TenantResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("tenant1", okResult.Value?.UrlStub);
        }

        [TestMethod]
        public async Task GetTenant_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();

            // Act
            FakeHttpContextAccessor httpContextAccessor = new(isAdministrator: true);
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.GetTenant("tenant999", db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status404NotFound, badRequest.Value?.StatusCode);
        }


        [TestMethod]
        public async Task CreateTenant_AddsTenant_AndCreatesUserAndRoles()
        {
            // Arrange
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CreateTenantDb_" + System.Guid.NewGuid())
                .Options;
            ApplicationDbContext db = new(options);

            TenantContracts.CreateTenantRequest request = new(
                UrlStub: "newstub",
                Title: "New Tenant",
                Description: "A new tenant",
                TenantAccountOwnerFirstName: "Alice",
                TenantAccountOwnerLastName: "Smith",
                TenantAccountOwnerEmail: "alice@example.com"
            );

            FakeAuthClient fakeAuthClient = new FakeAuthClient();

            FakeHttpContextAccessor httpContextAccessor = new(isAdministrator: true);

            // Act
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.CreateTenant(request, db, httpContextAccessor, fakeAuthClient);

            // Assert
            Ok<TenantContracts.TenantResponse>? okResult = result.Result as Ok<TenantContracts.TenantResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("newstub", okResult.Value?.UrlStub);
            Assert.AreEqual(1, db.Tenants.Count());
            Assert.HasCount(1, fakeAuthClient.CreatedUsers);
            Assert.HasCount(3, fakeAuthClient.CreatedRoles);
            Assert.HasCount(1, fakeAuthClient.UserRoleAssignments);
        }

        [TestMethod]
        public async Task UpdateTenant_UpdatesTenant_WhenExists()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            TenantContracts.UpdateTenantRequest request = new(1, "updatedstub", "Updated Tenant", "Updated Desc");

            // Act
            FakeHttpContextAccessor httpContextAccessor = new(isAdministrator: true);
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.UpdateTenant("tenant1", request, db, httpContextAccessor);

            // Assert
            Ok<TenantContracts.TenantResponse>? okResult = result.Result as Ok<TenantContracts.TenantResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("updatedstub", okResult.Value?.UrlStub);
            Assert.AreEqual("Updated Tenant", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task DeleteTenant_SoftDeletesTenant()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            FakeHttpContextAccessor httpContextAccessor = new(isAdministrator: true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await TenantEndpoints.DeleteTenant("tenant1", db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Shared.Data.Models.Tenants.Tenant? tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == 1);
            Assert.IsNull(tenant);
        }

        private class FakeHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }

            public FakeHttpContextAccessor(bool isAdministrator = false)
            {
                System.Security.Claims.ClaimsPrincipal user = new(
                    new System.Security.Claims.ClaimsIdentity(
                        isAdministrator
                            ? new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Administrator") }
                            : [],
                        "TestAuth"
                    )
                );
                HttpContext = new DefaultHttpContext { User = user };
            }
        }
    }
}