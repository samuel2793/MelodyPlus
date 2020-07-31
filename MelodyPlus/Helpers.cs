using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace MelodyPlus
{
    class Helpers
    {
        public static Avalonia.Media.Imaging.Bitmap GetBitmapFromImage(System.Drawing.Image image)
        {
            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
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
        public static Avalonia.Media.Imaging.Bitmap GetBitmapFromURL(string url)
        {
            Avalonia.Media.Imaging.Bitmap retVal = null;
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
                        retVal = new Avalonia.Media.Imaging.Bitmap(memstream);
                    }
                }
            }
            return retVal;
            //return GetBitmapFromImage(GetImageFromUri(url));
        }
        public static Image GetImageFromUri(string url)
        {
            Image retVal = null;
            if (!string.IsNullOrWhiteSpace(url))
            {
                var req = System.Net.WebRequest.Create(url.Trim());
                using (var request = req.GetResponse())
                {
                    using (var stream = request.GetResponseStream())
                    {
                        retVal = new Bitmap(Image.FromStream(stream));
                    }
                }
            }
            return retVal;
        }
    }
}
