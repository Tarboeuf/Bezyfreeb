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
        private const string ApiAdresse = "http://api.betaseries.com";

        private const string EnteteArgs = "?v=2.2&key=d0256f2444ab";

        //private const string _EnteteArgs = "?v=2.2&key=3c15b9796654";

        private const string Comments = "/comments";
        private const string Episodes = "/episodes";
        private const string Friends = "/friends";
        private const string Members = "/members";
        private const string Messages = "/messages";
        private const string Movies = "/movies";
        private const string Pictures = "/pictures";
        private const string Planning = "/planning";
        private const string Shows = "/shows";
        private const string Subtitles = "/subtitles";
        private const string Timeline = "/timeline";

        public string Error { get; private set; }

        private string Token { get; set; }

        public BetaSerie()
        {
            try
            {
                string link = ApiAdresse + Members + "/auth.xml" + EnteteArgs;
                link += "&login=Tarboeuf&password=55fc47e4665c3df4047618f941c054e5";
                Error = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

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
                string link = ApiAdresse + Episodes + "/list" + EnteteArgs;
                link += "&userid=Tarboeuf&token=" + Token;
                var xml = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof(EpisodeRoot), new XmlRootAttribute("root"));
                var reader = GenerateStreamFromString(xml);
                var rt = (EpisodeRoot)serializer.Deserialize(reader);
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
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
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
                string link = ApiAdresse + Subtitles + "/episode" + EnteteArgs;
                link += "&id=" + episode + "&language=vf&token=" + Token;
                var xml = ApiConnector.Call(link, WebMethod.Get, null, null, "text/xml");

                var serializer = new XmlSerializer(typeof(SousTitreRoot), new XmlRootAttribute("root"));
                var reader = GenerateStreamFromString(xml);
                var rt = (SousTitreRoot)serializer.Deserialize(reader);
                reader.Close();

                return rt;
            }
            catch (Exception e)
            {
                Error = "GetPathSousTitre : " + e.Message;
            }
            return null;
        }

        private static string GetMd5Hash(HashAlgorithm md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            foreach (byte t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}