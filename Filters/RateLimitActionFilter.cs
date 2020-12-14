using auto_highlighter_back_end.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Filters
{
    public class RateLimitActionFilter : IActionFilter
    {

        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public RateLimitActionFilter(IConfiguration config, IMemoryCache cache)
        {
            _config = config;
            _cache = cache;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            RateLimitAttribute rateLimitAttribute = context.ActionDescriptor.FilterDescriptors
            .Select(x => x.Filter).OfType<RateLimitAttribute>().FirstOrDefault();

            string actionName = context.ActionDescriptor.DisplayName;
            string controllerName = context.Controller.GetType().FullName;

            if (rateLimitAttribute is not null)
            {
                bool testProxy = context.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For");
                int ipHash = 0;
                if (testProxy)
                {
                    bool ipAddress = IPAddress.TryParse(context.HttpContext.Request.Headers["X-Forwarded-For"], out IPAddress realClient);
                    if (ipAddress)
                    {
                        ipHash = realClient.GetHashCode();
                    }
                }
                if (ipHash != 0)
                {
                    ipHash = context.HttpContext.Connection.RemoteIpAddress.GetHashCode();
                }

                string key = actionName + controllerName + ipHash;

                _cache.TryGetValue(key, out bool forbidExecute);
                if (forbidExecute)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
                }
                else
                {
                    _cache.Set(key, true, new MemoryCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(rateLimitAttribute.MillisecondsBetweenRequests) });
                }
            }
        }
    }
}
