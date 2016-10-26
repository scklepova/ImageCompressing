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
            // min и max значения для каждой цветовой компоненты блока, плюс все пиксели, что принадлежат блоку
            var blocks = new List<Tuple<Color, Color, List<int>>>();
            var colors = GetColorsFromBytes(pixels);

            var min = Color.FromArgb(colors.Min(x => x.R), colors.Min(x => x.G), colors.Min(x => x.B));
            var max = Color.FromArgb(colors.Max(x => x.R), colors.Max(x => x.G), colors.Max(x => x.B));
            var indices = new List<int>();
            for(var i = 0; i < colors.Count; i++)
                indices.Add(i);

            blocks.Add(new Tuple<Color, Color, List<int>>(min, max, indices));

            for (var i = 0; i < k; i++)
            {
                var newBlocks = new List<Tuple<Color, Color, List<int>>>();
                foreach (var block in blocks)
                {
                    if (block.Item2.R - block.Item1.R >= block.Item2.G - block.Item1.G &&
                        block.Item2.R - block.Item1.R >= block.Item2.B - block.Item1.B)
                    {
                        var ordered = block.Item3.OrderBy(x => colors[x].R).ToList();
                        var median = GetMedian(ordered.Select(x => colors[x]), x => x.R);

                        newBlocks.Add(new Tuple<Color, Color, List<int>>(block.Item1, Color.FromArgb(median, block.Item2.G, block.Item2.B), ordered.GetRange(0, ordered.Count / 2)));
                        newBlocks.Add(new Tuple<Color, Color, List<int>>(Color.FromArgb(median, block.Item1.G, block.Item1.B), block.Item2, ordered.GetRange(ordered.Count / 2, ordered.Count - ordered.Count / 2)));
                    }
                    else if (block.Item2.G - block.Item1.G >= block.Item2.R - block.Item1.R &&
                             block.Item2.G - block.Item1.G >= block.Item2.B - block.Item1.B)
                    {
                        var ordered = block.Item3.OrderBy(x => colors[x].G).ToList();
                        var median = GetMedian(ordered.Select(x => colors[x]), x => x.G);

                        newBlocks.Add(new Tuple<Color, Color, List<int>>(block.Item1, Color.FromArgb(block.Item2.R, median, block.Item2.B), ordered.GetRange(0, ordered.Count / 2)));
                        newBlocks.Add(new Tuple<Color, Color, List<int>>(Color.FromArgb(block.Item1.R, median, block.Item1.B), block.Item2, ordered.GetRange(ordered.Count / 2, ordered.Count - ordered.Count / 2)));
                    }
                    else
                    {
                        var ordered = block.Item3.OrderBy(x => colors[x].B).ToList();
                        var median = GetMedian(ordered.Select(x => colors[x]), x => x.B);

                        newBlocks.Add(new Tuple<Color, Color, List<int>>(block.Item1, Color.FromArgb(block.Item2.R, block.Item2.G, median), ordered.GetRange(0, ordered.Count / 2)));
                        newBlocks.Add(new Tuple<Color, Color, List<int>>(Color.FromArgb(block.Item1.R, block.Item1.G, median), block.Item2, ordered.GetRange(ordered.Count / 2, ordered.Count - ordered.Count / 2)));
                    }
                }
                blocks = newBlocks;
            }

            foreach (var block in blocks)
            {
                var blockColors = block.Item3.Select(x => colors[x]).ToList();
                var median = Color.FromArgb(GetMedian(blockColors, x => x.R),
                    GetMedian(blockColors, x => x.G), GetMedian(blockColors, x => x.B));

                foreach ( var index in block.Item3)
                {
                    pixels[index*4] = median.R;
                    pixels[index*4 + 1] = median.G;
                    pixels[index*4 + 2] = median.B;
                }
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

        private static int GetMedian(IEnumerable<Color> colors, Func<Color, int> byComponent)
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