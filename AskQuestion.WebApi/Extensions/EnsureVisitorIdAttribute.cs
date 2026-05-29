using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AskQuestion.WebApi.Extensions
{
    public class EnsureVisitorIdAttribute : ActionFilterAttribute
    {
        private const string CookieName = "VisitorId";
        private const string ItemsKey = "VisitorId";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? visitorId = context.HttpContext.Request.Cookies[CookieName];

            if (string.IsNullOrEmpty(visitorId) || !Guid.TryParse(visitorId, out _))
            {
                visitorId = Guid.NewGuid().ToString();
                context.HttpContext.Response.Cookies.Append(CookieName, visitorId, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(365),
                    Secure = false,
                });
            }

            context.HttpContext.Items[ItemsKey] = visitorId;

            base.OnActionExecuting(context);
        }
    }
}
