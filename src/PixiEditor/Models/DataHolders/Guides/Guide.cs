using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Guides;

namespace PixiEditor.Models.DataHolders.Guides
{
    internal abstract class Guide : NotifyableObject
    {
        private string name;
        private bool alwaysShowName;
        private bool showExtended;
        private bool showEditable;
        private List<GuideRenderer> renderers = new();

        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    InvalidateVisual();
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        public bool AlwaysShowName
        {
            get => alwaysShowName;
            set
            {
                if (SetProperty(ref alwaysShowName, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public bool ShowExtended
        {
            get => showExtended;
            set
            {
                if (SetProperty(ref showExtended, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public bool ShowEditable
        {
            get => showEditable;
            set
            {
                if (SetProperty(ref showEditable, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public abstract Control SettingsControl { get; }

        public abstract string TypeNameKey { get; }

        public virtual string IconPath => $"/Images/Guides/{GetType().Name}.png";

        public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name : TypeNameKey;

        protected IReadOnlyCollection<GuideRenderer> Renderers => renderers;

        public DocumentViewModel Document { get; }

        public Guide(DocumentViewModel document)
        {
            Document = document;
        }

        public abstract void Draw(DrawingContext context, GuideRenderer renderer);

        public void AttachRenderer(GuideRenderer renderer)
        {
            renderers.Add(renderer);
            RendererAttached(renderer);
        }

        public void DetachRenderer(GuideRenderer renderer)
        {
            renderers.Remove(renderer);
            RendererDetached(renderer);
        }

        protected virtual void RendererAttached(GuideRenderer renderer)
        { }

        protected virtual void RendererDetached(GuideRenderer renderer)
        { }

        protected void InvalidateVisual()
        {
            foreach (var renderer in renderers)
            {
                renderer.InvalidateVisual();
            }
        }
    }
}
