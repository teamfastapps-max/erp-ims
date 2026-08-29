//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using IMS.Helpers.Options;
//using IMS.Models.Auth;
//using IMS.Models.Common;
//using IMS.Services.Interfaces;

//namespace IMS.Services
//{
//    public class UserApiService : IUserApiService
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ILogger<UserApiService> _logger;
//        private readonly UserApiSettings _userApiSettings;
//        public UserApiService(IHttpClientFactory httpClientFactory, IOptions<ApiOptions> apiOptions, ILogger<UserApiService> logger)
//        {
//            _httpClientFactory = httpClientFactory;
//            _userApiSettings = apiOptions.Value.UserApi;
//            _logger = logger;
//        }

//        public async Task<UserProfileModel> GetMyProfileAsync(string accessToken)
//        {
//            if (string.IsNullOrEmpty(accessToken))
//            {
//                _logger.LogWarning("Cannot fetch profile — access token is missing");
//                return null;
//            }
//            var url = $"{_userApiSettings.BaseUrl}{_userApiSettings.Endpoints.MyProfile}";
//            try
//            {
//                var httpClient = _httpClientFactory.CreateClient();
//                httpClient.DefaultRequestHeaders.Authorization =new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

//                var response = await httpClient.GetAsync(url);
//                var body = await response.Content.ReadAsStringAsync();

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogWarning("my-profile API returned {StatusCode}: {Body}", response.StatusCode, body);
//                    return null;
//                }

//                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//                var parsed = JsonSerializer.Deserialize<ApiResponseModel<UserProfileModel>>(body, options);

//                if (parsed?.Status != "success" || parsed.Data == null)
//                {
//                    _logger.LogWarning("my-profile API returned non-success status: {Status}", parsed?.Status);
//                    return null;
//                }

//                return parsed.Data;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error calling my-profile API");
//                return null;
//            }
//        }
//    }
//}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IMS.Helpers.Options;
using IMS.Models.Auth;
using IMS.Models.Common;
using IMS.Services.Interfaces;
using IMS.Models.TenantUser;

namespace IMS.Services
{
    public class UserApiService : IUserApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserApiService> _logger;
        private readonly UserApiSettings _userApiSettings;

        // camelCase so outgoing JSON matches the API's expected body shape
        // (email, firstName, customRoleId, ... not Email, FirstName, ...)
        private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions DeserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public UserApiService(IHttpClientFactory httpClientFactory, IOptions<ApiOptions> apiOptions, ILogger<UserApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _userApiSettings = apiOptions.Value.UserApi;
            _logger = logger;
        }

        public async Task<UserProfileModel> GetMyProfileAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Cannot fetch profile — access token is missing");
                return null;
            }

            var url = $"{_userApiSettings.BaseUrl}{_userApiSettings.Endpoints.MyProfile}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("my-profile API returned {StatusCode}: {Body}", response.StatusCode, body);
                    return null;
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseModel<UserProfileModel>>(body, DeserializeOptions);
                if (parsed?.Status != "success" || parsed.Data == null)
                {
                    _logger.LogWarning("my-profile API returned non-success status: {Status}", parsed?.Status);
                    return null;
                }

                return parsed.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling my-profile API");
                return null;
            }
        }

        // ------------------------------------------------------------------
        // Tenant user (Teacher) management
        // ------------------------------------------------------------------

        public async Task<PagedResult<TenantUserModel>> GetTenantUsersAsync(int page, int limit, string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Cannot list tenant users — access token is missing");
                return new PagedResult<TenantUserModel>();
            }

            var url = $"{_userApiSettings.BaseUrl}{_userApiSettings.Endpoints.TenantUsers}?page={page}&limit={limit}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("users/tenant list API returned {StatusCode}: {Body}", response.StatusCode, body);
                    return new PagedResult<TenantUserModel>();
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseModel<PagedResult<TenantUserModel>>>(body, DeserializeOptions);
                return parsed?.Data ?? new PagedResult<TenantUserModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling users/tenant list API");
                return new PagedResult<TenantUserModel>();
            }
        }

        public async Task<TenantUserModel> GetTenantUserByIdAsync(string id, string accessToken)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(accessToken))
                return null;

            var path = _userApiSettings.Endpoints.TenantUserById.Replace("{id}", id);
            var url = $"{_userApiSettings.BaseUrl}{path}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("users/tenant/{Id} GET returned {StatusCode}: {Body}", id, response.StatusCode, body);
                    return null;
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseModel<TenantUserModel>>(body, DeserializeOptions);
                return parsed?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling users/tenant/{Id} GET", id);
                return null;
            }
        }

        public async Task<TenantUserModel> CreateTenantUserAsync(CreateTenantUserRequest request, string accessToken)
        {
            if (request == null || string.IsNullOrEmpty(accessToken))
                return null;

            var url = $"{_userApiSettings.BaseUrl}{_userApiSettings.Endpoints.TenantUsers}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var json = JsonSerializer.Serialize(request, SerializeOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("users/tenant POST returned {StatusCode}: {Body}", response.StatusCode, body);
                    return null;
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseModel<TenantUserModel>>(body, DeserializeOptions);
                return parsed?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling users/tenant POST");
                return null;
            }
        }

        public async Task<TenantUserModel> UpdateTenantUserAsync(string id, UpdateTenantUserRequest request, string accessToken)
        {
            if (string.IsNullOrEmpty(id) || request == null || string.IsNullOrEmpty(accessToken))
                return null;

            var path = _userApiSettings.Endpoints.TenantUserById.Replace("{id}", id);
            var url = $"{_userApiSettings.BaseUrl}{path}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var json = JsonSerializer.Serialize(request, SerializeOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
                var response = await httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("users/tenant/{Id} PATCH returned {StatusCode}: {Body}", id, response.StatusCode, body);
                    return null;
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseModel<TenantUserModel>>(body, DeserializeOptions);
                return parsed?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling users/tenant/{Id} PATCH", id);
                return null;
            }
        }

        public async Task<bool> DeleteTenantUserAsync(string id, string accessToken)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(accessToken))
                return false;

            var path = _userApiSettings.Endpoints.TenantUserById.Replace("{id}", id);
            var url = $"{_userApiSettings.BaseUrl}{path}";
            try
            {
                var httpClient = CreateAuthorizedClient(accessToken);
                var response = await httpClient.DeleteAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("users/tenant/{Id} DELETE returned {StatusCode}: {Body}", id, response.StatusCode, body);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling users/tenant/{Id} DELETE", id);
                return false;
            }
        }

        // ------------------------------------------------------------------

        private HttpClient CreateAuthorizedClient(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return httpClient;
        }
    }
}