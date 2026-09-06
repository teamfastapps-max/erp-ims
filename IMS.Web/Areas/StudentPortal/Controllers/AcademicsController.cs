using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class AcademicsController : StudentPortalBaseController
    {
        private readonly IStuddentPortalService _StuddentPortalService;

        public AcademicsController(IStuddentPortalService StuddentPortalService)
        {
            _StuddentPortalService = StuddentPortalService;
        }

        [HttpGet]
        public async Task<IActionResult> Attendance(int? month, int? year)
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetAttendanceCalendarAsync(studentId, CurrentTenantId, month, year);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Timetable()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetTimetableAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Syllabus()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetSyllabusAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ClassDetails()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetClassDetailsAsync(studentId, CurrentTenantId);
            return View(vm);
        }
    }
}
