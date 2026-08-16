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

        //[Authorize(Roles = AppRoles.TenantUser)]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = User.Identity.Name;
            var roles = await _redis.GetUserFieldAsync(user, "roles");
            ViewBag.RedisRoles = roles;
            return View();
        }

        [Permission(Permissions.ReadVendor)]
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
    }
}
