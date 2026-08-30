using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.TenantUser
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        // ---- Read-only identity/summary (not part of the edit form) ----
        public string Email { get; set; }
        public string UserType { get; set; }
        public string RoleName { get; set; }
        public string TenantName { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // ---- Editable fields ----
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string ProfilePic { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
