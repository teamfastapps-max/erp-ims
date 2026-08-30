using System;

namespace IMS.Models.Entities
{
    public class Staff
    {
        public Guid ST_Id { get; set; }
        public Guid ST_TenantId { get; set; }
        public Guid ST_BranchId { get; set; }
        public Guid? ST_UserId { get; set; }
        public Guid? ST_DepartmentId { get; set; }
        public Guid? ST_DesignationId { get; set; }
        public string ST_EmployeeCode { get; set; }
        public string ST_FirstName { get; set; }
        public string ST_LastName { get; set; }
        public string ST_Email { get; set; }
        public string ST_Phone { get; set; }
        public DateTime? ST_JoiningDate { get; set; }
        public string ST_Status { get; set; }
        public DateTime ST_CreatedAt { get; set; }
        public DateTime ST_UpdatedAt { get; set; }
        public DateTime? ST_DeletedAt { get; set; }

        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
    }
}
