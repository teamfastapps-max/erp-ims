using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services.Interfaces
{
    public interface IRoleApiService
    {
        Task<List<string>> GetRolePermissionsAsync(string roleId, string accessToken);
    }
}
