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
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _service;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(IEnrollmentService service, ILogger<EnrollmentController> logger)
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

        public async Task<IActionResult> Index(string searchTerm, Guid? academicYearId, Guid? courseId,
            Guid? batchId, string status, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetListAsync(CurrentTenantId, searchTerm, academicYearId, courseId, batchId, status, page, 10);
            return View(vm);
        }

        public IActionResult Create()
        {
            var vm = new EnrollmentFormViewModel();
            _service.PopulateDropdowns(vm);
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetForEditAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetDetailsAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEnrollment(EnrollmentFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _service.CreateAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error creating enrollment"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollment(EnrollmentFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _service.UpdateAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error updating enrollment"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEnrollment(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _service.DeleteAsync(id, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error deleting enrollment"); return Json(new { success = false, message = "Something went wrong." }); }
        }
    }
}
