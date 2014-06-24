using System;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace TarboeufEzBsFb
{
    class Freebox
    {
        public static void Main2()
        {
            var app_token = "";
            var challenge = "";
            var track_id = "";
            var sessionRequest = "";

            if (!File.Exists("appid"))
            {
                Tuple<string, string> apptokenrequest = AppTokenRequest();

                TextWriter tw = new StreamWriter("appid");
                tw.WriteLine(apptokenrequest.Item1);
                tw.WriteLine(apptokenrequest.Item2);
                tw.Close();

                app_token = apptokenrequest.Item1;
                track_id = apptokenrequest.Item2;

                Tuple<string, string> apptokenstatus = AppTokenStatus(apptokenrequest.Item2);
                while (apptokenstatus.Item1.ToLower() == "pending")
                    apptokenstatus = AppTokenStatus(apptokenrequest.Item2);

                challenge = apptokenstatus.Item2;
            }
            else
            {
                var tr = new StreamReader("appid");

                app_token = tr.ReadLine();
                track_id = tr.ReadLine();

                tr.Close();

                challenge = ChallengeRequest();
            }

            sessionRequest = SessionRequest("fr.freebox.testapp", app_token, challenge);

            if (sessionRequest != String.Empty)
            {
                //string Output = ApiCall("http://mafreebox.freebox.fr/api/v2/downloads/", "",  "application/json", sessionRequest, "GET");
                //Console.WriteLine(Output);
                string content = "download_url=http%3A%2F%2Fmirror.ovh.net%2Fubuntu-releases%2F14.04%2Fubuntu-14.04-desktop-amd64.iso.torrent";
                string Output = ApiCall("http://mafreebox.freebox.fr/api/v2/downloads/add", content, "application/x-www-form-urlencoded", sessionRequest, "POST");
                Console.WriteLine(Output);

                Logout(sessionRequest);
            }
        }

        private static Tuple<string, string> AppTokenRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/authorize/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"app_id\":\"fr.freebox.testapp\"," +
                 "\"app_name\":\"Test app\"," +
                 "\"app_version\":\"1.0\"," +
                 "\"device_name\":\"PC-Tristan\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Regex token = new Regex("\"app_token\":\"(.[^\"]*)");
                    Regex trackid = new Regex("\"track_id\":(\\d*)");
                    return new Tuple<string, string>(token.Match(result).Groups[1].Value.ToString().Replace("\\/", "/"), trackid.Match(result).Groups[1].Value.ToString().Replace("\\/", "/"));
                }

            }
        }

        private static Tuple<string, string> AppTokenStatus(string trackid)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/authorize/" + trackid);
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Regex status = new Regex("\"status\":\"(.[^\"]*)");
                Regex challenge = new Regex("\"challenge\":\"(.[^\"]*)");
                return new Tuple<string, string>(status.Match(result).Groups[1].Value.ToString().Replace("\\/", "/"), challenge.Match(result).Groups[1].Value.ToString().Replace("\\/", "/"));
            }

        }

        private static string ChallengeRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/");
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Regex challenge = new Regex("\"challenge\":\"(.[^\"]*)");
                return challenge.Match(result).Groups[1].Value.ToString().Replace("\\/", "/");
            }

        }

        private static void Logout(string sessionid)
        {
            ApiCall("http://mafreebox.freebox.fr/api/v2/login/logout/", "", "application/json", sessionid, "POST");
        }

        private static string SessionRequest(string appid, string apptoken, string challenge)
        {
            string password = Encode(challenge, apptoken);


            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mafreebox.freebox.fr/api/v2/login/session/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"password\":\"" + password + "\"," +
                    "\"app_id\":\"" + appid + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        Regex session = new Regex("\"session_token\":\"(.[^\"]*)");
                        return session.Match(result).Groups[1].Value.ToString().Replace("\\/", "/");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return String.Empty;
                }

            }
        }


        private static string ApiCall(string url, string content, string contentType, string sessionid, string method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method;
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Headers.Add("X-Fbx-App-Auth", sessionid);

            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(content);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch
            {
            }


            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string Encode(string input, string key)
        {
            var encoding = new System.Text.UTF8Encoding();

            byte[] byteKey = encoding.GetBytes(key);
            byte[] byteInput = encoding.GetBytes(input);

            using (var hmacsha1 = new HMACSHA1(byteKey, false))
            {
                hmacsha1.Initialize();

                byte[] hashmessage = hmacsha1.ComputeHash(byteInput);
                return string.Concat(hashmessage.Select(b => string.Format("{0:X2}", b).ToLower()));
            }
        }
    }
}