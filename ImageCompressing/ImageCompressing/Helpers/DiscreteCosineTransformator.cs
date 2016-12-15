using System;

namespace ImageCompressing.Helpers
{
    public class DiscreteCosineTransformator
    {
        public DiscreteCosineTransformator()
        {
            M = new double[8][];
            var d = 1.0/Math.Sqrt(8.0);
            for (var j = 0; j < size; j++)
                M[0][j] = d;

            for(var i = 1; i < size; i++)
                for (var j = 0; j < size; j++)
                    M[i][j] = 0.5*Math.Cos((2*j + 1)*i*Math.PI/16);
            Mt = M.GetTranspose(size);
        }

        public double[][] DCT(double[][] matrix)
        {
            var temp = M.MultiplyBy(matrix, size);
            return temp.MultiplyBy(Mt, size);
        }

        public double[][] InverseDCT(double[][] matrix)
        {
            var temp = Mt.MultiplyBy(matrix, size);
            return temp.MultiplyBy(M, size);
        }

        private const int size = 8;
        private readonly double[][] M;
        private readonly double[][] Mt;
    }
}