using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.TenantUser
{
    public class LocationModel
    {
        public string Label { get; set; }
        public string Type { get; set; } = "Point";
        public List<double> Coordinates { get; set; } = new List<double>();
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Landmark { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string FormattedAddress { get; set; }
        public string PlaceId { get; set; }
    }

    /// <summary>
    /// A single tenant user as returned by GET/POST/PATCH users/tenant endpoints.
    /// This is the identity/account record — school-specific fields (designation,
    /// joining date, etc.) live in the local Teachers_T table, keyed by Id.
    /// </summary>
    public class TenantUserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string TenantId { get; set; }
        public string UserType { get; set; }
        public string CustomRoleId { get; set; }
        public string CustomRoleName { get; set; }
        public List<string> CustomRolePermissions { get; set; } = new List<string>();
        public object TenantDetails { get; set; }
        public LocationModel Location { get; set; }
        public string ProfilePic { get; set; }
        public string KeycloakId { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool Deleted { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    /// <summary>
    /// Pagination metadata block ("meta") on the paged users/tenant list response.
    /// </summary>
    public class PagedMeta
    {
        public int TotalDocs { get; set; }
        public int Skip { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int Limit { get; set; }
        public bool HasPrevPage { get; set; }
        public bool HasNextPage { get; set; }
        public int? PrevPage { get; set; }
        public int? NextPage { get; set; }
    }

    /// <summary>
    /// The "data" object on GET users/tenant — { meta, docs[] }.
    /// </summary>
    public class PagedResult<T>
    {
        public PagedMeta Meta { get; set; }
        public List<T> Docs { get; set; } = new List<T>();
    }

    /// <summary>
    /// POST users/tenant request body.
    /// </summary>
    public class CreateTenantUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserType { get; set; } = "TENANT_USER";
        public string CustomRoleId { get; set; }
        public LocationModel Location { get; set; }
    }

    /// <summary>
    /// PATCH users/tenant/{id} request body.
    /// </summary>
    public class UpdateTenantUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public LocationModel Location { get; set; }
        public string UserType { get; set; }
        public string CustomRoleId { get; set; }
    }

    /// <summary>
    /// A tenant role as returned by GET roles/tenant.
    /// NOTE: the response shape for this endpoint wasn't included in the examples
    /// given — this is inferred from customRoleId/customRoleName usage elsewhere.
    /// Verify field names against an actual response and adjust if they differ.
    /// </summary>
    public class TenantRoleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
    public class UpdateMyProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public LocationModel Location { get; set; }
        public string ProfilePic { get; set; }
    }
}
