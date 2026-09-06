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
    public class TransferCertificateController : Controller
    {
        private const int PageSize = 15;
        private readonly ITransferCertificateService _tcService;
        private readonly ILogger<TransferCertificateController> _logger;

        public TransferCertificateController(ITransferCertificateService tcService, ILogger<TransferCertificateController> logger)
        {
            _tcService = tcService;
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

        [HttpGet]
        public async Task<IActionResult> Index(string? status = null, string? search = null, int page = 1)
        {
            var tenantId = CurrentTenantId;
            if (tenantId == Guid.Empty) return Unauthorized();

            if (page < 1) page = 1;

            var (list, totalCount) = await _tcService.GetPagedAsync(tenantId, status, search, page, PageSize);

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
        public async Task<IActionResult> Review(Guid tcId, bool libraryClearance, bool feeClearance, bool labClearance, string status, string? remarks)
        {
            var tenantId = CurrentTenantId;
            if (tenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });

            var result = await _tcService.ReviewAsync(tcId, tenantId, libraryClearance, feeClearance, labClearance, status, remarks);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid tcId)
        {
            var tenantId = CurrentTenantId;
            if (tenantId == Guid.Empty) return Json(new { success = false, message = "Session expired." });

            var result = await _tcService.DeleteAsync(tcId, tenantId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Print(Guid id)
        {
            var tenantId = CurrentTenantId;
            if (tenantId == Guid.Empty) return Unauthorized();

            var vm = await _tcService.GetByIdAsync(id, tenantId);
            if (vm == null) return NotFound("Transfer Certificate not found.");

            return View(vm);
        }
    }
}
