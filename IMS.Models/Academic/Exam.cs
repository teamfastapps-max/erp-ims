using System;

namespace IMS.Models.Entities
{
    public class Exam
    {
        public Guid EX_Id { get; set; }
        public Guid EX_TenantId { get; set; }
        public Guid EX_AcademicYearId { get; set; }
        public Guid EX_CourseId { get; set; }
        public Guid EX_BatchId { get; set; }
        public Guid EX_ExamTypeId { get; set; }
        public string EX_Name { get; set; }
        public string EX_Code { get; set; }
        public DateTime EX_StartDate { get; set; }
        public DateTime EX_EndDate { get; set; }
        public string EX_Status { get; set; }
        public DateTime EX_CreatedAt { get; set; }
        public DateTime EX_UpdatedAt { get; set; }

        public string AcademicYearName { get; set; }
        public string CourseName { get; set; }
        public string BatchName { get; set; }
        public string ExamTypeName { get; set; }
    }

    public class ExamSubject
    {
        public Guid ES_Id { get; set; }
        public Guid ES_ExamId { get; set; }
        public Guid ES_SubjectId { get; set; }
        public decimal ES_MaxMarks { get; set; }
        public decimal ES_PassMarks { get; set; }
        public decimal? ES_Weightage { get; set; }

        public string SubjectName { get; set; }
    }

    public class ExamSchedule
    {
        public Guid ESC_Id { get; set; }
        public Guid ESC_ExamSubjectId { get; set; }
        public DateTime ESC_ExamDate { get; set; }
        public TimeSpan ESC_StartTime { get; set; }
        public TimeSpan ESC_EndTime { get; set; }
        public Guid? ESC_ClassroomId { get; set; }

        public string ClassroomName { get; set; }
    }

    public class Mark
    {
        public Guid M_Id { get; set; }
        public Guid M_ExamSubjectId { get; set; }
        public Guid M_StudentId { get; set; }
        public decimal M_MarksObtained { get; set; }
        public decimal? M_Percentage { get; set; }
        public Guid? M_GradeScaleItemId { get; set; }
        public string M_Remarks { get; set; }
        public DateTime M_CreatedAt { get; set; }
        public DateTime M_UpdatedAt { get; set; }

        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string Grade { get; set; }
    }

    public class Result
    {
        public Guid R_Id { get; set; }
        public Guid R_ExamId { get; set; }
        public Guid R_StudentId { get; set; }
        public decimal R_TotalMarks { get; set; }
        public decimal R_MarksObtained { get; set; }
        public decimal R_Percentage { get; set; }
        public string R_Grade { get; set; }
        public string R_ResultStatus { get; set; }
        public string R_Remarks { get; set; }
        public DateTime? R_PublishedAt { get; set; }
        public DateTime R_CreatedAt { get; set; }
        public DateTime R_UpdatedAt { get; set; }

        public string StudentName { get; set; }
        public string StudentCode { get; set; }
    }

    public class ExamType
    {
        public Guid ET_Id { get; set; }
        public Guid ET_TenantId { get; set; }
        public string ET_Name { get; set; }
        public string ET_Code { get; set; }
        public string ET_Description { get; set; }
        public decimal? ET_WeightagePercentage { get; set; }
    }
}
