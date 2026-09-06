using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IMS.Helpers.Constants;
using IMS.Models.Portal;
using IMS.Services.Interfaces;
using IMS.Web.Authorization;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class StudentLeaveController : Controller
    {
        private const int PageSize = 15;
        private readonly IStudentLeaveService _leaveService;
        private readonly ILogger<StudentLeaveController> _logger;

        public StudentLeaveController(IStudentLeaveService leaveService, ILogger<StudentLeaveController> logger)
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

        [HttpGet]
        [Permission(Permissions.ViewStudentLeave)]
        public async Task<IActionResult> Index(string? status = null, string? search = null, int page = 1)
        {
            var tenantId = CurrentTenantId;
            if (tenantId == Guid.Empty) return Unauthorized();

            if (page < 1) page = 1;

            var (list, totalCount) = await _leaveService.GetPagedAsync(tenantId, status, search, page, PageSize);

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApproveStudentLeave)]
        public async Task<IActionResult> Review(Guid leaveId, string status, string? rejectionReason)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            if (tenantId == Guid.Empty || userId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            var result = await _leaveService.ReviewAsync(leaveId, tenantId, userId, status, rejectionReason);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.ApproveStudentLeave)]
        public async Task<IActionResult> Delete(Guid leaveId)
        {
            var tenantId = CurrentTenantId;
            if (tenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired. Please sign in again." });

            var result = await _leaveService.DeleteAsync(leaveId, tenantId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}