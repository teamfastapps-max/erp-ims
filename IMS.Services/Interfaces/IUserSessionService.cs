using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services.Interfaces
{
    public interface IUserSessionService
    {
        Task StoreUserSessionAsync(ClaimsPrincipal principal, string accessToken);
        Task RemoveUserSessionAsync(string username);
    }
}
