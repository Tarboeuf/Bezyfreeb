using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BezyFB
{
    public enum WebMethod
    {
        Get,
        Post,
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

            if (content != null)
            {
                httpWebRequest.ContentLength = content.Length;
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(content);
                    streamWriter.Flush();
                    streamWriter.Close();
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