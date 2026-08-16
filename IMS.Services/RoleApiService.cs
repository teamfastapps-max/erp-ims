using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IMS.Helpers.Options;
using IMS.Models.Common;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class RoleApiService : IRoleApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SystemApiSettings _settings;
        private readonly ILogger<RoleApiService> _logger;

        public RoleApiService(IHttpClientFactory httpClientFactory,IOptions<ApiOptions> apiOptions,ILogger<RoleApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = apiOptions.Value.SystemApi;
            _logger = logger;
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleId, string accessToken)
        {
            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(accessToken))
                return new List<string>();

            var path = _settings.Endpoints.RolePermissions.Replace("{roleId}", roleId);
            var url = $"{_settings.BaseUrl}{path}";

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetRolePermissions failed for role {RoleId}: {Status} - {Body}", roleId, response.StatusCode, body);
                    return new List<string>();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<ApiResponseModel<List<string>>>(body, options);
                return parsed?.Data ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetRolePermissions for role {RoleId}", roleId);
                return new List<string>();
            }
        }
    }
}
