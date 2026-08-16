using Microsoft.AspNetCore.Authorization;

namespace IMS.Web.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }
}
