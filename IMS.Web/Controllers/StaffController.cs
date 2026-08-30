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
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
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

        public async Task<IActionResult> Index(string searchTerm, Guid? branchId, string status, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _staffService.GetStaffListAsync(
                CurrentTenantId, searchTerm, branchId, status, page, pageSize: 10);
            return View(vm);
        }

        public IActionResult Create()
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = new StaffFormViewModel();
            _staffService.PopulateDropdowns(vm);
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _staffService.GetStaffForEditAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _staffService.GetStaffDetailsAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(StaffFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _staffService.CreateStaffAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff for tenant {TenantId}", CurrentTenantId);
                return Json(new { success = false, message = "Something went wrong while saving the staff member." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(StaffFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _staffService.UpdateStaffAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff {StaffId} for tenant {TenantId}", model?.ST_Id, CurrentTenantId);
                return Json(new { success = false, message = "Something went wrong while saving the staff member." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            if (CurrentTenantId == Guid.Empty)
                return Json(new { success = false, message = "Your session has expired." });
            try
            {
                var result = await _staffService.DeleteStaffAsync(id, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff {StaffId} for tenant {TenantId}", id, CurrentTenantId);
                return Json(new { success = false, message = "Something went wrong while removing the staff member." });
            }
        }
    }
}
