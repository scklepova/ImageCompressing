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


    }
}