using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMS.Models.Portal;
using IMS.Services.Interfaces;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    [Area("StudentPortal")]
    public class AuthController : Controller
    {
        private readonly IPortalService _portalService;

        public AuthController(IPortalService portalService)
        {
            _portalService = portalService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && (User.FindFirstValue(ClaimTypes.Role) == "STUDENT" || User.FindFirstValue(ClaimTypes.Role) == "GUARDIAN"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "StudentPortal" });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new PortalLoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(PortalLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _portalService.AuthenticateAsync(model.Email.Trim(), model.Password);
            if (!result.IsAuthenticated)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid credentials. Please check your email and password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.FullName),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Role, result.UserType),
                new Claim("TenantId", result.TenantId.ToString()),
                new Claim("ActiveStudentId", result.ActiveStudentId.ToString()),
                new Claim("StudentCode", result.StudentCode ?? string.Empty),
                new Claim("AdmissionNumber", result.AdmissionNumber ?? string.Empty),
                new Claim("BranchName", result.BranchName ?? string.Empty),
                new Claim("LinkedStudents", JsonSerializer.Serialize(result.LinkedStudents))
            };

            if (result.BranchId.HasValue)
            {
                claims.Add(new Claim("BranchId", result.BranchId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "StudentPortalAuth");
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync("StudentPortalAuth", principal, authProperties);

            Response.Cookies.Append("IMS_ActiveStudentId", result.ActiveStudentId.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "StudentPortal" });
        }

        [Authorize(AuthenticationSchemes = "StudentPortalAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("StudentPortalAuth");
            Response.Cookies.Delete("IMS_ActiveStudentId");
            return RedirectToAction("Login", "Auth", new { area = "StudentPortal" });
        }

        [Authorize(AuthenticationSchemes = "StudentPortalAuth")]
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync("StudentPortalAuth");
            Response.Cookies.Delete("IMS_ActiveStudentId");
            return RedirectToAction("Login", "Auth", new { area = "StudentPortal" });
        }

        [Authorize(AuthenticationSchemes = "StudentPortalAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SwitchStudent(Guid studentId, string? returnUrl = null)
        {
            var json = User.FindFirstValue("LinkedStudents");
            var linkedList = !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<List<LinkedStudentDto>>(json)
                : new List<LinkedStudentDto>();

            var userType = User.FindFirstValue(ClaimTypes.Role);
            var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : Guid.Empty;

            bool isAllowed = false;
            if (userType == "STUDENT" && studentId == userId) isAllowed = true;
            else if (userType == "GUARDIAN" && linkedList != null && linkedList.Exists(w => w.StudentId == studentId)) isAllowed = true;

            if (isAllowed)
            {
                Response.Cookies.Append("IMS_ActiveStudentId", studentId.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "StudentPortal" });
        }

        #region Forgot and Reset Password Flow
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var res = await _portalService.ForgotPasswordAsync(model.Email.Trim(), baseUrl);

            model.EmailSent = res.Success;
            if (res.Success)
            {
                model.InfoMessage = "A password reset request has been verified. You can use the secure link below to reset your password.";
                model.ResetUrlForDemo = res.DemoResetUrl;
            }
            else
            {
                ModelState.AddModelError(string.Empty, res.Message);
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string? token = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var res = await _portalService.ResetPasswordWithTokenAsync(model.Token, model.NewPassword);
            if (res.Success)
            {
                model.IsSuccess = true;
                model.Message = res.Message;
                return View(model);
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(model);
        }

        [Authorize(AuthenticationSchemes = "StudentPortalAuth")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize(AuthenticationSchemes = "StudentPortalAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = User.FindFirstValue(ClaimTypes.Role) ?? "STUDENT";

            if (!Guid.TryParse(userIdVal, out var userId))
            {
                return RedirectToAction("Login");
            }

            var res = await _portalService.ChangePasswordAsync(userId, userType, model.CurrentPassword, model.NewPassword);
            if (res.Success)
            {
                TempData["SuccessMessage"] = "Password updated successfully!";
                return RedirectToAction("Index", "Dashboard", new { area = "StudentPortal" });
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(model);
        }
        #endregion

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
