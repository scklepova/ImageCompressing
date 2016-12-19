using System.Linq;

namespace ImageCompressing.Helpers
{
    public class JpegQuantisor
    {
        public JpegQuantisor(int alpha, int gamma)
        {
            this.alpha = alpha;
            this.gamma = gamma;
            Q = new int[size][];
            for (var i = 0; i < size; i++)
            {
                Q[i] = new int[size];
                for (var j = 0; j < size; j++)
                    Q[i][j] = alpha*(1 + gamma*(i + j + 2));
            }
        }

        public int[][] SimpleQuantizing(int[][] matrix, int remainingCount)
        {
            var border = matrix.ToMyArray(size).OrderByDescending(x => x).ToArray()[remainingCount];
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    if (matrix[i][j] <= border)
                        matrix[i][j] = 0;
            return matrix;
        }

        public int[][] QQuantizing(int[][] matrix)
        {
            var ans = new int[size][];
            for(var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[i][j] = matrix[i][j]/Q[i][j];
            return ans;
        }

        public int[][] BackQQuantizing(int[][] matrix)
        {
            var ans = new int[size][];
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[i][j] = matrix[i][j] * Q[i][j];
            return ans;
        }

        public int[][] QuantizingRecommended(int[][] matrix, string channel)
        {
            var ans = new int[size][];
            var denomMatrix = channel == "Y" ? recommendedY : recommendedC;
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[i][j] = matrix[i][j] / denomMatrix[i][j];
            return ans;
        }

        public int[][] BackQuantizingRecommended(int[][] matrix, string channel)
        {
            var ans = new int[size][];
            var denomMatrix = channel == "Y" ? recommendedY : recommendedC;
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    ans[i][j] = matrix[i][j] * denomMatrix[i][j];
            return ans;
        }

        private int alpha, gamma;
        private int[][] Q;
        private const int size = 8;

        private int[][] recommendedY = new int[][]
        {
            new[] {16, 11, 10, 16, 24, 40, 51, 61},
            new[] {12, 12, 14, 19, 26, 58, 60, 55},
            new[] {14, 13, 16, 24, 40, 57, 69, 56},
            new[] {14, 17, 22, 29, 51, 87, 80, 62},
            new[] {18, 22, 37, 56, 68, 109, 103, 77},
            new[] {24, 35, 55, 64, 81, 104, 113, 92},
            new[] {49, 64, 78, 87, 103, 121, 120, 101},
            new[] {72, 92, 95, 98, 112, 100, 103, 99}
        };

        private int[][] recommendedC = new[]
        {
            new[] {17, 18, 24, 47, 99, 99, 99, 99},
            new[] {18, 21, 26, 66, 99, 99, 99, 99},
            new[] {24, 26, 56, 99, 99, 99, 99, 99},
            new[] {47, 66, 99, 99, 99, 99, 99, 99},
            new[] {99, 99, 99, 99, 99, 99, 99, 99},
            new[] {99, 99, 99, 99, 99, 99, 99, 99},
            new[] {99, 99, 99, 99, 99, 99, 99, 99},
            new[] {99, 99, 99, 99, 99, 99, 99, 99},
        };
    }
}