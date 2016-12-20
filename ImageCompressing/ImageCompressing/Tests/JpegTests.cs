using System;
using ImageCompressing.Helpers;
using NUnit.Framework;

namespace ImageCompressing.Tests
{
    public class JpegTests
    {
        [Test]
        public void TestZigzag()
        {
            var matrix = new int[][]
            {
                new[] {1, 2, 6},
                new[] {3, 5, 7},
                new[] {4, 8, 9}
            };
             Assert.AreEqual(new [] {1, 2, 3, 4, 5, 6, 7, 8, 9}, matrix.ZigZagToArray(3));
        }

        [Test]
        public void TestZigzag2()
        {
            var matrix = new int[][]
            {
                new[] {1, 2, 6, 7},
                new[] {3, 5, 8, 13},
                new[] {4, 9, 12, 14},
                new[] {10, 11, 15, 16}
            };
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, matrix.ZigZagToArray(4));
        }
 
    }
}