using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Entity
{
    public record HighlightEntity
    {
        public Guid Hid { get; init; }
        public string Status { get; init; }
        public DateTimeOffset CreatedTimestamp { get; init; }
    }
}
