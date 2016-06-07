using CommonPortableLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BezyFB.Configuration;

namespace BezyFB.Helpers
{
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

                case WebMethod.DELETE:
                    return "DELETE";

                default:
                    throw new ArgumentOutOfRangeException("method");
            }
        }
    }

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
                    httpWebRequest.Headers.Add(header.Item1, header.Item2);

            if (!string.IsNullOrEmpty(content))
            {
                byte[] byteArray = (encoding ?? Encoding.UTF8).GetBytes(content);
                httpWebRequest.ContentLength = byteArray.Length;

                Stream dataStream = await httpWebRequest.GetRequestStreamAsync();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            using (var webResponse = await httpWebRequest.GetResponseAsync())
            {
                using (Stream httpResponse = webResponse.GetResponseStream())
                {
                    if (null == httpResponse) return null;
                    using (var streamReader = new StreamReader(httpResponse))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
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
                    httpWebRequest.Headers.Add(header.Item1, header.Item2);

            if (content.Length > 0)
            {
                byte[] byteArray = (Encoding.UTF8).GetBytes(content);
                httpWebRequest.ContentLength = byteArray.Length + text.Length;

                using (Stream dataStream = await httpWebRequest.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Write(text, 0, text.Length);
                    dataStream.Close();
                }
            }

            try
            {
                Stream httpResponse = (await httpWebRequest.GetResponseAsync()).GetResponseStream();
                if (null == httpResponse) return null;
                using (var streamReader = new StreamReader(httpResponse))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<byte[]> GetResponse(string url, string contentType)
        {
            return null;
        }


    }

    public class FormUpload : IFormUploadService
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public async Task<string> MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters, IEnumerable<Tuple<string, string>> headers = null)
        {
            string formDataBoundary = $"----------{Guid.NewGuid():N}";
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            var response = PostForm(postUrl, userAgent, contentType, formData, headers).GetResponseStream();

            using (StreamReader sr = new StreamReader(response))
            {
                return sr.ReadToEnd();
            }
        }

        private HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData, IEnumerable<Tuple<string, string>> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            request.Accept = null;

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Item1, header.Item2);
            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

    }
}