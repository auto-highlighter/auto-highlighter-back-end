using auto_highlighter_back_end.DTOs;
using auto_highlighter_back_end.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Extentions
{
    public static class DTOExtentions
    {

        public static HighlightStatusDTO AsDto(this HighlightEntity highlightEntity)
        {
            return new HighlightStatusDTO
            {
                Hid = highlightEntity.Hid,
                Status = highlightEntity.Status
            };
        }
    }
}
