using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class FinanceController : StudentPortalBaseController
    {
        private readonly IStuddentPortalService _StuddentPortalService;

        public FinanceController(IStuddentPortalService StuddentPortalService)
        {
            _StuddentPortalService = StuddentPortalService;
        }

        [HttpGet]
        public async Task<IActionResult> Transactions()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _StuddentPortalService.GetFeeTransactionsAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Payment ID is required.");

            var vm = await _StuddentPortalService.GetReceiptDetailsAsync(id, CurrentTenantId);
            if (vm == null) return NotFound("Receipt not found or inaccessible.");

            return View(vm);
        }
    }
}
