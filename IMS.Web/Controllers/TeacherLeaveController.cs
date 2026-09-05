using IMS.Helpers.Constants;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;
using IMS.Web.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class TeacherLeaveController : Controller
    {
        private readonly ITeacherLeaveService _leaveService;
        private readonly ILogger<TeacherLeaveController> _logger;

        public TeacherLeaveController(ITeacherLeaveService leaveService, ILogger<TeacherLeaveController> logger)
        {
            _leaveService = leaveService;
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

        private Guid CurrentUserId
        {
            get
            {
                var raw = User.FindFirst("user_id")?.Value;
                return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
            }
        }

        // ============================================================
        // Admin: all teachers' requests
        // ============================================================

        [Permission(Permissions.ApproveTeacherLeave)]
        public async Task<IActionResult> Index(string status, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();

            var vm = await _leaveService.GetAllForAdminAsync(CurrentTenantId, status, page, pageSize: 10);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApproveTeacherLeave)]
        public async Task<IActionResult> ApproveTeacherLeave(Guid id)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var result = await _leaveService.ApproveAsync(CurrentTenantId, id, CurrentUserId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving leave {LeaveId}", id);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApproveTeacherLeave)]
        public async Task<IActionResult> RejectTeacherLeave(Guid id, string rejectionReason)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var result = await _leaveService.RejectAsync(CurrentTenantId, id, CurrentUserId, rejectionReason);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting leave {LeaveId}", id);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }

        // ============================================================
        // Self-service: current user's own requests
        // ============================================================

        [Permission(Permissions.ApplyTeacherLeave)]
        public async Task<IActionResult> MyLeave(int page = 1)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty) return Unauthorized();

            var vm = await _leaveService.GetMyLeaveAsync(CurrentTenantId, CurrentUserId, page, pageSize: 10);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApplyTeacherLeave)]
        public async Task<IActionResult> ApplyTeacherLeave(TeacherLeaveApplyViewModel model)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }
                var result = await _leaveService.ApplyAsync(CurrentTenantId, CurrentUserId, accessToken, model);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying for leave (teacher {TeacherId})", CurrentUserId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApplyTeacherLeave)]
        public async Task<IActionResult> CancelTeacherLeave(Guid id)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var result = await _leaveService.CancelAsync(CurrentTenantId, id, CurrentUserId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling leave {LeaveId}", id);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApplyTeacherLeave)]
        public async Task<IActionResult> EditTeacherLeave(Guid id, TeacherLeaveApplyViewModel model)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var result = await _leaveService.UpdateAsync(CurrentTenantId, id, CurrentUserId, model);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing leave {LeaveId}", id);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
    }
}