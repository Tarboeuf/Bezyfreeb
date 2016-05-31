using Newtonsoft.Json.Linq;
using System;

namespace FreeboxPortableLib
{
    public class FBFileInfo
    {
        public FBFileInfo(JToken token)
        {
            Name = token["name"].ToString();
            Mimetype = token["mimetype"].ToString();
            Type = token["type"].ToString();
            Size = token["size"].ToString();

            try
            {
                Filecount = token.Value<string>("filecount");
                Foldercount = token.Value<string>("foldercount");
            }
            catch (Exception)
            {
            }
        }

        public string Name { get; set; }
        public string Filecount { get; set; }
        public string Foldercount { get; set; }
        public string Mimetype { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
    }
}
