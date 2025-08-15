using Academy.Services.Api.Endpoints.Accounts.GetUserProfile;

using Aspire.Hosting;

using Microsoft.Extensions.Logging;

using System.Net.Http.Json;


namespace Academy.Tests.Services
{
    [TestClass]
    public class UsersApiTests
    {
        private static DistributedApplication? _app;
        private static HttpClient? _client;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            CancellationToken cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;

            IDistributedApplicationTestingBuilder appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Academy_AppHost>(cancellationToken);
            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
            });

            appHost.Services.ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddStandardResilienceHandler());

            _app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            _client = _app.CreateHttpClient("services-api");
            await _app.ResourceNotifications.WaitForResourceHealthyAsync("services-api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            _client?.Dispose();

            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetProfile_ReturnsBadRequest_WhenUserIdIsMissing()
        {
            if (_client == null)
            {
                Assert.Fail("HttpClient is not initialized.");
                return;
            }

            HttpResponseMessage response = await _client.GetAsync("api/v1/users/");

            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetProfile_ReturnsOk_WhenUserExists()
        {
            if (_client == null)
            {
                Assert.Fail("HttpClient is not initialized.");
                return;
            }

            HttpResponseMessage response = await _client.GetAsync("api/v1/users/valid-user-id");

            response.EnsureSuccessStatusCode();

            GetUserProfileContracts.GetUserProfileResponse? result = await response.Content.ReadFromJsonAsync<GetUserProfileContracts.GetUserProfileResponse>();
            Assert.IsNotNull(result);
        }
    }
}
