using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class DashboardController : StudentPortalBaseController
    {
        private readonly IStuddentPortalService _StuddentPortalService;

        public DashboardController(IStuddentPortalService StuddentPortalService)
        {
            _StuddentPortalService = StuddentPortalService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty)
            {
                return View("NoStudentLinked");
            }

            var vm = await _StuddentPortalService.GetDashboardAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetStudentProfileAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> IdCard()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetStudentIdCardAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GuardianIdCard()
        {
            Guid? guardianId = CurrentUserType == "GUARDIAN" ? CurrentUserId : null;
            Guid? studentId = CurrentStudentId;
            var vm = await _StuddentPortalService.GetGuardianIdCardAsync(guardianId, studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Notices()
        {
            var notices = await _StuddentPortalService.GetNoticesAsync(CurrentTenantId);
            return View(notices);
        }
    }
}
