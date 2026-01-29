namespace PixiEditor.ChangeableDocument.Helpers;

public static class StringHelper
{
    public static bool EqualSeparatedStrings(this string fullString, char separator, StringComparison comparison, ReadOnlySpan<string> segments)
    {
        ReadOnlySpan<char> fullSpan = fullString.AsSpan();
        int currentPos = 0;

        for (int i = 0; i < segments.Length; i++)
        {
            ReadOnlySpan<char> segment = segments[i].AsSpan();

            if (currentPos + segment.Length > fullSpan.Length)
                return false;

            var partToCompare = fullSpan.Slice(currentPos, segment.Length);
            if (!partToCompare.Equals(segment, comparison))
                return false;

            currentPos += segment.Length;

            if (i >= segments.Length - 1)
                continue;

            if (currentPos >= fullSpan.Length || fullSpan[currentPos] != separator)
                return false;
                
            currentPos++;
        }

        return currentPos == fullSpan.Length;
    }
    
    public static bool EqualSeparatedStrings(this string fullString, char separator, StringComparison comparison, string left, string right)
    {
        if (fullString.Length != left.Length + 1 + right.Length)
            return false;

        var span = fullString.AsSpan();

        if (span[left.Length] != separator)
            return false;

        return span.Slice(0, left.Length).Equals(left, comparison) &&
               span.Slice(left.Length + 1).Equals(right, comparison);
    }
}
