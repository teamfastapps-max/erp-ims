using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IMS.Helpers.Options;
using IMS.Models.Auth;
using IMS.Models.Common;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class UserApiService : IUserApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserApiService> _logger;
        private readonly UserApiSettings _userApiSettings;
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
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization =new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("my-profile API returned {StatusCode}: {Body}", response.StatusCode, body);
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<ApiResponseModel<UserProfileModel>>(body, options);

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
    }
}
