using CommonPortableLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class ApiConnector : IApiConnectorService
    {
        public async Task<string> Call(string url, WebMethod method = WebMethod.Post, string contentType = null, string content = null,
                                  string headerAccept = null, IEnumerable<Tuple<string, string>> headers = null, Encoding encoding = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method.GetLibelle();
            httpWebRequest.ContentType = contentType;

            httpWebRequest.Accept = headerAccept;

            if (headers != null)
                foreach (var header in headers)
                    httpWebRequest.Headers[header.Item1] = header.Item2;

            if (!string.IsNullOrEmpty(content))
            {
                byte[] byteArray = (encoding ?? Encoding.UTF8).GetBytes(content);
                //httpWebRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = await httpWebRequest.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }

            try
            {
                var response = await httpWebRequest.GetResponseAsync();
                using (Stream httpResponse = response.GetResponseStream())
                {
                    if (null == httpResponse) return null;
                    using (var streamReader = new StreamReader(httpResponse))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> CallByte(string url, WebMethod method = WebMethod.Post, string contentType = null, string content = null, byte[] text = null,
                                  string headerAccept = null, IEnumerable<Tuple<string, string>> headers = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method.GetLibelle();
            httpWebRequest.ContentType = contentType;

            httpWebRequest.Accept = headerAccept;

            if (headers != null)
                foreach (var header in headers)
                    httpWebRequest.Headers[header.Item1] = header.Item2;

            if (content.Length > 0)
            {
                byte[] byteArray = (Encoding.UTF8).GetBytes(content);
                //httpWebRequest.Headers.ContentLength = byteArray.Length + text.Length;

                using (Stream dataStream = await httpWebRequest.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Write(text, 0, text.Length);
                }
            }

            try
            {
                var response = await httpWebRequest.GetResponseAsync();
                using (Stream httpResponse = response.GetResponseStream())
                {
                    if (null == httpResponse) return null;
                    using (var streamReader = new StreamReader(httpResponse))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public async Task<byte[]> GetResponse(string url, string contentType)
        {
            WebRequest request = HttpWebRequest.Create(url);

            request.ContentType = "application/x-bittorrent";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (null != stream)
                    {
                        return ApiConnectorHelper.ReadFully(stream);
                    }
                }
            }
            return null;
        }
    }
}