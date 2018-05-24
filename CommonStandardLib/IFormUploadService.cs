using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonStandardLib
{
    public interface IFormUploadService
    {
        Task<string> MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters, IEnumerable<Tuple<string, string>> headers = null);
    }
}
