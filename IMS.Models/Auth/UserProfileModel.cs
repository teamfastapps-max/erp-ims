using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Auth
{
    public class UserProfileModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TenantId { get; set; }
        public string UserType { get; set; }
        public string CustomRoleId { get; set; }
        public string CustomRoleName { get; set; }
        public List<string> CustomRolePermissions { get; set; } = new();
        public TenantDetailsModel TenantDetails { get; set; }
        public string KeycloakId { get; set; }
        public string Id { get; set; }
    }

    public class TenantDetailsModel
    {
        public string Name { get; set; }
        public string TenantId { get; set; }
    }
}
