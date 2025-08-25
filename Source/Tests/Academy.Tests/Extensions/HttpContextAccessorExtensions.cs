using Microsoft.AspNetCore.Http;

namespace Academy.Tests.Extensions
{
    public static class HttpContextAccessorExtensions
    {
        public static IHttpContextAccessor Wrap(this HttpContext context)
        {
            return new SimpleHttpContextAccessor(context);
        }

        private class SimpleHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }
            public SimpleHttpContextAccessor(HttpContext context) { HttpContext = context; }
        }


        public static IHttpContextAccessor GetHttpContextAccessor(            
            long? userId,
            bool isAdministrator = false,
            bool isInstructor = false,
            bool isGlobalAdministrator = false,
            string tenantStub = "tenant")
        {
            List<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            if (userId.HasValue)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }
            if (isAdministrator)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, $"{tenantStub}:Administrator"));
            }
            if (isGlobalAdministrator)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Administrator"));
            }
            if (isInstructor && !string.IsNullOrEmpty(tenantStub))
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, $"{tenantStub}:Instructor"));
            }

            System.Security.Claims.ClaimsPrincipal user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims, "TestAuth"));

            return new DefaultHttpContext { User = user }.Wrap();
        }
    }
}