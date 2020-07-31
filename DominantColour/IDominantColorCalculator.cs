using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DominantColor
{
    public interface IDominantColorCalculator
    {
        Color CalculateDominantColor<TPixel>(Image<TPixel> bitmap) where TPixel : unmanaged, IPixel<TPixel>;
    }
}
