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
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
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

        public async Task<IActionResult> Index(string searchTerm, Guid? branchId, Guid? batchId, DateTime? date, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _attendanceService.GetSessionListAsync(
                CurrentTenantId, searchTerm, branchId, batchId, date, page, pageSize: 10);
            return View(vm);
        }

        public IActionResult Create()
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = new AttendanceSessionFormViewModel();
            _attendanceService.PopulateSessionDropdowns(vm);
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _attendanceService.GetSessionForEditAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        public async Task<IActionResult> MarkAttendance(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _attendanceService.GetMarkAttendanceAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSession(AttendanceSessionFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _attendanceService.CreateSessionAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attendance session for tenant {TenantId}", CurrentTenantId);
                return Json(new { success = false, message = "Something went wrong while saving the session." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(AttendanceSessionFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _attendanceService.UpdateSessionAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId} for tenant {TenantId}", model?.AS_Id, CurrentTenantId);
                return Json(new { success = false, message = "Something went wrong while saving the session." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(Guid id)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _attendanceService.DeleteSessionAsync(id, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId} for tenant {TenantId}", id, CurrentTenantId);
                return Json(new { success = false, message = "Something went wrong while removing the session." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(AttendanceRecordSaveViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _attendanceService.SaveAttendanceAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving attendance for session {SessionId}", model?.SessionId);
                return Json(new { success = false, message = "Something went wrong while saving attendance." });
            }
        }
    }
}
