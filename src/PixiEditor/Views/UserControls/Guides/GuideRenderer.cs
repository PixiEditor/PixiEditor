using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.DataHolders.Guides;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Guides;

namespace PixiEditor.Views.UserControls.Guides
{
    internal class GuideRenderer : Control
    {
        public static readonly DependencyProperty GuideProperty =
            DependencyProperty.Register(nameof(Guide), typeof(Guide), typeof(GuideRenderer), new PropertyMetadata(GuideChanged));

        public Guide Guide
        {
            get => (Guide)GetValue(GuideProperty);
            set => SetValue(GuideProperty, value);
        }

        public static readonly DependencyProperty ZoomboxScaleProperty =
            DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(GuideRenderer), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public double ZoomboxScale
        {
            get => (double)GetValue(ZoomboxScaleProperty);
            set => SetValue(ZoomboxScaleProperty, value);
        }

        public double ScreenUnit => 1.0 / ZoomboxScale;

        protected override void OnRender(DrawingContext context)
        {
            Guide.Draw(context, this);
        }

        private static void GuideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var renderer = (GuideRenderer)d;

            if (e.OldValue is Guide oldGuide)
            {
                oldGuide.AttachRenderer(renderer);
            }

            if (e.NewValue is Guide newGuide)
            {
                newGuide.DetachRenderer(renderer);
            }
        }
    }
}
