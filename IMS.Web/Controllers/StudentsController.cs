using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMS.Helpers.Constants;
using IMS.Models.ViewModels;
using IMS.Services.Interfaces;

namespace IMS.Web.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // TODO: replace with tenant resolved from the authenticated user's claims
        private Guid CurrentTenantId => HardcodedMasterData.CurrentTenantId;

        // GET: /Students
        public async Task<IActionResult> Index(string searchTerm, string status, Guid? branchId, int page = 1)
        {
            var vm = await _studentService.GetStudentListAsync(
                CurrentTenantId, searchTerm, status, branchId, page, pageSize: 10);

            return View(vm);
        }

        // GET: /Students/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var vm = await _studentService.GetStudentDetailsAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // GET: /Students/Create
        public IActionResult Create()
        {
            var vm = new StudentFormViewModel
            {
                BranchOptions = HardcodedMasterData.GetBranchSelectList(),
                GenderOptions = HardcodedMasterData.GetGenderSelectList(),
                StatusOptions = HardcodedMasterData.GetStatusSelectList("Admitted")
            };
            return View(vm);
        }

        // POST: /Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                RepopulateDropdowns(model);
                return View(model);
            }

            var (id, error) = await _studentService.CreateStudentAsync(model, CurrentTenantId);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                RepopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Student created successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Students/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var vm = await _studentService.GetStudentForEditAsync(id, CurrentTenantId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: /Students/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, StudentFormViewModel model)
        {
            if (id != model.S_Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                RepopulateDropdowns(model);
                return View(model);
            }

            var (success, error) = await _studentService.UpdateStudentAsync(model, CurrentTenantId);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error);
                RepopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Student updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Students/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _studentService.DeleteStudentAsync(id, CurrentTenantId);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Student removed successfully." : "Unable to remove student.";
            return RedirectToAction(nameof(Index));
        }

        private static void RepopulateDropdowns(StudentFormViewModel model)
        {
            model.BranchOptions = HardcodedMasterData.GetBranchSelectList(model.S_BranchId);
            model.GenderOptions = HardcodedMasterData.GetGenderSelectList(model.S_Gender);
            model.StatusOptions = HardcodedMasterData.GetStatusSelectList(model.S_Status);
        }
    }
}
