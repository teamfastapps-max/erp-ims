using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Teacher
{
    public class Teacher
    {
        public Guid T_Id { get; set; }
        public Guid T_TenantId { get; set; }
        public Guid T_BranchId { get; set; }
        public string T_EmployeeCode { get; set; }
        public string T_Designation { get; set; }
        public string T_Department { get; set; }
        public DateTime? T_JoiningDate { get; set; }
        public string T_Qualification { get; set; }
        public int? T_ExperienceYears { get; set; }
        public string T_BloodGroup { get; set; }
        public string T_Status { get; set; }
        public bool T_IsActive { get; set; }        
        public DateTime T_CreatedAt { get; set; }
        public DateTime T_UpdatedAt { get; set; }
    }
}
