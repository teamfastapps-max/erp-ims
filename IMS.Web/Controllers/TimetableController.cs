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
    public class TimetableController : Controller
    {
        private readonly ITimetableService _service;
        private readonly ILogger<TimetableController> _logger;
        public TimetableController(ITimetableService service, ILogger<TimetableController> logger) { _service = service; _logger = logger; }

        private Guid CurrentTenantId
        {
            get { var raw = User.FindFirst("tenant_id")?.Value; return Guid.TryParse(raw, out var id) ? id : Guid.Empty; }
        }

        public async Task<IActionResult> Index(Guid? batchId, Guid? branchId)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetListAsync(CurrentTenantId, batchId, branchId);
            return View(vm);
        }

        public IActionResult Create(Guid? batchId)
        {
            var vm = new TimetableFormViewModel { TT_BatchId = batchId ?? Guid.Empty };
            _service.PopulateDropdowns(vm);
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetForEditAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            _service.PopulateDropdowns(vm);
            return View(vm);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _service.GetDetailsAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CheckConflict(Guid batchId, int dayOfWeek, string startTime, string endTime, Guid? excludeId)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { conflict = false });
            var hasConflict = await _service.CheckConflictAsync(CurrentTenantId, batchId, dayOfWeek, startTime, endTime, excludeId);
            return Json(new { conflict = hasConflict });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTimetable(TimetableFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try { var r = await _service.CreateAsync(model, CurrentTenantId); return Json(new { success = r.Success, message = r.Message, id = r.Id }); }
            catch (Exception ex) { _logger.LogError(ex, "Error creating timetable"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTimetable(TimetableFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try { var r = await _service.UpdateAsync(model, CurrentTenantId); return Json(new { success = r.Success, message = r.Message, id = r.Id }); }
            catch (Exception ex) { _logger.LogError(ex, "Error updating timetable"); return Json(new { success = false, message = "Something went wrong." }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimetable(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try { var r = await _service.DeleteAsync(id, CurrentTenantId); return Json(new { success = r.Success, message = r.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Error deleting timetable"); return Json(new { success = false, message = "Something went wrong." }); }
        }
    }
}
