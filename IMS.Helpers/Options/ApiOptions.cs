using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Helpers.Options
{
    public class ApiOptions
    {
        public const string SectionName = "ApiOptions";
        public UserApiSettings UserApi { get; set; }
        public SystemApiSettings SystemApi { get; set; }
    }

    public class UserApiSettings
    {
        public string BaseUrl { get; set; }
        public UserApiEndpoints Endpoints { get; set; }
    }

    public class UserApiEndpoints
    {
        public string MyProfile { get; set; }
        public string UpdateMyProfile { get; set; }
        public string TenantRoles { get; set; }     
        public string TenantUsers { get; set; }     
        public string TenantUserById { get; set; }    
    }

    public class SystemApiSettings
    {
        public string BaseUrl { get; set; }
        public SystemApiEndpoints Endpoints { get; set; }
    }

    public class SystemApiEndpoints
    {
        public string PermissionsSync { get; set; }
        public string RolePermissions { get; set; }
    }
}
