using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommonStandardLib
{
    public interface IApiConnectorService
    {
        Task<string> Call(string url, WebMethod method = WebMethod.Post, string contentType = null, string content = null,
                                    string headerAccept = null, IEnumerable<Tuple<string, string>> headers = null, Encoding encoding = null);

        Task<string> CallByte(string url, WebMethod method = WebMethod.Post, string contentType = null, string content = null, byte[] text = null,
                                  string headerAccept = null, IEnumerable<Tuple<string, string>> headers = null);

        Task<byte[]> GetResponse(string url, string contentType);
    }
}
