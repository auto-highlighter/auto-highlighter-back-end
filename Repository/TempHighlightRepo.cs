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
        private readonly List<HighlightEntity> highlights = new()
        {
            new HighlightEntity { Hid = Guid.NewGuid(), Status = HighlightStatusEnum.Ready.ToString(), CreatedTimestamp = DateTimeOffset.UtcNow },
            new HighlightEntity { Hid = Guid.NewGuid(), Status = HighlightStatusEnum.Ready.ToString(), CreatedTimestamp = DateTimeOffset.UtcNow },
            new HighlightEntity { Hid = Guid.NewGuid(), Status = HighlightStatusEnum.Ready.ToString(), CreatedTimestamp = DateTimeOffset.UtcNow },
            new HighlightEntity { Hid = Guid.NewGuid(), Status = HighlightStatusEnum.Ready.ToString(), CreatedTimestamp = DateTimeOffset.UtcNow },
        };

        public IEnumerable<HighlightEntity> GetHighlights()
        {
            return highlights;
        }

        public HighlightEntity GetHighlight(Guid hid)
        {
            return highlights.Where(highlight => highlight.Hid == hid).SingleOrDefault();
        }

        public void CreateHighlight(HighlightEntity highlight)
        {
            highlights.Add(highlight);
        }
    }
}
