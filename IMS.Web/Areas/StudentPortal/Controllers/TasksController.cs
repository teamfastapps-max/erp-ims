using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class TasksController : StudentPortalBaseController
    {
        private readonly IStuddentPortalService _StuddentPortalService;

        public TasksController(IStuddentPortalService StuddentPortalService)
        {
            _StuddentPortalService = StuddentPortalService;
        }

        [HttpGet]
        public async Task<IActionResult> HomeTasks()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetHomeTasksAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitHomeTask(Guid taskId, string? content, string? attachmentUrl)
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return BadRequest(new { success = false, message = "No active student selected." });

            var success = await _StuddentPortalService.SubmitHomeTaskAsync(taskId, studentId, content, attachmentUrl);
            if (success)
            {
                return Json(new { success = true, message = "Home task submitted successfully." });
            }

            return Json(new { success = false, message = "Failed to submit assignment. Please try again." });
        }

        [HttpGet]
        public async Task<IActionResult> MockTests()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetMockTestsAsync(studentId, CurrentTenantId);
            return View(vm);
        }
    }
}
