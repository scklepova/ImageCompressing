using System;

namespace ImageCompressing.Helpers
{
    public static class MatrixExtensions
    {
        public static double[][] MultiplyBy(this double[][] target, double[][] matrix, int size)
        {
            var ans = new double[size][];
            for (var i = 0; i < size; i++)
            {
                ans[i] = new double[size];
                for (var j = 0; j < size; j++)
                {
                    double sum = 0;
                    for (var k = 0; k < size; k++)
                        sum += target[i][k]*matrix[k][j];
                    ans[i][j] = sum;
                }
            }
            return ans;
        }

        public static double[] MultiplyBy(this double[][] matrix, double[] vector, int size)
        {
            var ans = new double[size];
            for (var i = 0; i < size; i++)
            {
                ans[i] = matrix[i].MultiplyBy(vector, size).GetSum(size);
            }
            return ans;
        }

        public static double[][] GetTranspose(this double[][] target, int size)
        {
            var ans = new double[size][];
            for(var i = 0; i < size; i++)
                ans[i] = new double[size];
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[j][i] = target[i][j];
            return ans;
        }

        public static T[] ToMyArray<T>(this T[][] matrix, int size)
        {
            var ans = new T[size * size];
            for(var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[i*size + j] = matrix[i][j];
            return ans;
        }

        public static int[] ToZigZagArray(this int[][] matrix, int size)
        {
            var ans = new int[size * size];
            var i = 0;
            var j = 0;
            var k = 0;
            while (i < size && j < size)
            {
                ans[k] = matrix[i][j];
                k++;
                if (i == 0 || j == size - 1)
                {
                    if (j == size - 1) i++;
                    else j++;
                    if (j >= size)
                        break;
                    while (j > 0 && i < size - 1)
                    {
                        ans[k] = matrix[i][j];
                        i++;
                        j--;
                        k++;
                    } 
                }
                else if(j == 0 || i == size - 1)
                {
                    if (i == size - 1)
                        j++;
                    else i++;

                    if (i >= size)
                        break;
                    while (i > 0 && j < size - 1)
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

        public static T[] GetColumn<T>(this T[][] matrix, int size, int num)
        {
            var ans = new T[size];
            for (var i = 0; i < size; i++)
                ans[i] = matrix[i][num];
            return ans;
        }

        public static void SetColumn<T>(this T[][] matrix, T[] column, int size, int num)
        {
            for (var i = 0; i < size; i++)
                matrix[i][num] = column[i];
        }
    }
}