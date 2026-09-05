using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IMS.Models.Portal;

namespace IMS.Web.Areas.StudentPortal.Controllers
{
    [Area("StudentPortal")]
    [Authorize(AuthenticationSchemes = "StudentPortalAuth")]
    public abstract class StudentPortalBaseController : Controller
    {
        public Guid CurrentUserId
        {
            get
            {
                var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(val, out var id) ? id : Guid.Empty;
            }
        }

        public Guid CurrentTenantId
        {
            get
            {
                var val = User.FindFirstValue("TenantId");
                return Guid.TryParse(val, out var id) ? id : Guid.Empty;
            }
        }

        public string CurrentUserType => User.FindFirstValue(ClaimTypes.Role) ?? "STUDENT";
        public string CurrentUserFullName => User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        public string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public List<LinkedStudentDto> LinkedStudents
        {
            get
            {
                var json = User.FindFirstValue("LinkedStudents");
                if (string.IsNullOrEmpty(json)) return new List<LinkedStudentDto>();
                try
                {
                    return JsonSerializer.Deserialize<List<LinkedStudentDto>>(json) ?? new List<LinkedStudentDto>();
                }
                catch
                {
                    return new List<LinkedStudentDto>();
                }
            }
        }

        public Guid CurrentStudentId
        {
            get
            {
                // Check if an active student override cookie exists for guardians
                if (Request.Cookies.TryGetValue("IMS_ActiveStudentId", out var cookieVal) && Guid.TryParse(cookieVal, out var cookieStudentId))
                {
                    // Verify that the requested student ID is legitimately linked to this guardian/student
                    if (CurrentUserType == "STUDENT" && cookieStudentId == CurrentUserId)
                    {
                        return cookieStudentId;
                    }
                    if (CurrentUserType == "GUARDIAN" && LinkedStudents.Any(w => w.StudentId == cookieStudentId))
                    {
                        return cookieStudentId;
                    }
                }

                // Fallback to primary student in claims
                var val = User.FindFirstValue("ActiveStudentId");
                if (Guid.TryParse(val, out var primaryId) && primaryId != Guid.Empty)
                {
                    return primaryId;
                }

                return LinkedStudents.FirstOrDefault()?.StudentId ?? Guid.Empty;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var activeStudentId = CurrentStudentId;
            var activeWard = LinkedStudents.FirstOrDefault(w => w.StudentId == activeStudentId);

            ViewBag.CurrentStudentId = activeStudentId;
            ViewBag.ActiveWard = activeWard;
            ViewBag.CurrentUserType = CurrentUserType;
            ViewBag.CurrentUserFullName = CurrentUserFullName;
            ViewBag.CurrentUserEmail = CurrentUserEmail;
            ViewBag.LinkedStudents = LinkedStudents;
        }

        protected void SetActiveStudentCookie(Guid studentId)
        {
            Response.Cookies.Append("IMS_ActiveStudentId", studentId.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }
    }
}
