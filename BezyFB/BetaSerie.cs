// Créer par : pepinat
// Le : 03-06-2014

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BezyFB
{
    public class BetaSerie
    {
        private const string _APIAdresse = "http://api.betaseries.com";

        private const string _EnteteArgs = "?v=2.2&key=d0256f2444ab";

        //private const string _EnteteArgs = "?v=2.2&key=3c15b9796654";

        private const string _Comments = "/comments";
        private const string _Episodes = "/episodes";
        private const string _Friends = "/friends";
        private const string _Members = "/members";
        private const string _Messages = "/messages";
        private const string _Movies = "/movies";
        private const string _Pictures = "/pictures";
        private const string _Planning = "/planning";
        private const string _Shows = "/shows";
        private const string _Subtitles = "/subtitles";
        private const string _Timeline = "/timeline";

        public string Error { get; set; }

        public string Token { get; set; }

        public BetaSerie()
        {
            try
            {
                Error = Helper.LireRequetePOST(_APIAdresse, _EnteteArgs, _Members + "/auth.xml", "&login=Tarboeuf&password=55fc47e4665c3df4047618f941c054e5", false);

                XDocument xdoc = XDocument.Parse(Error);
                var t = from lv1 in xdoc.Descendants("token")
                        select lv1.Value;

                Token = t.First();
                Error = Token;
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
        }

        public EpisodeRoot GetListeNouveauxEpisodesTest()
        {
            Error = "";
            try
            {
                var xml = Helper.LireRequetePOST(_APIAdresse, _EnteteArgs, _Episodes + "/list", "&userid=Tarboeuf&token=" + Token, false);

                EpisodeRoot rt = null;

                XmlSerializer serializer = new XmlSerializer(typeof (EpisodeRoot), new XmlRootAttribute("root"));
                var reader = GenerateStreamFromString(xml);
                rt = (EpisodeRoot) serializer.Deserialize(reader);
                reader.Close();

                return rt;
            }
            catch (Exception e)
            {
                Error = "GetListeNouveauxEpisodesTest : " + e.Message;
            }
            return null;
        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public List<Episode> GetListeNouveauxEpisodes()
        {
            return null;
        }

        public SousTitreRoot GetPathSousTitre(string episode)
        {
            Error = "";
            try
            {
                var xml = Helper.LireRequetePOST(_APIAdresse, _EnteteArgs, _Subtitles + "/episode", "&id=" + episode + "&language=vf&token=" + Token, false);
                SousTitreRoot rt = null;

                XmlSerializer serializer = new XmlSerializer(typeof (SousTitreRoot), new XmlRootAttribute("root"));
                var reader = GenerateStreamFromString(xml);
                rt = (SousTitreRoot) serializer.Deserialize(reader);
                reader.Close();

                return rt;
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
            return null;
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}