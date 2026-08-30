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
    public class FeesController : Controller
    {
        private readonly IFeeService _feeService;
        private readonly ILogger<FeesController> _logger;

        public FeesController(IFeeService feeService, ILogger<FeesController> logger)
        {
            _feeService = feeService;
            _logger = logger;
        }

        private Guid CurrentTenantId
        {
            get { var raw = User.FindFirst("tenant_id")?.Value; return Guid.TryParse(raw, out var id) ? id : Guid.Empty; }
        }

        private Guid CurrentUserId
        {
            get { var raw = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value; return Guid.TryParse(raw, out var id) ? id : Guid.Empty; }
        }

        public async Task<IActionResult> Index(string searchTerm, Guid? academicYearId, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _feeService.GetFeeStructureListAsync(CurrentTenantId, searchTerm, academicYearId, page, 10);
            return View(vm);
        }

        public IActionResult Create()
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = new FeeStructureFormViewModel();
            _feeService.PopulateFeeStructureDropdowns(vm, CurrentTenantId);
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _feeService.GetFeeStructureForEditAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeeStructure(FeeStructureFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _feeService.CreateFeeStructureAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error creating fee structure"); return Json(new { success = false, message = "Error saving." }); }
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFeeStructure(FeeStructureFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _feeService.UpdateFeeStructureAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error updating fee structure"); return Json(new { success = false, message = "Error saving." }); }
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeeStructure(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            var result = await _feeService.DeleteFeeStructureAsync(id, CurrentTenantId);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ---- Invoices ----
        public async Task<IActionResult> Invoices(string searchTerm, string status, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _feeService.GetFeeInvoiceListAsync(CurrentTenantId, searchTerm, status, page, 10);
            return View(vm);
        }

        public IActionResult CreateInvoice()
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = new FeeInvoiceFormViewModel();
            _feeService.PopulateFeeInvoiceDropdowns(vm, CurrentTenantId);
            return View(vm);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInvoice(FeeInvoiceFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _feeService.CreateFeeInvoiceAsync(model, CurrentTenantId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error creating invoice"); return Json(new { success = false, message = "Error saving." }); }
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            var result = await _feeService.DeleteFeeInvoiceAsync(id, CurrentTenantId);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ---- Payments ----
        public async Task<IActionResult> Payments(string searchTerm, string status, int page = 1)
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = await _feeService.GetPaymentListAsync(CurrentTenantId, searchTerm, status, page, 10);
            return View(vm);
        }

        public IActionResult CreatePayment()
        {
            if (CurrentTenantId == Guid.Empty) return Unauthorized();
            var vm = new PaymentFormViewModel();
            _feeService.PopulatePaymentDropdowns(vm, CurrentTenantId);
            return View(vm);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(PaymentFormViewModel model)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            try
            {
                var result = await _feeService.CreatePaymentAsync(model, CurrentTenantId, CurrentUserId);
                return Json(new { success = result.Success, message = result.Message, id = result.Id });
            }
            catch (Exception ex) { _logger.LogError(ex, "Error creating payment"); return Json(new { success = false, message = "Error saving." }); }
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            if (CurrentTenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });
            var result = await _feeService.DeletePaymentAsync(id, CurrentTenantId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
