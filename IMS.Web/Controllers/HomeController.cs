using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics;
using IMS.Helpers.Constants;
using IMS.Services;
using IMS.Services.Interfaces;
using IMS.Web.Authorization;
using IMS.Web.Models;

namespace IMS.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly IUserSessionService _sessionService;
        private readonly IRedisService _redis;

        public HomeController(IUserSessionService sessionService, IRedisService redis)
        {
            _sessionService = sessionService;
            _redis = redis;
        }
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View(); 
            }
            return RedirectToAction("Dashboard");
        }

        //[Authorize(Roles = AppRoles.TenantAdmin)]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = User.Identity.Name;
            var roles = await _redis.GetUserFieldAsync(user, "roles");
            ViewBag.RedisRoles = roles;
            return View();
        }

        //[Permission(Permissions.ReadVendor)]
        public IActionResult AdminPage()
        {
            return Content("Only TENANT_ADMIN can see this");
        }

        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/Home/Dashboard"
            });
        }
       
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            await _sessionService.RemoveUserSessionAsync(username);

            return SignOut(new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },OpenIdConnectDefaults.AuthenticationScheme,CookieAuthenticationDefaults.AuthenticationScheme);
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult About() => View();

        [AllowAnonymous]
        public IActionResult Academics() => View();

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Admission()
        {
            var vm = new IMS.Models.ViewModels.AdmissionApplicationFormViewModel();
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyAdmission(
            IMS.Models.ViewModels.AdmissionApplicationFormViewModel model,
            [FromServices] IAdmissionApplicationService admissionService)
        {
            if (!ModelState.IsValid)
            {
                return View("Admission", model);
            }

            if (string.IsNullOrWhiteSpace(model.AA_ApplicationNumber))
            {
                model.AA_ApplicationNumber = "APP-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
            }

            model.AA_Status = "Submitted";
            var tenantId = HardcodedMasterData.CurrentTenantId;
            var result = await admissionService.CreateAsync(model, tenantId);

            if (result.Success)
            {
                TempData["AdmissionSuccess"] = $"Your application has been received! Your application reference number is: {model.AA_ApplicationNumber}. Our admissions office will contact you soon.";
                return RedirectToAction(nameof(Admission));
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to submit application. Please try again.");
            return View("Admission", model);
        }

        [AllowAnonymous]
        public IActionResult Notices() => View();

        [AllowAnonymous]
        public IActionResult Gallery() => View();

        [AllowAnonymous]
        public IActionResult Calendar() => View();

        [AllowAnonymous]
        public IActionResult Contact() => View();
    }
}
