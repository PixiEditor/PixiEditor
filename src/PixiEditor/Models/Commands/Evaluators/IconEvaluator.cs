using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.Models.Commands.Evaluators;

internal class IconEvaluator : Evaluator<IImage>
{
    public static IconEvaluator Default { get; } = new FontIconEvaluator();

    public override IImage? CallEvaluate(Command command, object parameter)
    {
        return base.CallEvaluate(command, parameter is CommandSearchResult or Command ? parameter : command);
    }

    [DebuggerDisplay("IconEvaluator.Default")]
    private class FontIconEvaluator : IconEvaluator
    {

        public override IImage? CallEvaluate(Command command, object parameter)
        {
            string symbolCode = command.Icon;

            return PixiPerfectIconExtensions.ToIcon(symbolCode);
        }
    }
}
