using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Models.Portal;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    public class RequestsController : StudentPortalBaseController
    {
        private readonly IPortalService _portalService;

        public RequestsController(IPortalService portalService)
        {
            _portalService = portalService;
        }

        [HttpGet]
        public async Task<IActionResult> LeaveApply()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var leaves = await _portalService.GetLeavesAsync(studentId, CurrentTenantId);
            var vm = new PortalLeaveApplyViewModel
            {
                Leaves = leaves,
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(1)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveApply(PortalLeaveApplyViewModel model)
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            if (!ModelState.IsValid)
            {
                model.Leaves = await _portalService.GetLeavesAsync(studentId, CurrentTenantId);
                return View(model);
            }

            var res = await _portalService.ApplyLeaveAsync(
                CurrentTenantId,
                studentId,
                model.FromDate!.Value,
                model.ToDate!.Value,
                model.LeaveType,
                model.Reason,
                CurrentUserType
            );

            if (res.Success)
            {
                TempData["SuccessMessage"] = "Leave application submitted successfully.";
                return RedirectToAction(nameof(LeaveApply));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            model.Leaves = await _portalService.GetLeavesAsync(studentId, CurrentTenantId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Transport()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var vm = await _portalService.GetTransportDetailsAsync(studentId, CurrentTenantId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> TCApply()
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            var list = await _portalService.GetTCStatusAsync(studentId, CurrentTenantId);
            var vm = new PortalTCApplyViewModel
            {
                Applications = list
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TCApply(PortalTCApplyViewModel model)
        {
            var studentId = CurrentStudentId;
            if (studentId == Guid.Empty) return View("NoStudentLinked");

            if (!ModelState.IsValid)
            {
                model.Applications = await _portalService.GetTCStatusAsync(studentId, CurrentTenantId);
                return View(model);
            }

            var res = await _portalService.ApplyTCAsync(
                CurrentTenantId,
                studentId,
                model.Reason,
                model.ExpectedLeavingDate!.Value
            );

            if (res.Success)
            {
                TempData["SuccessMessage"] = "Transfer certificate application submitted successfully.";
                return RedirectToAction(nameof(TCApply));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            model.Applications = await _portalService.GetTCStatusAsync(studentId, CurrentTenantId);
            return View(model);
        }
    }
}
