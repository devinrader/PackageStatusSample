using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PackageStatus
{
    public static class UrlHelperExtensions
    {
        public static string AbsoluteContent(this UrlHelper url, string contentPath, string absoluteUri)
        {
            var uriBuilder = new System.UriBuilder(absoluteUri)
            {
                Path = url.Content(contentPath)
            };

            return uriBuilder.ToString();
        }
    }
}