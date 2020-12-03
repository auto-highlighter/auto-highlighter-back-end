using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimitAttribute : Attribute, IFilterMetadata
    {
        public RateLimitAttribute(int MillisecondsBetweenRequests)
        {
            this.MillisecondsBetweenRequests = MillisecondsBetweenRequests;
        }

        public int MillisecondsBetweenRequests { get; set; }
    }

}
