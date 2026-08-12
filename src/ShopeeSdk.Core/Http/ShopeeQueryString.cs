using System.Globalization;
using System.Net;

namespace Loscardos.ShopeeSdk.Core.Http;

public static class ShopeeQueryString
{
    public static string Build(IReadOnlyDictionary<string, object?> values)
    {
        var pairs = new List<string>();

        foreach (var (key, value) in values)
        {
            if (value is null)
            {
                continue;
            }

            if (value is string text)
            {
                pairs.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(text)}");
                continue;
            }

            if (value is System.Collections.IEnumerable sequence and not byte[])
            {
                foreach (var item in sequence)
                {
                    if (item is not null)
                    {
                        pairs.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(Convert.ToString(item, CultureInfo.InvariantCulture))}");
                    }
                }

                continue;
            }

            pairs.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(Convert.ToString(value, CultureInfo.InvariantCulture))}");
        }

        return string.Join("&", pairs);
    }
}
