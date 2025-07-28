namespace PixiEditor.Helpers;

public static class SpriteSheetUtility
{
    // calculate rows and columns so as little empty space is left
    // For example 3 frames should be in 3x1 grid because 2x2 would leave 1 empty space, but 4 frames should be in 2x2 grid
    public static (int rows, int columns) CalculateGridDimensionsAuto(int imagesLength)
    {
        int optimalRows = 1;
        int optimalColumns = imagesLength;
        int minDifference = Math.Abs(optimalRows - optimalColumns);

        for (int rows = 1; rows <= Math.Sqrt(imagesLength); rows++)
        {
            int columns = (int)Math.Ceiling((double)imagesLength / rows);

            if (rows * columns >= imagesLength)
            {
                int difference = Math.Abs(rows - columns);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    optimalRows = rows;
                    optimalColumns = columns;
                }
            }
        }

        return (Math.Max(optimalRows, 1), Math.Max(optimalColumns, 1));
    }
}
