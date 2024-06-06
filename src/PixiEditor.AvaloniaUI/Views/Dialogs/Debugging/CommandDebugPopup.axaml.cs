using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Evaluators;

namespace PixiEditor.AvaloniaUI.Views.Dialogs.Debugging;

public partial class CommandDebugPopup : PixiEditorPopup
{
    private static Brush infoBrush = new SolidColorBrush(Color.FromRgb(129, 143, 156));

    private static Brush warningBrush = new SolidColorBrush(Color.FromRgb(222, 130, 55));

    private static Brush errorBrush = new SolidColorBrush(Color.FromRgb(230, 34, 57));

    internal static readonly StyledProperty<IEnumerable<CommandDebug>> CommandsProperty = AvaloniaProperty.Register<CommandDebugPopup, IEnumerable<CommandDebug>>(
        "Commands");

    internal IEnumerable<CommandDebug> Commands
    {
        get => GetValue(CommandsProperty);
        set => SetValue(CommandsProperty, value);
    }

    public CommandDebugPopup()
    {
        var debugCommands = new List<CommandDebug>();

        foreach (var command in CommandController.Current.Commands)
        {
            var comments = new TextBlock { TextWrapping = TextWrapping.Wrap };

            IImage? image = null;
            Exception imageException = null;

            try
            {
                image = command.IconEvaluator.CallEvaluate(command, null);
            }
            catch (Exception e)
            {
                imageException = e;
            }

            var analysis = AnalyzeCommand(command, image, imageException, out int issues);

            foreach (var inline in analysis)
            {
                comments.Inlines.Add(inline);
            }

            debugCommands.Add(new CommandDebug(command, comments, image, issues));
        }

        Commands = debugCommands.OrderByDescending(x => x.Issues).ThenBy(x => x.Command.InternalName).ToArray();

        InitializeComponent();
    }

    private List<Inline> AnalyzeCommand(Command command, IImage? image, Exception? imageException, out int issues)
    {
        var inlines = new List<Inline>();
        issues = 0;

        if (imageException != null)
        {
            Error($"Icon evaluator throws exception\n{imageException}\n");
            issues++;
        }
        else
        {
            if (image == null && command.IconEvaluator == IconEvaluator.Default)
            {
                var expected = IconEvaluators.GetDefaultPath(command);

                if (string.IsNullOrWhiteSpace(command.Icon))
                {
                    Info(
                        $"Default evaluator has not found a image (No icon path provided). Expected at '{expected}'\n");
                }
                else
                {
                    Error($"Default evaluator has not found a image at icon path! Expected at '{expected}'.\n");
                    issues++;
                }
            }
        }

        if (command.IconEvaluator == null)
        {
            Warning("Icon evaluator is null");
        }
        else if (command.IconEvaluator != IconEvaluator.Default)
        {
            Info($"Uses custom icon evaluator ({command.IconEvaluator.Name})\n");
        }

        if (!string.IsNullOrWhiteSpace(command.Icon))
        {
            Info($"Has custom icon path: '{command.Icon}'\n");
        }

        return inlines;

        void Info(string text) => inlines.Add(new Run(text) { Foreground = infoBrush });

        void Warning(string text) => inlines.Add(new Run(text) { Foreground = warningBrush });

        void Error(string text) => inlines.Add(new Run(text) { Foreground = errorBrush });
    }

    internal class CommandDebug
    {
        public Command Command { get; }

        public TextBlock Comments { get; }

        public IImage Image { get; }

        public int Issues { get; }

        public CommandDebug(Command command, TextBlock comments, IImage image, int issues)
        {
            Command = command;
            Comments = comments;
            Image = image;
            Issues = issues;
        }
    }
}
