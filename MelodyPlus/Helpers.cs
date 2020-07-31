using System;
using System.IO;
using SixLabors.ImageSharp;

namespace MelodyPlus
{
    class Helpers
    {
        public static Avalonia.Media.Imaging.Bitmap GetBitmapFromImage(Image image)
        {
            using var stream = new MemoryStream();
            image.Save(stream,SixLabors.ImageSharp.Formats.Png.PngFormat.Instance);
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new Avalonia.Media.Imaging.Bitmap(stream);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static Image<SixLabors.ImageSharp.PixelFormats.Argb32> GetImageFromUri(string url)
        {
            Image<SixLabors.ImageSharp.PixelFormats.Argb32> retVal = null;
            if (!string.IsNullOrWhiteSpace(url))
            {
                var req = System.Net.WebRequest.Create(url.Trim());
                using (var request = req.GetResponse())
                {
                    using (var stream = request.GetResponseStream())
                    {
                        using var memstream = new MemoryStream();
                        stream.CopyTo(memstream);
                        memstream.Seek(0, SeekOrigin.Begin);
                        retVal = Image.Load(memstream).CloneAs<SixLabors.ImageSharp.PixelFormats.Argb32>();
                    }
                }
            }
            return retVal;
        }
    }
}
