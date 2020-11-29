using System;
using System.IO;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Services
{
    public interface IBlobService
    {
        Task<Uri> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName);
    }
}