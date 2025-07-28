using System.Diagnostics;
using Avalonia.Media;

namespace PixiEditor.UI.Common.Localization;

[DebuggerDisplay("{LanguageData.Name}, strings: {Locale.Count}")]
public class Language
{
    public static bool FlipFlowDirection { get; set; } = false;

    private FlowDirection flowDirection;
    
    public LanguageData LanguageData { get; }
    public IReadOnlyDictionary<string, string> Locale { get; }

    public FlowDirection FlowDirection
    {
        get
        {
            if (FlipFlowDirection)
            {
                return flowDirection switch
                {
                    FlowDirection.RightToLeft => FlowDirection.LeftToRight,
                    FlowDirection.LeftToRight => FlowDirection.RightToLeft
                };
            }

            return flowDirection;
        }
    }
    
    public Language(LanguageData languageData, Dictionary<string, string> locale, bool isRightToLeft)
    {
        LanguageData = languageData;
        Locale = locale;
        flowDirection = isRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }
}
