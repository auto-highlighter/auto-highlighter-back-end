using auto_highlighter_back_end.Entity;
using System;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Services
{
    public interface IVideoProcessService
    {
        Task ProcessHightlightAsync(HighlightEntity highlight);
    }
}