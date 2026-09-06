using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class ExamsController : StudentPortalBaseController
    {
        private readonly IStuddentPortalService _StuddentPortalService;

        public ExamsController(IStuddentPortalService StuddentPortalService)
        {
            _StuddentPortalService = StuddentPortalService;
        }

        [HttpGet]
        public async Task<IActionResult> AdmitCard()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetAdmitCardAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MarkSheet()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetMarkSheetAsync(studentId, CurrentTenantId);
            return View(vm);
        }
    }
}
