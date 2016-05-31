using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPortableLib
{
    public interface IFormUploadService
    {
        Task<string> MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters, IEnumerable<Tuple<string, string>> headers = null);
    }
}
