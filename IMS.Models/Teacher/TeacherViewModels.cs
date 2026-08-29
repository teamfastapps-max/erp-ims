using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Teacher
{
    public class TeacherListItemViewModel
    {
        public Guid T_Id { get; set; }
        public string T_EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BranchName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string T_Status { get; set; }
    }

    public class TeacherIndexViewModel
    {
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public Guid? BranchFilter { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<TeacherListItemViewModel> Teachers { get; set; } = new();
        public IEnumerable<SelectListItem> BranchOptions { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
    }

    public class TeacherDetailsViewModel
    {
        public Guid T_Id { get; set; }
        public string T_EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullAddress { get; set; }
        public string BranchName { get; set; }
        public string RoleName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public DateTime? T_JoiningDate { get; set; }
        public string T_Qualification { get; set; }
        public int? T_ExperienceYears { get; set; }
        public string T_BloodGroup { get; set; }
        public string T_Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class TeacherFormViewModel
    {
        public Guid? T_Id { get; set; } // null on Create, populated on Edit

        // ---- Identity fields (posted to the Users API) ----
        public string Email { get; set; }
        public string Password { get; set; } // Create only; blank => auto-generated temp password
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string CustomRoleId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // ---- Local profile fields (Teachers_T) ----
        public Guid T_BranchId { get; set; }
        public string T_EmployeeCode { get; set; }
        public string T_Designation { get; set; }
        public string T_Department { get; set; }
        public DateTime? T_JoiningDate { get; set; }
        public string T_Qualification { get; set; }
        public int? T_ExperienceYears { get; set; }
        public string T_BloodGroup { get; set; }
        public string T_Status { get; set; }

        // ---- Dropdown sources ----
        public IEnumerable<SelectListItem> BranchOptions { get; set; }
        public IEnumerable<SelectListItem> RoleOptions { get; set; }
        public IEnumerable<SelectListItem> DesignationOptions { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public IEnumerable<SelectListItem> BloodGroupOptions { get; set; }
    }
}
