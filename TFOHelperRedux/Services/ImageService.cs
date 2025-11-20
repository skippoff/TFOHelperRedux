using System.IO;
using System.Windows.Media.Imaging;

namespace TFOHelperRedux.Services;

public static class ImageService
{
    public static BitmapImage? LoadImage(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }
}
