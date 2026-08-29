using IMS.Helpers.Options;
using IMS.Models.Common;
using IMS.Models.TenantUser;
using IMS.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IMS.Services
{
    //public class RoleApiService : IRoleApiService
    //{
    //    private readonly IHttpClientFactory _httpClientFactory;
    //    private readonly SystemApiSettings _settings;
    //    private readonly ILogger<RoleApiService> _logger;

    //    public RoleApiService(IHttpClientFactory httpClientFactory,IOptions<ApiOptions> apiOptions,ILogger<RoleApiService> logger)
    //    {
    //        _httpClientFactory = httpClientFactory;
    //        _settings = apiOptions.Value.SystemApi;
    //        _logger = logger;
    //    }

    //    public async Task<List<string>> GetRolePermissionsAsync(string roleId, string accessToken)
    //    {
    //        if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(accessToken))
    //            return new List<string>();

    //        var path = _settings.Endpoints.RolePermissions.Replace("{roleId}", roleId);
    //        var url = $"{_settings.BaseUrl}{path}";

    //        try
    //        {
    //            var httpClient = _httpClientFactory.CreateClient();
    //            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    //            var response = await httpClient.GetAsync(url);
    //            var body = await response.Content.ReadAsStringAsync();

    //            if (!response.IsSuccessStatusCode)
    //            {
    //                _logger.LogWarning("GetRolePermissions failed for role {RoleId}: {Status} - {Body}", roleId, response.StatusCode, body);
    //                return new List<string>();
    //            }

    //            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    //            var parsed = JsonSerializer.Deserialize<ApiResponseModel<List<string>>>(body, options);
    //            return parsed?.Data ?? new List<string>();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error calling GetRolePermissions for role {RoleId}", roleId);
    //            return new List<string>();
    //        }
    //    }
    //}

    public class RoleApiService : IRoleApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SystemApiSettings _systemApiSettings;
        private readonly UserApiSettings _userApiSettings;
        private readonly ILogger<RoleApiService> _logger;

        private static readonly JsonSerializerOptions DeserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public RoleApiService(IHttpClientFactory httpClientFactory, IOptions<ApiOptions> apiOptions, ILogger<RoleApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _systemApiSettings = apiOptions.Value.SystemApi;
            _userApiSettings = apiOptions.Value.UserApi;
            _logger = logger;
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleId, string accessToken)
        {
            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(accessToken))
                return new List<string>();

            var path = _systemApiSettings.Endpoints.RolePermissions.Replace("{roleId}", roleId);
            var url = $"{_systemApiSettings.BaseUrl}{path}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetRolePermissions failed for role {RoleId}: {Status} - {Body}", roleId, response.StatusCode, body);
                    return new List<string>();
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseModel<List<string>>>(body, DeserializeOptions);
                return parsed?.Data ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetRolePermissions for role {RoleId}", roleId);
                return new List<string>();
            }
        }

        public async Task<List<TenantRoleModel>> GetTenantRolesAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Cannot fetch tenant roles — access token is missing");
                return new List<TenantRoleModel>();
            }

            var url = $"{_userApiSettings.BaseUrl}{_userApiSettings.Endpoints.TenantRoles}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("roles/tenant GET returned {StatusCode}: {Body}", response.StatusCode, body);
                    return new List<TenantRoleModel>();
                }

                // NOTE: response shape for this endpoint was not confirmed against a real
                // sample. Tries { data: [...] } first, then falls back to { data: { docs: [...] } }.
                try
                {
                    var parsedList = JsonSerializer.Deserialize<ApiResponseModel<List<TenantRoleModel>>>(body, DeserializeOptions);
                    if (parsedList?.Data != null) return parsedList.Data;
                }
                catch (JsonException)
                {
                    // fall through to try the paged shape
                }

                var parsedPaged = JsonSerializer.Deserialize<ApiResponseModel<PagedResult<TenantRoleModel>>>(body, DeserializeOptions);
                return parsedPaged?.Data?.Docs ?? new List<TenantRoleModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling roles/tenant GET");
                return new List<TenantRoleModel>();
            }
        }

        private HttpClient CreateAuthorizedClient(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return httpClient;
        }
    }

}
