using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Enums;

namespace PixiEditor.SVG.Units;

public struct SvgPreserveAspectRatioUnit : ISvgUnit
{
    public SvgAspectRatio Align { get; set; } = SvgAspectRatio.XMidYMid;
    public SvgMeetOrSlice MeetOrSlice { get; set; } = SvgMeetOrSlice.Meet;

    public SvgPreserveAspectRatioUnit(SvgAspectRatio align, SvgMeetOrSlice meetOrSlice)
    {
        Align = align;
        MeetOrSlice = meetOrSlice;
    }

    public string ToXml(DefStorage defs)
    {
        return $"{Align.ToString().Replace("X", "x")} {MeetOrSlice.ToString().ToLower()}";
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        string[] parts = readerValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            if (Enum.TryParse(parts[0], true, out SvgAspectRatio align))
            {
                Align = align;
            }
        }
        if (parts.Length > 1)
        {
            if (Enum.TryParse(parts[1], true, out SvgMeetOrSlice meetOrSlice))
            {
                MeetOrSlice = meetOrSlice;
            }
        }
    }
}
