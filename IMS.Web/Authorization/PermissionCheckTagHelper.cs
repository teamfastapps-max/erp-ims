using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IMS.Web.TagHelpers
{
    [HtmlTargetElement("permission-check")]
    public class PermissionCheckTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;

        public PermissionCheckTagHelper(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }
        public string Code { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                output.TagName = null; 
                return;
            }

            var result = await _authorizationService.AuthorizeAsync(ViewContext.HttpContext.User, $"Permission:{Code}");

            if (!result.Succeeded)
            {
                output.SuppressOutput(); 
                return;
            }

            output.TagName = null; 
        }
    }
}
