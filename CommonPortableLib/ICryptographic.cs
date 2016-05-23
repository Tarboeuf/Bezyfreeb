using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPortableLib
{
    public interface ICryptographic
    {
        string EncodeTo64(string toEncode);
        string GetMd5Hash(string input);
        string Encode(string input, string key);
    }
}
