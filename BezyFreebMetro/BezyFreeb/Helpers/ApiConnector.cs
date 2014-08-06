using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BezyFB.Helpers
{
    public enum WebMethod
    {
        Get,
        Post,
        Put,
    };

    internal static class Extensions
    {
        public static string GetLibelle(this WebMethod method)
        {
            switch (method)
            {
                case WebMethod.Get:
                    return "GET";
                case WebMethod.Post:
                    return "POST";
                case WebMethod.Put:
                    return "PUT";
                default:
                    throw new ArgumentOutOfRangeException("method");
            }
        }
    }

    public static class ApiConnector
    {
        public static async Task<string> Call(string url, WebMethod method = WebMethod.Post, string contentType = null, string content = null,
                                  string headerAccept = null, IEnumerable<Tuple<string, string>> headers = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(url);
            httpWebRequest.Method = method.GetLibelle();
            httpWebRequest.ContentType = contentType;

            httpWebRequest.Accept = headerAccept;

            if (headers != null)
                foreach (var header in headers)
                    httpWebRequest.Headers[header.Item1] = header.Item2;

            if (!string.IsNullOrEmpty(content))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(content);
                //httpWebRequest.ContentLength = byteArray.Length;

                Stream dataStream = await httpWebRequest.GetRequestStreamAsync();
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                var response = await httpWebRequest.GetResponseAsync();
                Stream httpResponse = response.GetResponseStream();
                if (null == httpResponse) return null;
                using (var streamReader = new StreamReader(httpResponse))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}