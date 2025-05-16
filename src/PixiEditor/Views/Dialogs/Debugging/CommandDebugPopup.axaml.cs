using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Commands.Evaluators;

namespace PixiEditor.Views.Dialogs.Debugging;

public partial class CommandDebugPopup : PixiEditorPopup
{
    private static Brush infoBrush = new SolidColorBrush(Color.FromRgb(129, 143, 156));

    private static Brush warningBrush = new SolidColorBrush(Color.FromRgb(222, 130, 55));

    private static Brush errorBrush = new SolidColorBrush(Color.FromRgb(230, 34, 57));

    internal static readonly StyledProperty<ObservableCollection<CommandDebug>> CommandsProperty = AvaloniaProperty.Register<CommandDebugPopup, ObservableCollection<CommandDebug>>(
        "Commands");

    internal ObservableCollection<CommandDebug> Commands
    {
        get => GetValue(CommandsProperty);
        set => SetValue(CommandsProperty, value);
    }

    private List<CommandDebug> allCommands = new List<CommandDebug>();

    public CommandDebugPopup()
    {
        allCommands = new List<CommandDebug>();

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

            allCommands.Add(new CommandDebug(command, comments, image, issues));
        }

        Commands = new ObservableCollection<CommandDebug>(allCommands.OrderByDescending(x => x.Issues)
            .ThenBy(x => x.Command.InternalName));

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

    private void ShowOnlyWithIssues_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            if (checkBox.IsChecked == true)
            {
                Commands = new ObservableCollection<CommandDebug>(Commands.Where(x => x.Issues > 0));
            }
            else
            {
                Commands = new ObservableCollection<CommandDebug>(allCommands);
            }
        }
    }

    private void ShowOnlyWithoutIcons_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            if (checkBox.IsChecked == true)
            {
                Commands = new ObservableCollection<CommandDebug>(Commands.Where(x => x.Image == null));
            }
            else
            {
                Commands = new ObservableCollection<CommandDebug>(allCommands);
            }
        }
    }

    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string filter = textBox.Text.ToLower();

            Commands = new ObservableCollection<CommandDebug>(allCommands
                .Where(x => x.Command.InternalName.ToLower().Contains(filter) ||
                            x.Command.DisplayName.ToString().ToLower().Contains(filter)));
        }
    }
}
