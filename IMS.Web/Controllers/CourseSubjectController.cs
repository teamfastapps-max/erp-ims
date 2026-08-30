using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class CourseSubjectController : Controller
    {
        private readonly ICourseSubjectService _service;
        private readonly ILogger<CourseSubjectController> _logger;

        public CourseSubjectController(ICourseSubjectService service, ILogger<CourseSubjectController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private Guid CurrentTenantId
        {
            get
            {
                var raw = User.FindFirst("tenant_id")?.Value;
                return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
            }
        }

        public async Task<IActionResult> Index(Guid? courseId)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetListAsync(CurrentTenantId, courseId);
            return View(vm);
        }

        public IActionResult Create(Guid? courseId)
        {
            var vm = new CourseSubjectFormViewModel { CS_CourseId = courseId ?? Guid.Empty };
            _service.PopulateDropdowns(vm, CurrentTenantId);
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid courseId, Guid subjectId)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetForEditAsync(courseId, subjectId, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseSubject(CourseSubjectFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _service.CreateAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course subject");
                return Json(new { success = false, message = "Something went wrong." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourseSubject(CourseSubjectFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _service.UpdateAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course subject");
                return Json(new { success = false, message = "Something went wrong." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseSubject(Guid courseId, Guid subjectId)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _service.DeleteAsync(courseId, subjectId, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course subject");
                return Json(new { success = false, message = "Something went wrong." });
            }
        }
    }
}
