using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPortableLib
{
    public static class ApiConnectorHelper
    {
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
    public enum WebMethod
    {
        Get,
        Post,
        Put,
        DELETE,
    };

    public static class Extensions
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
                    throw new ArgumentOutOfRangeException(nameof(method));
            }
        }
    }
}