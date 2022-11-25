namespace PixiEditor.Helpers;

internal static class SizeCalculator
{
    public static System.Drawing.Size CalcAbsoluteFromPercentage(float percentage, System.Drawing.Size currentSize)
    {
        float percFactor = percentage / 100f;
        float newWidth = currentSize.Width * percFactor;
        float newHeight = currentSize.Height * percFactor;
        return new System.Drawing.Size((int)MathF.Round(newWidth), (int)MathF.Round(newHeight));
    }

    public static int CalcPercentageFromAbsolute(int initAbsoluteSize, int currentAbsoluteSize)
    {
        return (int)((float)currentAbsoluteSize * 100) / initAbsoluteSize;
    }
}
