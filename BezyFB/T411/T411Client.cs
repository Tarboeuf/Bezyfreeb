using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;

namespace BezyFB.T411
{
    public class T411Client
    {
        public static string BaseAddress { get; set; }

        private readonly string _username;
        private readonly string _password;

        private readonly string _token;

        private int _userId = -1;

        public int UserId
        {
            get
            {
                if (_userId == -1)
                {
                    if (_token == null)
                        throw new InvalidOperationException("Null token");
                    string[] part = _token.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (part.Length != 3)
                        throw new InvalidOperationException("Invalide token format");

                    if (!int.TryParse(part[0], out _userId))
                        throw new InvalidOperationException("Invalide token user id format");
                }
                return _userId;
            }
        }

        static T411Client()
        {
            BaseAddress = "https://api.t411.me/";
        }

        public T411Client(string username, string password)
        {
            _username = username;
            _password = password;
            _token = GetToken();
        }

        private string GetToken()
        {
            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                {
                    Dictionary<string, string> dico = new Dictionary<string, string>();
                    dico.Add("username", _username);
                    dico.Add("password", _password);

                    HttpResponseMessage response = client.PostAsync("/auth", new FormUrlEncodedContent(dico)).Result;
                    var tokResult = response.Content.ReadAsStringAsync().Result;
                    var tokObj = JsonConvert.DeserializeObject<AuthResult>(tokResult);
                    string token = tokObj.Token;
                    return token;
                }
            }
        }

        public List<Torrent> GetTop100()
        {
            return GetTorrents("/torrents/top/100");
        }

        public List<Torrent> GetTopToday()
        {
            return GetTorrents("/torrents/top/today");
        }

        public List<Torrent> GetTopWeek()
        {
            return GetTorrents("/torrents/top/week");
        }

        public List<Torrent> GetTopMonth()
        {
            return GetTorrents("/torrents/top/month");
        }

        private List<Torrent> GetTorrents(string uri)
        {
            return GetResponse<List<Torrent>>(new Uri(uri, UriKind.Relative));
        }

        public TorrentDetails GetTorrentDetails(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/details/{0}", id);
            return GetResponse<TorrentDetails>(new Uri(uri, UriKind.Relative));
        }

        public Stream DownloadTorrent(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/download/{0}", id);

            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    requestMessage.Headers.TryAddWithoutValidation("Authorization", _token);

                    using (var response = client.SendAsync(requestMessage).Result)
                    {
                        MemoryStream ms = null;
                        try
                        {
                            ms = new MemoryStream();

                            using (var msLocal = new MemoryStream())
                            {
                                response.Content.CopyToAsync(msLocal).Wait();
                                msLocal.Position = 0;
                                StreamReader sr = new StreamReader(msLocal);

                                string data = sr.ReadToEnd();
                                if (data.StartsWith("{\"error\":"))
                                {
                                    ErrorResult error = JsonConvert.DeserializeObject<ErrorResult>(data);
                                    throw ErrorCodeException.CreateFromErrorCode(error);
                                }
                                msLocal.Position = 0;
                                msLocal.CopyTo(ms);
                            }

                            ms.Position = 0;
                            return ms;
                        }
                        catch
                        {
                            if (ms != null)
                            {
                                ms.Dispose();
                            }
                            throw;
                        }
                    }
                }
            }
        }

        public UserDetails GetUserDetails(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/users/profile/{0}", id);
            return GetResponse<UserDetails>(new Uri(uri, UriKind.Relative));
        }

        public QueryResult GetQuery(string query)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/search/{0}", query);
            return GetResponse<QueryResult>(new Uri(uri, UriKind.Relative));
        }

        public QueryResult GetQuery(string query, QueryOptions options)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (options == null)
                throw new ArgumentNullException("options");

            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/search/{0}?{1}", query.Replace(" ", "%20"), options.QueryString);
            return GetResponse<QueryResult>(new Uri(uri, UriKind.Relative));
        }

        public Dictionary<int, Category> GetCategory()
        {
            string uri = "/categories/tree";
            return GetResponse<Dictionary<int, Category>>(new Uri(uri, UriKind.Relative));
        }

        public List<TermCategory> GetTerms()
        {
            string uri = "/terms/tree";
            var result = GetResponse<Dictionary<int, Dictionary<int, TermType>>>(new Uri(uri, UriKind.Relative));

            List<TermCategory> list = new List<TermCategory>();
            foreach (var categoryItem in result)
            {
                TermCategory termCategory = new TermCategory();
                termCategory.Id = categoryItem.Key;
                termCategory.TermTypes = new List<TermType>();
                foreach (var termItem in categoryItem.Value)
                {
                    TermType termType = termItem.Value;
                    termType.Id = termItem.Key;
                    termCategory.TermTypes.Add(termType);
                }
                list.Add(termCategory);
            }

            return list;
        }

        private T GetResponse<T>(Uri uri)
        {
            string data = GetRawResponse(uri);

            if (data.StartsWith("{\"error\":"))
            {
                ErrorResult error = JsonConvert.DeserializeObject<ErrorResult>(data);
                throw ErrorCodeException.CreateFromErrorCode(error);
            }

            T torrents = JsonConvert.DeserializeObject<T>(data);
            return torrents;
        }

        private string GetRawResponse(Uri uri)
        {
            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    requestMessage.Headers.TryAddWithoutValidation("Authorization", _token);

                    using (var response = client.SendAsync(requestMessage).Result)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                }
            }
        }

        public List<Torrent> GetBookmarks()
        {
            return GetResponse<List<Torrent>>(new Uri("/bookmarks", UriKind.Relative));
        }

        public int CreateBookmark(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/bookmarks/save/{0}", id);
            return GetResponse<int>(new Uri(uri, UriKind.Relative));
        }

        public int DeleteBookmark(IEnumerable<int> bookmarkIds)
        {
            string ids = string.Join(",", bookmarkIds);
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/bookmarks/delete/{0}", ids);
            return GetResponse<int>(new Uri(uri, UriKind.Relative));
        }
    }
}