using System;

namespace ImageCompressing.Helpers
{
    public static class MatrixExtensions
    {
        public static double[][] MultiplyBy(this double[][] target, double[][] matrix, int size)
        {
            var ans = new double[size][];
            for(var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                {
                    double sum = 0;
                    for (var k = 0; k < size; k++)
                        sum += target[i][k]*matrix[k][j];
                    ans[i][j] = sum;
                }
            return ans;
        }

        public static double[][] GetTranspose(this double[][] target, int size)
        {
            var ans = new double[size][];
            for(var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[j][i] = target[i][j];
            return ans;
        }

        public static double[] ToArray(this double[][] matrix, int size)
        {
            var ans = new double[size * size];
            for(var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[i*size + j] = matrix[i][j];
            return ans;
        }

        public static double[] ZigZagToArray(this double[][] matrix, int size)
        {
            var ans = new double[size * size];
            var i = 0;
            var j = 0;
            var k = 0;
            while (i < size && j < size)
            {
                ans[k] = matrix[i][j];
                k++;
                if (i == 0)
                {
                    j++;
                    if (j >= size)
                        break;
                    while (j > 0 && i < size)
                    {
                        ans[k] = matrix[i][j];
                        i++;
                        j--;
                        k++;
                    } 
                }
                else
                {
                    i++;
                    if (i >= size)
                        break;
                    while (i > 0 && j < size)
                    {
                        ans[k] = matrix[i][j];
                        i--;
                        j++;
                        k++;
                    } 
                }
            }
            return ans;
        }
    }
}