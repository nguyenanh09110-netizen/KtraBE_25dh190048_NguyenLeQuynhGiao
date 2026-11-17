using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebBH.Helpers
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null)
                return false;

            var role = httpContext.Session["Role"] as string;
            return !string.IsNullOrEmpty(role) && role.Trim() == "A";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var username = session?["Username"] as string;

            if (string.IsNullOrEmpty(username))
            {
                // Chưa đăng nhập -> chuyển về trang Đăng nhập
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login",
                        returnUrl = filterContext.HttpContext.Request.RawUrl
                    }));
            }
            else
            {
                // Đã đăng nhập nhưng không phải Admin -> đá về trang bán hàng
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Shop",
                        action = "Index"
                    }));
            }
        }
    }
}