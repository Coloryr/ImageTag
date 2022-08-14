using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
    public static BitmapImage GetBitmapImage(string imagePath)
    {
        FileStream fs = null;
        try
        {
            fs = new FileStream(imagePath, FileMode.Open);
            byte[] MyData = new byte[fs.Length];
            fs.Read(MyData, 0, (int)fs.Length);

            MemoryStream ms = new(MyData);
            BitmapImage bitmap = new();  //WPF
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            return bitmap;
        }
        catch (Exception ex)
        {
            // Exception treatment code here

            return null;
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }
    }
}
