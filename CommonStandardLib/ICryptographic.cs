namespace CommonStandardLib
{
    public interface ICryptographic
    {
        string EncodeTo64(string toEncode);
        string GetMd5Hash(string input);
        string Encode(string input, string key);
    }
}
