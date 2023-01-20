using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Views.Dialogs;

public partial class CommandDebugPopup : Window
{
    private static Brush infoBrush = new SolidColorBrush(Color.FromRgb(129, 143, 156));

    private static Brush warningBrush = new SolidColorBrush(Color.FromRgb(222, 130, 55));

    private static Brush errorBrush = new SolidColorBrush(Color.FromRgb(230, 34, 57));

    public static readonly DependencyProperty CommandsProperty =
        DependencyProperty.Register(nameof(Commands), typeof(IEnumerable<CommandDebug>), typeof(CommandDebugPopup));

    internal IEnumerable<CommandDebug> Commands
    {
        get => (IEnumerable<CommandDebug>)GetValue(CommandsProperty);
        set => SetValue(CommandsProperty, value);
    }

    public CommandDebugPopup()
    {
        var debugCommands = new List<CommandDebug>();

        foreach (var command in CommandController.Current.Commands)
        {
            var comments = new TextBlock();

            var image = command.IconEvaluator.CallEvaluate(command, null);

            foreach (var inline in AnalyzeCommand(command, image))
            {
                comments.Inlines.Add(inline);
            }

            debugCommands.Add(new CommandDebug(command, comments, image));
        }

        Commands = debugCommands;

        InitializeComponent();
        //ItemsControl.ItemsSource = CommandController.Current.Commands;
    }

    private IEnumerable<Inline> AnalyzeCommand(Command command, ImageSource image)
    {
        if (image == null && command.IconEvaluator == IconEvaluator.Default)
        {
            if (string.IsNullOrWhiteSpace(command.IconPath))
            {
                yield return Info("Default evaluator has not found a image (No icon path provided)");
            }
            else
            {
                yield return Error("Default evaluator has not found a image at icon path!");
            }
        }

        if (command.IconEvaluator != IconEvaluator.Default)
        {
            yield return Info("Uses custom icon evaluator");
        }

        if (!string.IsNullOrWhiteSpace(command.IconPath))
        {
            yield return Info($"Has custom icon path: '{command.IconPath}'");
        }

        Run Info(string text) => new Run(text) { Foreground = infoBrush };

        Run Warning(string text) => new Run(text) { Foreground = warningBrush };

        Run Error(string text) => new Run(text) { Foreground = errorBrush };
    }

    internal class CommandDebug
    {
        public Command Command { get; }

        public TextBlock Comments { get; }

        public ImageSource Image { get; }

        public CommandDebug(Command command, TextBlock comments, ImageSource image)
        {
            Command = command;
            Comments = comments;
            Image = image;
        }
    }
}

