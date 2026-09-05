using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class AcademicsController : StudentPortalBaseController
    {
        private readonly IPortalService _portalService;

        public AcademicsController(IPortalService portalService)
        {
            _portalService = portalService;
        }

        [HttpGet]
        public async Task<IActionResult> Attendance(int? month, int? year)
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetAttendanceCalendarAsync(studentId, CurrentTenantId, month, year);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Timetable()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetTimetableAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Syllabus()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetSyllabusAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ClassDetails()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetClassDetailsAsync(studentId, CurrentTenantId);
            return View(vm);
        }
    }
}
