using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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
        public static string Call(string url, WebMethod method = WebMethod.Post, string contentType = null, string content = null,
                                  string headerAccept = null, IEnumerable<Tuple<string, string>> headers = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method.GetLibelle();
            httpWebRequest.ContentType = contentType;

            httpWebRequest.Accept = headerAccept;

            if (headers != null)
                foreach (var header in headers)
                    httpWebRequest.Headers.Add(header.Item1, header.Item2);

            if (content != null && content.Length != 0)
            {
                httpWebRequest.ContentLength = content.Length;
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(content);
                }
            }

            Stream httpResponse = httpWebRequest.GetResponse().GetResponseStream();
            if (null == httpResponse) return null;
            using (var streamReader = new StreamReader(httpResponse))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}