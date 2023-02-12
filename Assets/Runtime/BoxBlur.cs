using UnityEngine;

public static class BoxBlur
{
    public static void FastBoxBlur(float[,] grid, int range, int iterations = 1)
    {
        if (range == 0 || iterations == 0)
        {
            return;
        }

        // A radius from the center
        range = range * 2 + 1;
        
        for (int i = 0; i < iterations; i++)
        {
            BoxBlurHorizontal(grid, range);
            BoxBlurVertical(grid, range);
        }
    }

    private static void BoxBlurHorizontal(float[,] grid, int range)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);
        int halfRange = range / 2;
        float[] temp = new float[w];

        for (int y = 0; y < h; y++)
        {
            int hits = 0;
            float val = 0;
            for (int x = -halfRange; x < w; x++)
            {
                int oldPixel = x - halfRange - 1;
                if (oldPixel >= 0)
                {
                    val -= grid[oldPixel, y];
                    hits--;
                }

                int newPixel = x + halfRange;
                if (newPixel < w)
                {
                    val += grid[newPixel, y];
                    hits++;
                }

                if (x >= 0)
                {
                    temp[x] = val / hits;
                }
            }

            for (int x = 0; x < w; x++)
            {
                // Clamp due to numerical inaccuracies
                grid[x, y] = Mathf.Clamp01(temp[x]);
            }
        }
    }

    private static void BoxBlurVertical(float[,] grid, int range)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);
        int halfRange = range / 2;

        float[] temp = new float[h];
        for (int x = 0; x < w; x++)
        {
            int hits = 0;
            float val = 0;
            for (int y = -halfRange; y < h; y++)
            {
                int oldPixel = y - halfRange - 1;
                if (oldPixel >= 0)
                {
                    val -= grid[x, oldPixel];
                    hits--;
                }

                int newPixel = y + halfRange;
                if (newPixel < h)
                {
                    val += grid[x, newPixel];
                    hits++;
                }

                if (y >= 0)
                {
                    temp[y] = val / hits;
                }
            }

            for (int y = 0; y < h; y++)
            {
                // Clamp due to numerical inaccuracies
                grid[x, y] = Mathf.Clamp01(temp[y]);
            }
        }
    }
}