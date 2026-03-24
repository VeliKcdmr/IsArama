using System.Security.Cryptography;
using System.Text;

namespace IsArama.Scraper.Services;

public class HashService
{
    public string Compute(string title, string company, string city)
    {
        var raw = $"{title.ToLower().Trim()}|{company.ToLower().Trim()}|{city.ToLower().Trim()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLower();
    }
}
