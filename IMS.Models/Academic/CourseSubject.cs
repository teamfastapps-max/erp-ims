using System;

namespace IMS.Models.Entities
{
    public class CourseSubject
    {
        public Guid CS_CourseId { get; set; }
        public Guid CS_SubjectId { get; set; }
        public int CS_SequenceNo { get; set; }
        public bool CS_IsMandatory { get; set; }
        public decimal? CS_MaxMarks { get; set; }
        public decimal? CS_PassMarks { get; set; }

        public string CourseName { get; set; }
        public string SubjectName { get; set; }
    }
}
