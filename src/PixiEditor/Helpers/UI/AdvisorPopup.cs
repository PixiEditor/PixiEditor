using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Helpers.Converters;
using PixiEditor.Models.AdvisorSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.UI;

public class AdvisorPopup : ContentPresenter
{
    public static readonly StyledProperty<Advice> AdviceProperty = AvaloniaProperty.Register<AdvisorPopup, Advice>(
        nameof(Advice));

    public Advice Advice
    {
        get => GetValue(AdviceProperty);
        set => SetValue(AdviceProperty, value);
    }

    public Control Anchor { get; }
    public ShowDirection Direction { get; }

    private const double Offset = 10;

    public AdvisorPopup(Control anchor, ShowDirection direction)
    {
        Anchor = anchor;
        Direction = direction;
    }

    private bool isContentCreated = false;

    public void CreateContent(Advice advice, bool isFollowUp = false)
    {
        Binding adviceBinding = new Binding(nameof(Advice) + "." + nameof(Advice.Content)) { Source = this };

        Grid grid = null;

        Border tbBorder = new Border()
        {
            Background = ResourceLoader.GetResource<IBrush>("ThemeBackgroundBrush"),
            BorderBrush = ResourceLoader.GetResource<IBrush>("ThemeBorderMidBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(5),
        };

        TextBlock textBlock = new TextBlock()
        {
            Margin = new Thickness(10), MaxWidth = 400, TextWrapping = TextWrapping.Wrap
        };

        TextBlock sizingTextBlock = new TextBlock()
        {
            Margin = new Thickness(10), MaxWidth = 400, TextWrapping = TextWrapping.Wrap, Opacity = 0
        };

        List<Button> choiceButtons = new List<Button>();
        if (advice.Choices != null)
        {
            for (var index = 0; index < advice.Choices.Count; index++)
            {
                var choice = advice.Choices[index];
                Button choiceButton = new Button();
                choiceButton.Classes.Add("AdviceChoiceButton");
                Translator.SetLocalizedString(choiceButton, choice);

                int capturedIndex = index;
                choiceButton.Click += (s, e) =>
                {
                    if (advice.ChoiceSelected != null)
                    {
                        advice.ChoiceSelected(capturedIndex);
                    }

                    NextAdvice(advice, capturedIndex, grid);
                };

                choiceButtons.Add(choiceButton);
            }
        }

        UniformGrid choicesGrid = new UniformGrid()
        {
            Columns = choiceButtons.Count, Margin = new Thickness(10, 0), ColumnSpacing = 10
        };

        choicesGrid.Children.AddRange(choiceButtons);

        TextBlock mushySays = new TextBlock()
        {
            Text = new LocalizedString(choiceButtons.Count > 0 ? "MUSHY_ASKS" : "MUSHY_SAYS"),
            FontStyle = FontStyle.Italic,
            Foreground = ResourceLoader.GetResource<IBrush>("ThemeForegroundLowBrush")
        };

        tbBorder.Child = new StackPanel()
        {
            Children = { mushySays, new Panel() { Children = { sizingTextBlock, textBlock } }, choicesGrid }
        };

        textBlock.Bind(Translator.LocalizedStringProperty, adviceBinding);
        sizingTextBlock.Bind(Translator.LocalizedStringProperty, adviceBinding);
        Image image = new Image
        {
            Source = ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Mushy.png"),
            Width = 64,
            Height = 64,
            Margin = new Thickness(0, 0, 5, 0),
        };

        CancellationTokenSource cts = new CancellationTokenSource();
        TypeTextAsync(textBlock, advice.Content, 15).ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    cts.Cancel();
                });
            }

            if (isFollowUp && (advice.Choices == null || advice.Choices.Count == 0) && advice.AutoDismiss)
            {
                DispatcherTimer.RunOnce(() =>
                {
                    advice.Dismiss();
                    HidePopup(grid);
                }, TimeSpan.FromSeconds(3));
            }
        });

        AddMushyAnimation(image, cts.Token);

        Grid.SetColumn(tbBorder, 1);

        string icon = advice.NextAdvice != null ? PixiPerfectIcons.ArrowRight : PixiPerfectIcons.Exit;

        Button closeButton = new Button()
        {
            Content = icon,
            Classes = { "pixi-icon" },
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Foreground = ResourceLoader.GetResource<IBrush>("ThemeForegroundLowBrush"),
            Padding = new Thickness(0),
            Margin = new Thickness(5)
        };

        Grid.SetColumn(closeButton, 0);
        Grid.SetColumnSpan(closeButton, 2);

        closeButton.Click += (s, e) =>
        {
            if (advice.NextAdvice != null)
            {
                CreateContent(advice.NextAdvice, true);
                return;
            }

            advice.Dismiss();
            HidePopup(grid);
        };

        grid = new Grid() { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Children = { image, tbBorder }, };

        if (!isFollowUp)
        {
            PopIn(grid);
        }

        if (advice.Choices == null || advice.Choices.Count == 0)
        {
            grid.Children.Add(closeButton);
        }

        var topLevel = TopLevel.GetTopLevel(Anchor);
        if (topLevel is not Window window)
            return;

        var transform = window.GetVisualDescendants().OfType<LayoutTransformControl>().FirstOrDefault()
            ?.LayoutTransform;

        Content = new LayoutTransformControl() { Child = grid, LayoutTransform = transform, UseRenderTransform = true };

        try
        {
            var layer = window.FindControl<Canvas>("AdvisorLayer");
            if (layer == null)
            {
                layer = new Canvas() { Name = "AdvisorLayer", IsHitTestVisible = true };
                var firstPanel = window.GetVisualDescendants().OfType<Panel>().FirstOrDefault();
                LayoutTransformControl? parentScaler = firstPanel?.GetVisualParent<LayoutTransformControl>();
                firstPanel?.Children.Add(layer);
                window.SizeChanged += (s, e) =>
                {
                    foreach (var child in layer.Children)
                    {
                        if (child is AdvisorPopup popup)
                        {
                            popup.SetPosition(window, parentScaler?.LayoutTransform);
                        }
                    }
                };

                if (firstPanel == null) return;
            }

            IsHitTestVisible = true;

            if (!isFollowUp)
            {
                layer.Children.Add(this);
            }

            isContentCreated = true;
        }
        catch
        {
            return;
        }
    }

    private void NextAdvice(Advice advice, int capturedIndex, Grid grid)
    {
        if (advice.NextAdvice == null)
        {
            advice.Dismiss(capturedIndex);
            HidePopup(grid!);
        }
        else
        {
            CreateContent(advice.NextAdvice, true);
        }
    }

    public void Show()
    {
        if (!isContentCreated)
        {
            CreateContent(Advice);
        }

        Dispatcher.UIThread.Post(() =>
        {
            var root = Anchor.GetVisualRoot();
            if (root is not Visual rootVisual)
            {
                return;
            }

            var parentScalerLayoutTransform = rootVisual.GetVisualParent<LayoutTransformControl>()?.LayoutTransform;

            SetPosition(root, parentScalerLayoutTransform);
            Anchor.DetachedFromVisualTree += AnchorOnDetachedFromVisualTree;
        }, DispatcherPriority.Render);
        this.IsVisible = true;
    }

    private void AnchorOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        this.IsVisible = false;
    }


    private void SetPosition(IRenderRoot? root, ITransform? parentScalerLayoutTransform)
    {
        if (root is not Visual rootVisual)
            return;

        var anchorTopLeft = Anchor.TranslatePoint(
            new Point(0, 0),
            rootVisual);

        if (parentScalerLayoutTransform != null)
        {
            anchorTopLeft = parentScalerLayoutTransform.Value.Transform(anchorTopLeft.Value);
        }

        if (!anchorTopLeft.HasValue)
            return;

        var anchorPoint = anchorTopLeft.Value;

        var anchorWidth = Anchor.Bounds.Width;
        var anchorHeight = Anchor.Bounds.Height;

        switch (Direction)
        {
            case ShowDirection.Left:
                Canvas.SetLeft(this, anchorPoint.X - Bounds.Width - Offset);
                Canvas.SetTop(this, anchorPoint.Y);
                break;

            case ShowDirection.Right:
                Canvas.SetLeft(this, anchorPoint.X + anchorWidth + Offset);
                Canvas.SetTop(this, anchorPoint.Y);
                break;

            case ShowDirection.Up:
                Canvas.SetLeft(this, anchorPoint.X);
                Canvas.SetTop(this, anchorPoint.Y - Bounds.Height - Offset);
                break;

            case ShowDirection.Down:
                Canvas.SetLeft(this, anchorPoint.X);
                Canvas.SetTop(this, anchorPoint.Y + anchorHeight + Offset);
                break;
        }
    }

    private void AddMushyAnimation(Image image, CancellationToken ctsToken)
    {
        var animation = new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromSeconds(1),
            IterationCount = new IterationCount(0, IterationType.Infinite),
            SpeedRatio = 1.5,
            Easing = new SineEaseInOut(),
        };


        var transformGroup = new TransformGroup();
        var rotateTransform = new RotateTransform();
        var scaleTransform = new ScaleTransform(1, 1);

        transformGroup.Children.Add(scaleTransform);
        transformGroup.Children.Add(rotateTransform);

        image.RenderTransform = transformGroup;
        image.RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative);

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0d),
            Setters =
            {
                new Setter(RotateTransform.AngleProperty, 0d),
                new Setter(ScaleTransform.ScaleXProperty, 1d),
                new Setter(ScaleTransform.ScaleYProperty, 1d)
            }
        });

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0.25d),
            Setters =
            {
                new Setter(RotateTransform.AngleProperty, -5d),
                new Setter(ScaleTransform.ScaleXProperty, 1.1d),
                new Setter(ScaleTransform.ScaleYProperty, 0.9d)
            }
        });

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0.75d),
            Setters =
            {
                new Setter(RotateTransform.AngleProperty, 5d),
                new Setter(ScaleTransform.ScaleXProperty, 0.9d),
                new Setter(ScaleTransform.ScaleYProperty, 1.1d)
            }
        });

        animation.RunAsync(image, ctsToken);
    }

    public static async Task TypeTextAsync(
        TextBlock textBlock,
        string fullText,
        int delayMs = 35,
        CancellationToken cancellationToken = default)
    {
        textBlock.Text = string.Empty;

        for (int i = 0; i <= fullText.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            textBlock.Text = fullText.Substring(0, i);
            await Task.Delay(delayMs, cancellationToken);
        }
    }

    private void PopIn(Control control)
    {
        var scale = new ScaleTransform(0, 0);
        control.RenderTransform = scale;
        control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        var animation = new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(500), Easing = new QuinticEaseOut(), FillMode = FillMode.Forward
        };

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1d),
            Setters =
            {
                new Setter(ScaleTransform.ScaleXProperty, 1d), new Setter(ScaleTransform.ScaleYProperty, 1d)
            }
        });

        _ = animation.RunAsync(control);
    }

    private void HidePopup(Control control)
    {
        var scale = new ScaleTransform(1, 1);
        control.RenderTransform = scale;
        control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        var animation = new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(500), Easing = new QuinticEaseOut(), FillMode = FillMode.Forward
        };

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1d),
            Setters =
            {
                new Setter(ScaleTransform.ScaleXProperty, 0d), new Setter(ScaleTransform.ScaleYProperty, 0d)
            }
        });

        if (Anchor != null)
        {
            Anchor.DetachedFromVisualTree -= AnchorOnDetachedFromVisualTree;
        }

        _ = animation.RunAsync(control).ContinueWith(t =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                this.IsVisible = false;
            });
        });
    }
}
