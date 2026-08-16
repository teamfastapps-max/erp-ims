using Microsoft.AspNetCore.Mvc;
using IMS.Services.Interfaces;
using IMS.Models;
using System.Security.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace IMS.Web.Areas.Vendor.Controllers
{
    //[Area("Vendor")]
    //public class VendorController : Controller
    //{
    //    private readonly IVendorService _vendorService;

    //    public VendorController(IVendorService vendorService)
    //    {
    //        _vendorService = vendorService;
    //    }

    //    // GET: Vendor/Create
    //    public IActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: Vendor/Create
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public IActionResult Create(VendorModel model)
    //    {
    //        if (!ModelState.IsValid)
    //            return View(model);

    //        try
    //        {
    //            int vendorId = _vendorService.AddVendor(model);

    //            TempData["Success"] = "Vendor created successfully.";

    //            return RedirectToAction(nameof(Index));
    //        }
    //        catch (Exception ex)
    //        {
    //            ModelState.AddModelError("", ex.Message);
    //            return View(model);
    //        }
    //    }

    //    // GET: Vendor/Index
    //    public IActionResult Index()
    //    {
    //        return View();
    //    }
    //}

    [Area("Vendor")]
    [Route("Vendor")]
    public class VendorController : Controller
    {
        private readonly IVendorService _vendorService;

        // TODO: Replace with actual tenant/user resolution from session/claims
        private static readonly Guid DemoTenantId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");
        private const int DemoUserId = 1;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // ============================================================================
        // INDEX — Vendor List
        // ============================================================================
        [HttpGet]
        public async Task<IActionResult> Index(VendorFilterModel filter)
        {
            filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var model = await _vendorService.GetAllVendorsAsync(DemoTenantId, filter);
            //var stats = await _vendorService.GetVendorStatsAsync(DemoTenantId);

            //ViewBag.Stats = stats;
            return View(model);
        }

        // ============================================================================
        // DETAIL — View single vendor
        // ============================================================================
        [HttpGet]
        //[SecurityPermission(PermissionEnum.VENDOR_DETAILS)]
        public async Task<IActionResult> Detail(int id)
        {
            var model = await _vendorService.GetVendorByIdAsync(id, DemoTenantId);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Vendor not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ============================================================================
        // CREATE
        // ============================================================================
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new VendorModel();
            //{
            //    Categories = await _vendorService.GetCategoriesAsync(DemoTenantId),
            //    Currencies = await _vendorService.GetCurrenciesAsync(),
            //    IsActive = true
            //};
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _vendorService.GetCategoriesAsync(DemoTenantId);
                model.Currencies = await _vendorService.GetCurrenciesAsync();
                return View(model);
            }

            var (success, message, vendorId) = await _vendorService.CreateVendorAsync(model, DemoTenantId, DemoUserId);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Detail), new { id = vendorId });
            }

            ModelState.AddModelError(string.Empty, message);
            model.Categories = await _vendorService.GetCategoriesAsync(DemoTenantId);
            model.Currencies = await _vendorService.GetCurrenciesAsync();
            return View(model);
        }

        // ============================================================================
        // EDIT
        // ============================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _vendorService.GetVendorByIdAsync(id, DemoTenantId);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Vendor not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VendorModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _vendorService.GetCategoriesAsync(DemoTenantId);
                model.Currencies = await _vendorService.GetCurrenciesAsync();
                return View(model);
            }

            var (success, message) = await _vendorService.UpdateVendorAsync(model, DemoTenantId);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Detail), new { id = model.VendorId });
            }

            ModelState.AddModelError(string.Empty, message);
            model.Categories = await _vendorService.GetCategoriesAsync(DemoTenantId);
            model.Currencies = await _vendorService.GetCurrenciesAsync();
            return View(model);
        }

        // ============================================================================
        // DELETE (AJAX)
        // ============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _vendorService.DeleteVendorAsync(id, DemoTenantId);
            return Json(new { success, message });
        }

        // ============================================================================
        // ADDRESS (AJAX)
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> SaveAddress([FromBody] VendorAddressModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var (success, message) = await _vendorService.UpsertAddressAsync(model, DemoTenantId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var (success, message) = await _vendorService.DeleteAddressAsync(id, DemoTenantId);
            return Json(new { success, message });
        }

        // ============================================================================
        // CONTACT (AJAX)
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> SaveContact([FromBody] VendorContactModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var (success, message) = await _vendorService.UpsertContactAsync(model, DemoTenantId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var (success, message) = await _vendorService.DeleteContactAsync(id, DemoTenantId);
            return Json(new { success, message });
        }

        // ============================================================================
        // BANK (AJAX)
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> SaveBank([FromBody] VendorBankModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            var (success, message) = await _vendorService.UpsertBankAsync(model, DemoTenantId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBank(int id)
        {
            var (success, message) = await _vendorService.DeleteBankAsync(id, DemoTenantId);
            return Json(new { success, message });
        }

        // ============================================================================
        // DOCUMENT UPLOAD (AJAX)
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int vendorId, string documentType,
            string documentNumber, IFormFile file)
        {
            var (success, message, docId) = await _vendorService.UploadDocumentAsync(
                vendorId, documentType, documentNumber, file, DemoTenantId, DemoUserId);
            return Json(new { success, message, docId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var (success, message) = await _vendorService.DeleteDocumentAsync(id, DemoTenantId);
            return Json(new { success, message });
        }
    }

}
