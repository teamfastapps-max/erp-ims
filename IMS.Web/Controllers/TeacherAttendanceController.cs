using IMS.Helpers.Constants;
using IMS.Services.Interfaces;
using IMS.Web.Authorization;
using IMS.Web.Extensions; // GetAccessTokenAsync() extension
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class TeacherAttendanceController : Controller
    {
        private readonly ITeacherAttendanceService _attendanceService;
        private readonly ILogger<TeacherAttendanceController> _logger;

        public TeacherAttendanceController(ITeacherAttendanceService attendanceService, ILogger<TeacherAttendanceController> logger)
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

        private Guid CurrentUserId
        {
            get
            {
                var raw = User.FindFirst("user_id")?.Value;
                return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
            }
        }

        // ============================================================
        // Admin: bulk mark grid for a given date
        // ============================================================

        [Permission(Permissions.ManageTeacherAttendance)]
        public async Task<IActionResult> Index(DateTime? date)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var selectedDate = date ?? DateTime.Today;
            var vm = await _attendanceService.GetMarkGridAsync(CurrentTenantId, accessToken, selectedDate);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ManageTeacherAttendance)]
        public async Task<IActionResult> MarkTeacherAttendance(Guid teacherId, DateTime date, string status, string remarks)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var result = await _attendanceService.MarkTeacherAttendanceAsync(CurrentTenantId, teacherId, date, status, remarks, CurrentUserId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking attendance for teacher {TeacherId} on {Date}", teacherId, date);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }


        [Permission(Permissions.ViewOwnTeacherAttendance)]
        public async Task<IActionResult> TeacherAttendance(DateTime? fromDate, DateTime? toDate)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty) return Unauthorized();

            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            var vm = await _attendanceService.GetTeacherAttendanceAsync(CurrentTenantId, CurrentUserId, from, to);
            return View(vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.MarkOwnTeacherAttendance)]
        public async Task<IActionResult> MarkTeacherSelfAttendance(string status, string remarks)
        {
            if (CurrentTenantId == Guid.Empty || CurrentUserId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            try
            {
                var result = await _attendanceService.MarkTeacherSelfAttendanceAsync(CurrentTenantId, CurrentUserId, status, remarks);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error self-marking attendance for teacher {TeacherId}", CurrentUserId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
    }
}
