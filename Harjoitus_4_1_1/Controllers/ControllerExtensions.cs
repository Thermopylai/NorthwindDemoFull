using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Harjoitus_4_1_1.Controllers
{
    public static class ControllerExtensions
    {
        // Returns the current URL (path + query) without any nested returnUrl/ReturnUrl parameter.
        public static string GetCleanReturnUrl(this Controller controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            var request = controller.Request;
            if (request == null || request.Url == null)
                return "/";

            var path = request.Url.AbsolutePath;

            var query = HttpUtility.ParseQueryString(request.Url.Query);
            query.Remove("returnUrl");
            query.Remove("ReturnUrl");

            var remaining = query.AllKeys
                .Where(k => !string.IsNullOrEmpty(k))
                .Select(k => string.Format("{0}={1}", HttpUtility.UrlEncode(k), HttpUtility.UrlEncode(query[k])));

            var qs = string.Join("&", remaining);
            return string.IsNullOrEmpty(qs) ? path : (path + "?" + qs);
        }
    }
}