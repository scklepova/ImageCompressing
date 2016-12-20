namespace ImageCompressing.Helpers
{
    public static class ArrayExtensions
    {
        public static int[][] ZigZagToMatrix(this int[] array, int matrixSize)
        {
            var ans = new int[matrixSize][];
            var i = 0;
            var j = 0;
            var k = 0;
            while (i < matrixSize && j < matrixSize)
            {
                if (ans[i] == null)
                    ans[i] = new int[matrixSize];
                ans[i][j] = array[k];
                k++;
                if (i == 0 || j == matrixSize - 1)
                {
                    if (j == matrixSize - 1) i++;
                    else j++;
                    if (j >= matrixSize)
                        break;
                    while (j > 0 && i < matrixSize - 1)
                    {
                        if(ans[i] == null)
                            ans[i] = new int[matrixSize];
                        ans[i][j] = array[k];
                        i++;
                        j--;
                        k++;
                    }
                }
                else if (j == 0 || i == matrixSize - 1)
                {
                    if (i == matrixSize - 1)
                        j++;
                    else i++;

                    if (i >= matrixSize)
                        break;
                    while (i > 0 && j < matrixSize - 1)
                    {
                        if (ans[i] == null)
                            ans[i] = new int[matrixSize];
                        ans[i][j] = array[k];
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