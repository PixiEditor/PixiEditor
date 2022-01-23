namespace PixiEditor.Helpers
{
    public static class SizeCalculator
    {
        public static System.Drawing.Size CalcAbsoluteFromPercentage(int percentage, System.Drawing.Size currentSize)
        {
            var percFactor = ((float)percentage) / 100f;
            var newWidth = currentSize.Width * percFactor;
            var newHeight = currentSize.Height * percFactor;
            return new System.Drawing.Size((int)newWidth, (int)newHeight);
        }
    }
}
