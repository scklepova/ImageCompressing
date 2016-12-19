using System;

namespace ImageCompressing.Helpers
{
    public class DiscreteCosineTransformator
    {
        public DiscreteCosineTransformator()
        {
            M = new double[8][];
            var d = 1.0/Math.Sqrt(8.0);
            M[0] = new double[8];
            for (var j = 0; j < size; j++)
                M[0][j] = d;

            for (var i = 1; i < size; i++)
            {
                M[i] = new double[8];
                for (var j = 0; j < size; j++)
                    M[i][j] = 0.5*Math.Cos((2*j + 1)*i*Math.PI/16);
            }
            Mt = M.GetTranspose(size);
        }

        public int[][] DCT(byte[][] matrix)
        {
            var sourceDouble = new double[size][];
            for (var i = 0; i < size; i++)
            {
                sourceDouble[i] = new double[size];
                for (var j = 0; j < size; j++)
                    sourceDouble[i][j] = matrix[i][j]*1.0;
            }

            var temp = M.MultiplyBy(sourceDouble, size);
            var temp2 = temp.MultiplyBy(Mt, size);

            var ans = new int[size][];
            for (var i = 0; i < size; i++)
            {
                ans[i] = new int[size];
                for (var j = 0; j < size; j++)
                    ans[i][j] = (int) temp2[i][j];
            }
            return ans;
        }

        public int[][] InverseDCT(int[][] matrix)
        {
            var sourceDouble = new double[size][];
            for (var i = 0; i < size; i++)
            {
                sourceDouble[i] = new double[size];
                for (var j = 0; j < size; j++)
                    sourceDouble[i][j] = matrix[i][j]*1.0;
            }

            var temp = Mt.MultiplyBy(sourceDouble, size);
            var temp2 = temp.MultiplyBy(M, size);

            var ans = new int[size][];
            for (var i = 0; i < size; i++)
            {
                ans[i] = new int[size];
                for (var j = 0; j < size; j++)
                    ans[i][j] = (int) temp2[i][j];
            }
            return ans;
        }

        private const int size = 8;
        private readonly double[][] M;
        private readonly double[][] Mt;
    }
}