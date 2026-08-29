using IMS.Models.Teacher;
using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMS.Web.Extensions;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class TeachersController : Controller
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }
       
        // GET: /Teachers
        public async Task<IActionResult> Index(string searchTerm, string status, Guid? branchId, int page = 1, int pageSize = 10)
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var model = await _teacherService.GetTeacherListAsync(tenantId, accessToken, searchTerm, status, branchId, page, pageSize);
            return View(model);
        }

        // GET: /Teachers/Create
        public async Task<IActionResult> Create()
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var model = await _teacherService.GetNewTeacherFormAsync(tenantId, accessToken);
            return View(model);
        }

        // GET: /Teachers/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var model = await _teacherService.GetTeacherForEditAsync(id, tenantId, accessToken);
            if (model == null) return NotFound();

            return View(model);
        }

        // GET: /Teachers/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var model = await _teacherService.GetTeacherDetailsAsync(id, tenantId, accessToken);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /Teachers/AddTeacher (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(TeacherFormViewModel model)
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var result = await _teacherService.CreateTeacherAsync(model, tenantId, accessToken);
            return Json(new { success = result.Success, message = result.Message });
        }

        // POST: /Teachers/EditTeacher (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(TeacherFormViewModel model)
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var result = await _teacherService.UpdateTeacherAsync(model, tenantId, accessToken);
            return Json(new { success = result.Success, message = result.Message });
        }

        // POST: /Teachers/DeleteTeacher/{id} (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(Guid id)
        {
            var tenantId = GetTenantId();
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }
            var result = await _teacherService.DeleteTeacherAsync(id, tenantId, accessToken);
            return Json(new { success = result.Success, message = result.Message });
        }

        // Set by UserSessionService.StoreUserSessionAsync via
        // identity?.AddClaim(new Claim("tenant_id", profile.TenantId))
        private Guid GetTenantId()
        {
            var claim = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
       
    }
}
