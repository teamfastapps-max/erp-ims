using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models.Session;

namespace IMS.Services.Interfaces
{
    public interface IRedisService
    {
        Task SetUserAsync(string key, UserSessionModel data);
        Task<UserSessionModel> GetUserAsync(string key);
        Task<string> GetUserFieldAsync(string key, string field);
        Task RemoveUserAsync(string key);

        Task<HashSet<string>> GetPermissionSetAsync(string roleId);
        Task SetPermissionSetAsync(string roleId, IEnumerable<string> permissions, TimeSpan ttl);
    }
}
