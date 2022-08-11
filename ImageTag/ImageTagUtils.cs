using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ImageTag;

internal static class ImageTagUtils
{
    public static string GetSHA1(string data)
    {
        return BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "");
    }
    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower().Replace("-", "");
    }
}
