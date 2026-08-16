using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using IMS.Helpers.Constants;
using IMS.Helpers.Options;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class PermissionSyncService : IPermissionSyncService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SystemApiSettings _settings;
        private readonly ILogger<PermissionSyncService> _logger;

        public PermissionSyncService(IHttpClientFactory httpClientFactory,IOptions<ApiOptions> apiOptions,ILogger<PermissionSyncService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = apiOptions.Value.SystemApi;
            _logger = logger;
        }

        public async Task SyncPermissionsAsync()
        {
            var url = $"{_settings.BaseUrl}{_settings.Endpoints.PermissionsSync}";

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync(url, Permissions.All);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Permission sync failed: {response.StatusCode} - {body}");
            }

            _logger.LogInformation("Synced {Count} permissions successfully", Permissions.All.Count);
        }
    }
}