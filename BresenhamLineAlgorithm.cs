public static class BresenhamLineAlgorithm
{
    public static IEnumerable<Dimensions> GetPointsOnLine(Dimensions first, Dimensions second)
    {
        int x0 = first.X, y0 = first.Y;
        int x1 = second.X, y1 = second.Y;
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            int t;
            t = x0; // swap x0 and y0
            x0 = y0;
            y0 = t;
            t = x1; // swap x1 and y1
            x1 = y1;
            y1 = t;
        }
        if (x0 > x1)
        {
            int t;
            t = x0; // swap x0 and x1
            x0 = x1;
            x1 = t;
            t = y0; // swap y0 and y1
            y0 = y1;
            y1 = t;
        }
        int dx = x1 - x0;
        int dy = Math.Abs(y1 - y0);
        int error = dx / 2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            yield return new Dimensions((steep ? y : x), (steep ? x : y));
            error -= dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
        yield break;
    }
}