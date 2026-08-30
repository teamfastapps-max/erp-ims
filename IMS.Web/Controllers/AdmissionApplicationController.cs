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
    public class AdmissionApplicationController : Controller
    {
        private readonly IAdmissionApplicationService _service;
        private readonly ILogger<AdmissionApplicationController> _logger;
        public AdmissionApplicationController(IAdmissionApplicationService service, ILogger<AdmissionApplicationController> logger) { _service = service; _logger = logger; }

        private Guid CurrentTenantId
        {
            get { var raw = User.FindFirst("tenant_id")?.Value; return Guid.TryParse(raw, out var id) ? id : Guid.Empty; }
        }

        private Guid? CurrentUserId
        {
            get { var raw = User.FindFirst("user_id")?.Value; return Guid.TryParse(raw, out var id) ? id : (Guid?)null; }
        }

        public async Task<IActionResult> Index(string searchTerm, Guid? branchId, Guid? courseId,
            Guid? academicYearId, string status, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetListAsync(CurrentTenantId, searchTerm, branchId, courseId, academicYearId, status, page, 10);
            return View(vm);
        }

        public IActionResult Create()
        {
            var vm = new AdmissionApplicationFormViewModel();
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
        public async Task<IActionResult> AddApplication(AdmissionApplicationFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try { var r = await _service.CreateAsync(model, CurrentTenantId); return Json(new { success = r.Success, message = r.Message, id = r.Id }); }
            catch (Exception ex) { _logger.LogError(ex, "Error creating application"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApplication(AdmissionApplicationFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try { var r = await _service.UpdateAsync(model, CurrentTenantId); return Json(new { success = r.Success, message = r.Message, id = r.Id }); }
            catch (Exception ex) { _logger.LogError(ex, "Error updating application"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try { var r = await _service.DeleteAsync(id, CurrentTenantId); return Json(new { success = r.Success, message = r.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Error deleting application"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(AdmissionReviewViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            var userId = CurrentUserId ?? Guid.Empty;
            try { var r = await _service.ReviewAsync(model, CurrentTenantId, userId); return Json(new { success = r.Success, message = r.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Error reviewing application"); return Json(new { success = false, message = "Something went wrong." }); }
        }
    }
}
