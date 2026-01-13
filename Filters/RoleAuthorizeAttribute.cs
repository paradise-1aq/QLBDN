using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QLBDN.Filters
{
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public RoleAuthorizeAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (role == null)
            {
                // Chưa đăng nhập
                context.Result = new RedirectToActionResult("Login", "User", null);
                return;
            }

            if (role != _role)
            {
                // Sai quyền → chặn
                context.Result = new RedirectToActionResult("AccessDenied", "User", null);
            }
        }
    }
}
