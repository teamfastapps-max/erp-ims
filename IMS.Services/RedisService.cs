using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models.Session;
using IMS.Services.Interfaces;

namespace IMS.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisService> _logger;
        private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);
        private const string UserKeyPrefix = "IMS:users:";
        private const string PermissionKeyPrefix = "permissions:role:";
        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task SetUserAsync(string key, UserSessionModel data)
        {
            var redisKey = UserKeyPrefix + key;
            try
            {
                var entries = new HashEntry[]
                {
                    new("email", data.Email ?? ""),
                    new("first_name", data.FirstName ?? ""),
                    new("last_name", data.LastName ?? ""),
                    new("tenant_id", data.TenantId ?? ""),
                    new("tenant_name", data.TenantName ?? ""),
                    new("user_type", data.UserType ?? ""),
                    new("custom_role_id", data.CustomRoleId ?? ""),
                    new("custom_role_name", data.CustomRoleName ?? ""),
                    new("permissions", data.Permissions ?? ""),
                    new("keycloak_id", data.KeycloakId ?? ""),
                    new("user_id", data.UserId ?? ""),
                    new("preferred_username", data.PreferredUsername ?? "")
                };

                await _db.HashSetAsync(redisKey, entries);
                await _db.KeyExpireAsync(redisKey, SessionTtl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store user session in Redis for key {Key}", redisKey);
            }
        }

        public async Task<UserSessionModel> GetUserAsync(string key)
        {
            var redisKey = UserKeyPrefix + key;
            try
            {
                var entries = await _db.HashGetAllAsync(redisKey);
                if (entries.Length == 0) return null;

                var dict = entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());

                return new UserSessionModel
                {
                    Email = dict.GetValueOrDefault("email"),
                    FirstName = dict.GetValueOrDefault("first_name"),
                    LastName = dict.GetValueOrDefault("last_name"),
                    TenantId = dict.GetValueOrDefault("tenant_id"),
                    TenantName = dict.GetValueOrDefault("tenant_name"),
                    UserType = dict.GetValueOrDefault("user_type"),
                    CustomRoleId = dict.GetValueOrDefault("custom_role_id"),
                    CustomRoleName = dict.GetValueOrDefault("custom_role_name"),
                    Permissions = dict.GetValueOrDefault("permissions"),
                    KeycloakId = dict.GetValueOrDefault("keycloak_id"),
                    UserId = dict.GetValueOrDefault("user_id"),
                    PreferredUsername = dict.GetValueOrDefault("preferred_username")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read user session from Redis for key {Key}", redisKey);
                return null;
            }
        }

        public async Task<string> GetUserFieldAsync(string key, string field)
        {
            var redisKey = UserKeyPrefix + key;
            try
            {
                var value = await _db.HashGetAsync(redisKey, field);
                return value.IsNullOrEmpty ? null : value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read field {Field} from Redis for key {Key}", field, redisKey);
                return null;
            }
        }

        public async Task RemoveUserAsync(string key)
        {
            var redisKey = UserKeyPrefix + key;
            try
            {
                await _db.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove Redis key {Key}", redisKey);
            }
        }

        public async Task<HashSet<string>> GetPermissionSetAsync(string roleId)
        {
            try
            {
                var members = await _db.SetMembersAsync(PermissionKeyPrefix + roleId);
                return members.Select(m => m.ToString()).ToHashSet();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read permission set for role {RoleId}", roleId);
                return new HashSet<string>();
            }
        }

        public async Task SetPermissionSetAsync(string roleId, IEnumerable<string> permissions, TimeSpan ttl)
        {
            try
            {
                var key = PermissionKeyPrefix + roleId;
                var values = permissions.Select(p => (RedisValue)p).ToArray();
                if (values.Length == 0) return;

                await _db.SetAddAsync(key, values);
                await _db.KeyExpireAsync(key, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to populate permission set for role {RoleId}", roleId);
            }
        }
       
    }
   
}
