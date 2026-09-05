using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class DashboardController : StudentPortalBaseController
    {
        private readonly IPortalService _portalService;

        public DashboardController(IPortalService portalService)
        {
            _portalService = portalService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty)
            {
                return View("NoStudentLinked");
            }

            var vm = await _portalService.GetDashboardAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetStudentProfileAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> IdCard()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetStudentIdCardAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GuardianIdCard()
        {
            var guardianId = CurrentUserId;
            var vm = await _portalService.GetGuardianIdCardAsync(guardianId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Notices()
        {
            var notices = await _portalService.GetNoticesAsync(CurrentTenantId);
            return View(notices);
        }
    }
}
