    using auto_highlighter_back_end.Entity;
using System;
using System.Collections.Generic;

namespace auto_highlighter_back_end.Repository
{
    public interface ITempHighlightRepo
    {
        HighlightEntity GetHighlight(Guid hid);
        IEnumerable<HighlightEntity> GetHighlights();
        void CreateHighlight(HighlightEntity highlight);
    }
}