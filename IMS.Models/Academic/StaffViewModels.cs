using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ViewModels
{
    public class StaffListItemViewModel
    {
        public Guid ST_Id { get; set; }
        public string ST_EmployeeCode { get; set; }
        public string ST_FirstName { get; set; }
        public string ST_LastName { get; set; }
        public string ST_Email { get; set; }
        public string ST_Phone { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public DateTime? ST_JoiningDate { get; set; }
        public string ST_Status { get; set; }
    }

    public class StaffIndexViewModel
    {
        public List<StaffListItemViewModel> StaffList { get; set; } = new();
        public string SearchTerm { get; set; }
        public Guid? BranchFilter { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class StaffFormViewModel
    {
        public Guid? ST_Id { get; set; }
        public Guid ST_BranchId { get; set; }
        public Guid? ST_DepartmentId { get; set; }
        public Guid? ST_DesignationId { get; set; }
        public string ST_EmployeeCode { get; set; }
        public string ST_FirstName { get; set; }
        public string ST_LastName { get; set; }
        public string ST_Email { get; set; }
        public string ST_Phone { get; set; }
        public DateTime? ST_JoiningDate { get; set; }
        public string ST_Status { get; set; } = "Active";

        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> DepartmentOptions { get; set; } = new();
        public List<SelectListItem> DesignationOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class StaffDetailsViewModel
    {
        public Guid ST_Id { get; set; }
        public string ST_EmployeeCode { get; set; }
        public string ST_FirstName { get; set; }
        public string ST_LastName { get; set; }
        public string ST_Email { get; set; }
        public string ST_Phone { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public DateTime? ST_JoiningDate { get; set; }
        public string ST_Status { get; set; }
        public DateTime ST_CreatedAt { get; set; }
        public DateTime ST_UpdatedAt { get; set; }
    }
}
