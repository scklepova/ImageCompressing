using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Documents;

namespace ImageCompressing.Helpers
{
    public static class QuantizingMaster
    {
        public static byte[] UniformQuantization(byte[] pixels, int rBits, int gBits, int bBits)
        {
            for (var i = 0; i + 4 < pixels.Length; i += 4)
            {
                pixels[i] >>= 8 - rBits;
                pixels[i] <<= 8 - rBits;

                pixels[i + 1] >>= 8 - gBits;
                pixels[i + 1] <<= 8 - gBits;

                pixels[i + 2] >>= 8 - bBits;
                pixels[i + 2] <<= 8 - bBits;
            }
            return pixels;
        }

        public static byte[] MedianCut(byte[] pixels, int k)
        {
            var blocks = new List<Tuple<Color, Color, List<Color>>>();
            var colors = GetColorsFromBytes(pixels);

            var min = Color.FromArgb(colors.Min(x => x.R), colors.Min(x => x.G), colors.Min(x => x.B));
            var max = Color.FromArgb(colors.Max(x => x.R), colors.Max(x => x.G), colors.Max(x => x.B));
            blocks.Add(new Tuple<Color, Color, List<Color>>(min, max, colors));

            for (var i = 0; i < k; i++)
            {
                var newBlocks = new List<Tuple<Color, Color, List<Color>>>();
                foreach (var block in blocks)
                {
                    if (block.Item2.R - block.Item1.R >= block.Item2.G - block.Item1.G &&
                        block.Item2.R - block.Item1.R >= block.Item2.B - block.Item1.B)
                    {
                        var ordered = block.Item3.OrderBy(x => x.R).ToList();
                        var median = GetMedian(ordered, x => x.R);

                        newBlocks.Add(new Tuple<Color, Color, List<Color>>(block.Item1, Color.FromArgb(median, block.Item2.G, block.Item2.B), ordered.GetRange(0, ordered.Count / 2)));
                        newBlocks.Add(new Tuple<Color, Color, List<Color>>(Color.FromArgb(median, block.Item1.G, block.Item1.B), block.Item2, ordered.GetRange(ordered.Count / 2, ordered.Count - ordered.Count / 2)));
                    }
                    else if (block.Item2.G - block.Item1.G >= block.Item2.R - block.Item1.R &&
                             block.Item2.G - block.Item1.G >= block.Item2.B - block.Item1.B)
                    {
                        var ordered = block.Item3.OrderBy(x => x.G).ToList();
                        var median = GetMedian(ordered, x => x.G);

                        newBlocks.Add(new Tuple<Color, Color, List<Color>>(block.Item1, Color.FromArgb(block.Item2.R, median, block.Item2.B), ordered.GetRange(0, ordered.Count / 2)));
                        newBlocks.Add(new Tuple<Color, Color, List<Color>>(Color.FromArgb(block.Item1.R, median, block.Item1.B), block.Item2, ordered.GetRange(ordered.Count / 2, ordered.Count - ordered.Count / 2)));
                    }
                    else
                    {
                        var ordered = block.Item3.OrderBy(x => x.B).ToList();
                        var median = GetMedian(ordered, x => x.B);

                        newBlocks.Add(new Tuple<Color, Color, List<Color>>(block.Item1, Color.FromArgb(block.Item2.R, block.Item2.G, median), ordered.GetRange(0, ordered.Count / 2)));
                        newBlocks.Add(new Tuple<Color, Color, List<Color>>(Color.FromArgb(block.Item1.R, block.Item1.G, median), block.Item2, ordered.GetRange(ordered.Count / 2, ordered.Count - ordered.Count / 2)));
                    }
                }
                blocks = newBlocks;
            }

            var list =
                blocks.Select(
                    x =>
                        new Tuple<Color, Color, Color>(x.Item1, x.Item2,
                            Color.FromArgb(GetMedian(x.Item3, c => c.R), GetMedian(x.Item3, c => c.G), GetMedian(x.Item3, c => c.B)))).ToList();

            var index = 0;
            foreach(var color in colors)
            {
                var belong = false;
                foreach (var block in list)
                {
                    if (BelongsToBlock(color, block.Item1, block.Item2))
                    {
                        pixels[index] = block.Item3.R;
                        pixels[index + 1] = block.Item3.G;
                        pixels[index + 2] = block.Item3.B;
//                        index += 4;
                        belong = true;
                        break;
                    }
                    
                }
                if (!belong)
                {
                    pixels[index] = 0;
                    pixels[index + 1] = 0;
                    pixels[index + 2] = 0;
                }
                index += 4;
            }

            return pixels;
        }

        private static List<Color> GetColorsFromBytes(byte[] pixels)
        {
            var colors = new List<Color>();
            for(var i = 0; i < pixels.Length; i+=4)
                colors.Add(Color.FromArgb(pixels[i], pixels[i + 1], pixels[i+2]));
            return colors;
        }

        private static int GetMedian(List<Color> colors, Func<Color, int> byComponent)
        {
            var ordered = colors.OrderBy(byComponent).ToList();
            return ordered.Count % 2 == 0
                            ? byComponent(ordered[ordered.Count / 2])
                            : (byComponent(ordered[ordered.Count / 2]) + byComponent(ordered[ordered.Count / 2 + 1])) / 2;
        }

        private static bool BelongsToBlock(Color color, Color min, Color max)
        {
            return color.R >= min.R && color.R <= max.R &&
                   color.G >= min.G && color.G <= max.G &&
                   color.B >= min.B && color.B <= max.B;
        }

    }
}