using ImageCompressing.Helpers;
using NUnit.Framework;

namespace ImageCompressing.Tests
{
    public class JpegTests
    {
        [Test]
        public void TestZigzag()
        {
            var matrix = new[]
            {
                new[] {1, 2, 6},
                new[] {3, 5, 7},
                new[] {4, 8, 9}
            };
            var array = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
            Assert.AreEqual(array, matrix.ToZigZagArray(3));
            Assert.AreEqual(matrix, array.ZigZagToMatrix(3));
        }

        [Test]
        public void TestZigzag2()
        {
            var matrix = new[]
            {
                new[] {1, 2, 6, 7},
                new[] {3, 5, 8, 13},
                new[] {4, 9, 12, 14},
                new[] {10, 11, 15, 16}
            };
            var array = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
            Assert.AreEqual(array, matrix.ToZigZagArray(4));
            Assert.AreEqual(matrix, array.ZigZagToMatrix(4));
        }
    }
}