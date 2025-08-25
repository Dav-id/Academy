using Academy.Services.Api.Endpoints;
using Academy.Services.Api.Endpoints.Tenants;
using Academy.Shared.Data.Contexts;
using Academy.Tests.Extensions;
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
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@email.com",
                IdentityProvider = "local",
                IdentityProviderId = "john",
            });
            db.SaveChanges();            
            return db;
        }

        [TestMethod]
        public async Task GetTenants_ReturnsAllTenants()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isGlobalAdministrator: true);

            // Act
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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isAdministrator: true);

            // Act
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
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isAdministrator: true);

            // Act
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

            FakeAuthClient fakeAuthClient = new();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isAdministrator: true);

            // Act
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.CreateTenant(request, db, httpContextAccessor, fakeAuthClient);

            // Assert
            Ok<TenantContracts.TenantResponse>? okResult = result.Result as Ok<TenantContracts.TenantResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("newstub", okResult.Value?.UrlStub);
            Assert.AreEqual(1, db.Tenants.Count());
            Assert.AreEqual(1, fakeAuthClient.CreatedUsers.Count);
            Assert.AreEqual(3, fakeAuthClient.CreatedRoles.Count);
            Assert.AreEqual(1, fakeAuthClient.UserRoleAssignments.Count);
        }

        [TestMethod]
        public async Task UpdateTenant_UpdatesTenant_WhenExists_WithGlobalAdmin()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            db.UserProfiles.Add(new Shared.Data.Models.Accounts.UserProfile
            {
                Id = 2,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@email.com",
                IdentityProvider = "local",
                IdentityProviderId = "john",
                TenantId = 1
            });
            await db.SaveChangesAsync();
            db.SetTenant(1);
            TenantContracts.UpdateTenantRequest request = new("updatedstub", "Updated Tenant", "Updated Desc");
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(2, isAdministrator: true, tenantStub:"tenant1");

            // Act
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.UpdateTenant("tenant1", request, db, httpContextAccessor);

            // Assert
            Ok<TenantContracts.TenantResponse>? okResult = result.Result as Ok<TenantContracts.TenantResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("updatedstub", okResult.Value?.UrlStub);
            Assert.AreEqual("Updated Tenant", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task UpdateTenant_UpdatesTenant_WhenExists_WithTenantAdmin()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            TenantContracts.UpdateTenantRequest request = new("updatedstub2", "Updated Tenant 2", "Updated Desc 2");
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isAdministrator: true, tenantStub: "tenant1");

            // Act
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.UpdateTenant("tenant1", request, db, httpContextAccessor);

            // Assert
            Ok<TenantContracts.TenantResponse>? okResult = result.Result as Ok<TenantContracts.TenantResponse>;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("updatedstub2", okResult.Value?.UrlStub);
            Assert.AreEqual("Updated Tenant 2", okResult.Value?.Title);
        }

        [TestMethod]
        public async Task UpdateTenant_ReturnsForbidden_WhenNotAdmin()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            TenantContracts.UpdateTenantRequest request = new("shouldnotupdate", "Should Not Update", "No Desc");
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isAdministrator: false);

            // Act
            Results<Ok<TenantContracts.TenantResponse>, BadRequest<ErrorResponse>> result = await TenantEndpoints.UpdateTenant("tenant1", request, db, httpContextAccessor);

            // Assert
            BadRequest<ErrorResponse>? badRequest = result.Result as BadRequest<ErrorResponse>;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status403Forbidden, badRequest.Value?.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTenant_SoftDeletesTenant()
        {
            // Arrange
            ApplicationDbContext db = GetDbContextWithTenants();
            IHttpContextAccessor httpContextAccessor = HttpContextAccessorExtensions.GetHttpContextAccessor(1, isAdministrator: true);

            // Act
            Results<Ok, BadRequest<ErrorResponse>> result = await TenantEndpoints.DeleteTenant("tenant1", db, httpContextAccessor);

            // Assert
            Ok? okResult = result.Result as Ok;
            Assert.IsNotNull(okResult);
            Shared.Data.Models.Tenants.Tenant? tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == 1);
            Assert.IsNull(tenant);
        }
    }
}