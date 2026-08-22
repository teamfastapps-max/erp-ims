using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace IMS.Models.ViewModels
{
    public class StudentListItemViewModel
    {
        public Guid S_Id { get; set; }
        public string S_StudentCode { get; set; }
        public string S_AdmissionNumber { get; set; }
        public string FullName { get; set; }
        public string S_Gender { get; set; }
        public string BranchName { get; set; }
        public DateTime? S_AdmissionDate { get; set; }
        public string S_Status { get; set; }
    }

    public class StudentIndexViewModel
    {
        public List<StudentListItemViewModel> Students { get; set; } = new();

        // Filters
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public Guid? BranchFilter { get; set; }

        // Paging
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Dropdown sources
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class StudentFormViewModel
    {
        public Guid? S_Id { get; set; } // null on Create

        [Required(ErrorMessage = "Branch is required")]
        [Display(Name = "Branch")]
        public Guid S_BranchId { get; set; }

        [Display(Name = "Student Code")]
        [StringLength(50)]
        public string S_StudentCode { get; set; } // auto-generated if left blank

        [Required(ErrorMessage = "Admission number is required")]
        [StringLength(50)]
        [Display(Name = "Admission Number")]
        public string S_AdmissionNumber { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string S_FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Middle Name")]
        public string S_MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string S_LastName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? S_DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string S_Gender { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email")]
        [StringLength(255)]
        public string S_Email { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number")]
        [StringLength(30)]
        [Display(Name = "Phone")]
        public string S_Phone { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Admission Date")]
        public DateTime? S_AdmissionDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string S_Status { get; set; } = "Admitted";

        // Dropdown sources (populated by controller before rendering the view)
        public List<SelectListItem> BranchOptions { get; set; } = new();
        public List<SelectListItem> GenderOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class StudentDetailsViewModel
    {
        public Guid S_Id { get; set; }
        public string S_StudentCode { get; set; }
        public string S_AdmissionNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? S_DateOfBirth { get; set; }
        public string S_Gender { get; set; }
        public string S_Email { get; set; }
        public string S_Phone { get; set; }
        public string BranchName { get; set; }
        public DateTime? S_AdmissionDate { get; set; }
        public string S_Status { get; set; }
        public DateTime S_CreatedAt { get; set; }
        public DateTime S_UpdatedAt { get; set; }
    }
}
