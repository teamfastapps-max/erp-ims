using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class ExamsController : StudentPortalBaseController
    {
        private readonly IPortalService _portalService;

        public ExamsController(IPortalService portalService)
        {
            _portalService = portalService;
        }

        [HttpGet]
        public async Task<IActionResult> AdmitCard()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetAdmitCardAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MarkSheet()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetMarkSheetAsync(studentId, CurrentTenantId);
            return View(vm);
        }
    }
}
