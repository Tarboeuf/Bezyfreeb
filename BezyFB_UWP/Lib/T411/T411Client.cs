using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BezyFB_UWP.Lib.T411
{
    public class T411Client
    {
        public static string BaseAddress { get; set; }

        private readonly string _username;
        private readonly string _password;

        private string _token;

        private int _userId = -1;

        public bool IsTokenCreated
        {
            get { return !string.IsNullOrEmpty(_token); }
        }

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
            BaseAddress = "https://api.t411.ch/";
        }

        public async static Task<T411Client> New(string username, string password)
        {
            var t4 = new T411Client(username, password);
            await t4.Initialiser();
            return t4;
        }

        public T411Client(string username, string password)
        {
            _username = username;
            _password = password;
        }

        private async Task Initialiser()
        {
            if (string.IsNullOrEmpty(_token))
            {
                _token = await GetToken();
            }
        }

        private async Task<string> GetToken()
        {
            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                {
                    var dico = new Dictionary<string, string>();
                    dico.Add("username", _username);
                    dico.Add("password", _password);

                    HttpResponseMessage response = client.PostAsync("/auth", new FormUrlEncodedContent(dico)).Result;
                    
                    var tokResultBytes = await response.Content.ReadAsByteArrayAsync();
                    var tokResult = Encoding.GetEncoding("latin1").GetString(tokResultBytes);
                    try
                    {
                        var tokObj = JsonConvert.DeserializeObject<AuthResult>(tokResult);
                        string token = tokObj.Token;
                        return token;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
        }

        public async Task<List<Torrent>> GetTop100()
        {
            return await GetTorrents("/torrents/top/100");
        }

        public async Task<List<Torrent>> GetTopToday()
        {
            return await GetTorrents("/torrents/top/today");
        }

        public async Task<List<Torrent>> GetTopWeek()
        {
            return await GetTorrents("/torrents/top/week");
        }

        public async Task<List<Torrent>> GetTopMonth()
        {
            return await GetTorrents("/torrents/top/month");
        }

        private async Task<List<Torrent>> GetTorrents(string uri)
        {
            return await GetResponse<List<Torrent>>(new Uri(uri, UriKind.Relative));
        }

        public async Task<TorrentDetails> GetTorrentDetails(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/details/{0}", id);
            return await GetResponse<TorrentDetails>(new Uri(uri, UriKind.Relative));
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

        public async Task<UserDetails> GetUserDetails(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/users/profile/{0}", id);
            return await GetResponse<UserDetails>(new Uri(uri, UriKind.Relative));
        }

        public async Task<QueryResult> GetQuery(string query)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/search/{0}", query);
            return await GetResponse<QueryResult>(new Uri(uri, UriKind.Relative));
        }

        public async Task<QueryResult> GetQuery(string query, QueryOptions options)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (options == null)
                throw new ArgumentNullException("options");

            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/search/{0}?{1}", query.Replace(" ", "%20"), options.QueryString);
            return await GetResponse<QueryResult>(new Uri(uri, UriKind.Relative));
        }

        public async Task<Dictionary<int, Category>> GetCategory()
        {
            string uri = "/categories/tree";
            return await GetResponse<Dictionary<int, Category>>(new Uri(uri, UriKind.Relative));
        }

        public async Task<List<TermCategory>> GetTerms()
        {
            string uri = "/terms/tree";
            var result = await GetResponse<Dictionary<int, Dictionary<int, TermType>>>(new Uri(uri, UriKind.Relative));

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

        private async Task<T> GetResponse<T>(Uri uri)
        {
            await Initialiser();
            string data = await GetRawResponse(uri);

            if (data.StartsWith("{\"error\":"))
            {
                var error = JsonConvert.DeserializeObject<ErrorResult>(data);
                throw ErrorCodeException.CreateFromErrorCode(error);
            }

            T torrents = JsonConvert.DeserializeObject<T>(data);
            return torrents;
        }

        private async Task<string> GetRawResponse(Uri uri)
        {
            return await Task.Run(() => GetRawResponseAsync(uri, _token));
        }

        private static string GetRawResponseAsync(Uri uri, string token)
        {
            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    requestMessage.Headers.TryAddWithoutValidation("Authorization", token);

                    using (var response = client.SendAsync(requestMessage).Result)
                    {
                        var resultBytes = response.Content.ReadAsByteArrayAsync().Result;
                        var result = Encoding.GetEncoding("latin1").GetString(resultBytes);
                        return result;
                    }
                }
            }
        }

        public async Task<List<Torrent>> GetBookmarks()
        {
            return await GetResponse<List<Torrent>>(new Uri("/bookmarks", UriKind.Relative));
        }

        public async Task<int> CreateBookmark(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/bookmarks/save/{0}", id);
            return await GetResponse<int>(new Uri(uri, UriKind.Relative));
        }

        public async Task<int> DeleteBookmark(IEnumerable<int> bookmarkIds)
        {
            string ids = string.Join(",", bookmarkIds);
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/bookmarks/delete/{0}", ids);
            return await GetResponse<int>(new Uri(uri, UriKind.Relative));
        }
    }
}