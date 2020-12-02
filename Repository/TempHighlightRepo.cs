using auto_highlighter_back_end.Entity;
using auto_highlighter_back_end.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Repository
{
    public class TempHighlightRepo : ITempHighlightRepo
    {
        private readonly List<HighlightEntity> highlights = new();

        public IEnumerable<HighlightEntity> GetHighlights()
        {
            return highlights;
        }

        public HighlightEntity GetHighlight(Guid hid)
        {

            return (from highlight in highlights
                    where highlight.Hid == hid
                    select highlight).SingleOrDefault();
        }

        public void CreateHighlight(HighlightEntity highlight)
        {
            highlights.Add(highlight);
        }

        public void UpdateHighlight(HighlightEntity newHighlight)
        {
            highlights[highlights.FindIndex((repoHighlight) => repoHighlight.Hid == newHighlight.Hid)] = newHighlight;
        }
    }
}
