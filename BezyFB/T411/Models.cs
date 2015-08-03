using System;
using System.Collections.Generic;

namespace BezyFB.T411
{
    [System.Diagnostics.DebuggerDisplay("{Uid} / {Token}")]
    public class AuthResult
    {
        public string Uid { get; set; }

        public string Token { get; set; }
    }

    public enum Privacy
    {
        Low,
        Normal,
        Strong
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Name}")]
    public class Torrent
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Category { get; set; }

        public string RewriteName { get; set; }

        public int Seeders { get; set; }

        public int Leechers { get; set; }

        public int Comments { get; set; }

        public int IsVerified { get; set; }

        public DateTime Added { get; set; }

        public long Size { get; set; }

        public int Times_completed { get; set; }

        public int Owner { get; set; }

        public string CategoryName { get; set; }

        public string CategoryImage { get; set; }

        public string Username { get; set; }

        public Privacy? Privacy { get; set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Name}")]
    public class TorrentDetails
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Category { get; set; }

        public string Categoryname { get; set; }

        public string CategoryImage { get; set; }

        public string RewriteName { get; set; }

        public int Owner { get; set; }

        public string Username { get; set; }

        public Privacy Privacy { get; set; }

        public string Description { get; set; }

        public Dictionary<string, string> Terms { get; set; }

        public TorrentDetails()
        {
            Terms = new Dictionary<string, string>();
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Query} / {Offset}:{Limit}:{Total}")]
    public class QueryResult
    {
        public string Query { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public int Total { get; set; }

        public List<Torrent> Torrents { get; set; }

        public QueryResult()
        {
            Torrents = new List<Torrent>();
        }
    }

    public class QueryOptions
    {
        public int Offset { get; set; }

        public int Limit { get; set; }

        public List<int> CategoryIds { get; set; }

        public List<Term> Terms { get; set; }

        public QueryOptions()
        {
            Terms = new List<Term>();
            CategoryIds = new List<int>();
        }

        public string QueryString
        {
            get
            {
                List<string> parameters = new List<string>();
                if (Offset > 0)
                {
                    parameters.Add("offset=" + Offset);
                }
                if (Limit > 0)
                {
                    parameters.Add("limit=" + Limit);
                }
                if (CategoryIds != null && CategoryIds.Count > 0)
                {
                    foreach (int categoryId in CategoryIds)
                    {
                        parameters.Add("cid=" + categoryId);
                    }
                }
                if (Terms != null)
                {
                    foreach (var term in Terms)
                    {
                        string parameter = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                         "[{0}][]={1}", term.TermTypeId, term.Id);
                        parameters.Add(parameter);
                    }
                }

                return string.Join("&", parameters);
            }
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Username}")]
    public class UserDetails
    {
        public string Username { get; set; }

        public string Gender { get; set; }

        public int Age { get; set; }

        public string Avatar { get; set; }

        public long Downloaded { get; set; }

        public long Uploaded { get; set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Code} / {Error}")]
    public class ErrorResult
    {
        public string Error { get; set; }

        public int Code { get; set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Pid} / {Name}")]
    public class Category
    {
        public int Id { get; set; }

        public int Pid { get; set; }

        public string Name { get; set; }

        public Dictionary<int, Category> Cats { get; set; }

        public Category()
        {
            Cats = new Dictionary<int, Category>();
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Type} / {Mode}")]
    public class TermType
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public Mode Mode { get; set; }

        public Dictionary<int, string> Terms { get; set; }

        public List<Term> GetTerms
        {
            get
            {
                if (Terms == null)
                    return null;
                List<Term> result = new List<Term>();
                foreach (var term in Terms)
                {
                    result.Add(new Term { TermTypeId = Id, Id = term.Key, Name = term.Value });
                }
                return result;
            }
        }

        public TermType()
        {
            Terms = new Dictionary<int, string>();
        }
    }

    public class Term
    {
        public int Id { get; set; }

        public int TermTypeId { get; set; }

        public string Name { get; set; }
    }

    public class TermCategory
    {
        public int Id { get; set; }

        public List<TermType> TermTypes { get; set; }

        public TermCategory()
        {
            TermTypes = new List<TermType>();
        }
    }

    public enum Mode { Single, Multi }
}