using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    public class StudentController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return RedirectToAction("Login", "Auth", new { area = "StudentPortal", returnUrl });
        }
    }
}
