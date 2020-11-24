using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Responses
{
    public class HighlightStatusResponse
    {
        public Guid hid { get; set; }
        public String Status { get; set; }
    }
}
