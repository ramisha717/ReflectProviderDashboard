using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace Reflect.Web.Extensions;

public static class CspExtensions
{
    public static string GetCspNonce(this HttpContext context)
    {
        const string key = "CspNonce";

        if (context.Items.TryGetValue(key, out var existing))
        {
            return (string)existing!;
        }

        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items[key] = nonce;

        return nonce;
    }
}
