using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommonPortableLib
{
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
                    throw new ArgumentOutOfRangeException("method");
            }
        }
    }
}