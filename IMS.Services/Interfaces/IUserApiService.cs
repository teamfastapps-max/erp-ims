using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Models.Auth;

namespace IMS.Services.Interfaces
{
    public interface IUserApiService
    {
        Task<UserProfileModel> GetMyProfileAsync(string accessToken);
    }
}
