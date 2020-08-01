using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace DominantColor
{
    public class ColorUtils
    {
        internal static Dictionary<int, uint> GetColorHueHistogram<TPixel>(Image<TPixel> bmp, float saturationThreshold, float brightnessThreshold) where TPixel : unmanaged, IPixel<TPixel>
        {
            Dictionary<int, uint> colorHueHistorgram = new Dictionary<int, uint>();
            for (int i = 0; i <= 360; i++)
            {
                colorHueHistorgram.Add(i, 0);
            }
            Span<TPixel> pixels;
            bmp.TryGetSinglePixelSpan(out pixels);
            foreach (var pixel in pixels)
            {
                Rgba32 color = new Rgba32();
                pixel.ToRgba32(ref color);
                var hsv = new SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter().ToHsv(color);
                if(hsv.S > saturationThreshold && hsv.V > brightnessThreshold)
                {
                    int hue = (int)Math.Round(hsv.H, 0);
                    colorHueHistorgram[hue]++;
                }
            }
            return colorHueHistorgram;
        }
       

        /// <summary>
        /// Correct out of bound hue index
        /// </summary>
        /// <param name="hue">hue index</param>
        /// <returns>Corrected hue index (within 0-360 boundaries)</returns>
        private static int CorrectHueIndex(int hue)
        {
            int result = hue;
            if (result > 360)
                result = result - 360;
            if (result < 0)
                result = result + 360;
            return result;
        }

        /// <summary>
        /// Smooth histogram with given smoothfactor. 
        /// </summary>
        /// <param name="colorHueHistogram">The histogram to smooth</param>
        /// <param name="smoothFactor">How many hue neighbouring hue indexes will be averaged by the smoothing algoritme.</param>
        /// <returns>Smoothed hue color histogram</returns>
        internal static Dictionary<int, uint> SmoothHistogram(Dictionary<int, uint> colorHueHistogram, int smoothFactor)
        {
            if(smoothFactor < 0 || smoothFactor > 360)
                throw new ArgumentException("smoothFactor may not be negative or bigger then 360", nameof(smoothFactor));
            if (smoothFactor == 0)
                return new Dictionary<int, uint>(colorHueHistogram);
            
            Dictionary<int, uint> newHistogram = new Dictionary<int, uint>();
            int totalNrColumns = (smoothFactor * 2) + 1;
            for (int i = 0; i <= 360; i++)
            {
                uint sum = 0;
                uint average = 0;
                for(int x = i - smoothFactor;  x <= i + smoothFactor; x++)
                {
                    int hueIndex = CorrectHueIndex(x);
                    sum += colorHueHistogram[hueIndex];
                }
                average = (uint)(sum / totalNrColumns);
                newHistogram[i] = average;
            }
            return newHistogram;
        }
    }
}
