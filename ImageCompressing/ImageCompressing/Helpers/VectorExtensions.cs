namespace ImageCompressing.Helpers
{
    public static class VectorExtensions
    {
        public static double[] MultiplyBy(this double[] target, double[] factor, int size)
        {
            var ans = new double[size];
            for (var i = 0; i < size; i++)
                ans[i] = target[i]*factor[i];
            return ans;
        }

        public static double GetSum(this double[] vector, int size)
        {
            double sum = 0;
            for (var i = 0; i < size; i++)
            {
                sum += vector[i];
            }
            return sum;
        }
    }
}