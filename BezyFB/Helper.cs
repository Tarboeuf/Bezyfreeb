// Créer par : pepinat
// Le : 23-06-2014

using System.IO;
using System.Net;
using System.Text;

namespace BezyFB
{
    public static class Helper
    {
        public static string LireRequetePOST(string apiAdresse, string enteteArgs, string rubrique, string post, bool isPost)
        {
            string link = apiAdresse + rubrique;

            if (!isPost)
                link += enteteArgs + post;

            HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(link);

            ASCIIEncoding encoding = new ASCIIEncoding();

            if (isPost)
            {
                byte[] data = encoding.GetBytes(enteteArgs + post);

                httpWReq.Method = "POST";

                httpWReq.ContentLength = data.Length;

                using (Stream stream = httpWReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            httpWReq.Accept = "text/xml";

            HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }
    }
}