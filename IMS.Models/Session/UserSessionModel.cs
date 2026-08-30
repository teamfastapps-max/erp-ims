using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Session
{
    public class UserSessionModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string UserType { get; set; }
        public string CustomRoleId { get; set; }
        public string CustomRoleName { get; set; }
        public string Permissions { get; set; }   
        public string KeycloakId { get; set; }
        public string UserId { get; set; }
        public string PreferredUsername { get; set; }
    }
}
