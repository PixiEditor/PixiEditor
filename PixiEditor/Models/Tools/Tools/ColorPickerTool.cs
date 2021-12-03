using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Services;
using PixiEditor.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static System.Math;

namespace PixiEditor.Models.Tools.Tools
{
    public class ColorPickerTool : ReadonlyTool
    {
        private readonly DocumentProvider _docProvider;
        private readonly BitmapManager _bitmapManager;

        public ColorPickerTool(DocumentProvider documentProvider, BitmapManager bitmapManager)
        {
            ActionDisplay = "Press on a pixel to make it the primary color. Hold Ctrl to pick from the reference. Hold Ctrl and Alt to blend the reference and canvas color";
            _docProvider = documentProvider;
            _bitmapManager = bitmapManager;
        }

        public override bool HideHighlight => true;

        public override string Tooltip => "Swaps primary color with selected on canvas. (O)";

        public override void Use(List<Coordinates> coordinates)
        {
            var coords = coordinates.First();
            ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = GetColorAt(coords.X, coords.Y);
        }

        public SKColor GetColorAt(int x, int y)
        {
            SKColor? color = null;
            Document activeDocument = _docProvider.GetDocument();
            Layer referenceLayer;

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                && (referenceLayer = _docProvider.GetReferenceLayer()) is not null)
            {
                double actualX = activeDocument.MouseXOnCanvas * referenceLayer.Width / activeDocument.Width;
                double actualY = activeDocument.MouseYOnCanvas * referenceLayer.Height / activeDocument.Height;

                x = (int)Round(actualX, MidpointRounding.ToZero);
                y = (int)Round(actualY, MidpointRounding.ToZero);

                color = referenceLayer.LayerBitmap.GetSRGBPixel(x, y);

                //if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && color != null)
                //{
                //    // TODO: Blend colors
                //    throw new NotImplementedException();
                //    //SKColor? canvasColor = _docProvider.GetRenderer()?.FinalSurface.GetSRGBPixel(x, y);
                //}
            }
            else
            {
                color = _docProvider.GetRenderer()?.FinalSurface.GetSRGBPixel(x, y);
            }

            return color ?? SKColors.Transparent;
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            UpdateActionDisplay();
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            UpdateActionDisplay();
        }

        public override void OnSelected()
        {
            UpdateActionDisplay();
        }

        public override void OnDeselected()
        {
            _bitmapManager.OnlyReferenceLayer = false;
            _bitmapManager.HideReferenceLayer = false;
        }

        private void UpdateActionDisplay()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) /*|| Keyboard.IsKeyDown(Key.RightCtrl)*/)
            {
                //if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                //{
                //    _bitmapManager.HideReferenceLayer = false;
                //    _bitmapManager.OnlyReferenceLayer = false;
                //    ActionDisplay = "Press on a pixel to make the blend of the reference and canvas the primary color. Release Ctrl and Alt to pick from the canvas. Release just Alt to pick from the reference";
                //    return;
                //}

                _bitmapManager.HideReferenceLayer = false;
                _bitmapManager.OnlyReferenceLayer = true;
                ActionDisplay = "Press on a pixel on the reference to make it the primary color. Release Ctrl to pick from the canvas. Hold Ctrl and Alt to blend the reference and canvas color";
            }
            else
            {
                _bitmapManager.HideReferenceLayer = true;
                _bitmapManager.OnlyReferenceLayer = false;
                ActionDisplay = "Press on a pixel to make it the primary color. Hold Ctrl to pick from the reference.";
            }
        }
    }
}
