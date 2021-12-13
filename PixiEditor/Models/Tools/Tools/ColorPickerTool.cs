using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Services;
using PixiEditor.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.Tools.Tools
{
    internal class ColorPickerTool : ReadonlyTool
    {
        private readonly DocumentProvider _docProvider;
        private readonly BitmapManager _bitmapManager;
        private readonly string defaultActionDisplay = "Click to pick colors. Hold Ctrl to hide the canvas. Hold Shift to hide the reference layer";

        public ColorPickerTool(DocumentProvider documentProvider, BitmapManager bitmapManager)
        {
            ActionDisplay = defaultActionDisplay;
            _docProvider = documentProvider;
            _bitmapManager = bitmapManager;
        }

        public override bool HideHighlight => true;

        public override bool RequiresPreciseMouseData => true;

        public override string Tooltip => "Picks the primary color from the canvas. (O)";

        public override void Use(IReadOnlyList<Coordinates> recordedMouseMovement)
        {
            var coords = recordedMouseMovement[^1];
            var doc = _docProvider.GetDocument();
            if (coords.X < 0 || coords.Y < 0 || coords.X >= doc.Width || coords.Y >= doc.Height)
                return;

            ViewModelMain.Current.ColorsSubViewModel.PrimaryColor = GetColorAt(coords.X, coords.Y);
        }

        public SKColor GetColorAt(int x, int y)
        {
            Layer referenceLayer = _docProvider.GetReferenceLayer();

            if (referenceLayer != null && referenceLayer.IsVisible)
            {
                double preciseX = _docProvider.GetDocument().MouseXOnCanvas;
                double preciseY = _docProvider.GetDocument().MouseYOnCanvas;

                if (Session.IsCtrlDown)
                    return GetReferenceColor(preciseX, preciseY);
                if (Session.IsShiftDown)
                    return GetCanvasColor(x, y);
                return GetCombinedColor(x, y, preciseX, preciseY);
            }

            return GetCanvasColor(x, y);
        }

        private SKColor GetCombinedColor(int x, int y, double preciseX, double preciseY)
        {
            SKColor top = GetCanvasColor(x, y);
            SKColor bottom = GetReferenceColor(preciseX, preciseY);
            return BitmapUtils.BlendColors(bottom, top);
        }

        private SKColor GetCanvasColor(int x, int y)
        {
            return _docProvider.GetRenderer()?.FinalSurface.GetSRGBPixel(x, y) ?? SKColors.Transparent;
        }

        private SKColor GetReferenceColor(double x, double y)
        {
            Document activeDocument = _docProvider.GetDocument();
            Layer referenceLayer = _docProvider.GetReferenceLayer();
            Coordinates refPos = CanvasSpaceToReferenceSpace(
                    x, y,
                    activeDocument.Width, activeDocument.Height,
                    referenceLayer.Width, referenceLayer.Height);

            if (refPos.X >= 0 && refPos.Y >= 0 && refPos.X < referenceLayer.Width && refPos.Y < referenceLayer.Height)
                return referenceLayer.LayerBitmap.GetSRGBPixel(refPos.X, refPos.Y);
            return SKColors.Transparent;
        }

        private Coordinates CanvasSpaceToReferenceSpace(double canvasX, double canvasY, int canvasW, int canvasH, int referenceW, int referenceH)
        {
            double canvasRatio = canvasW / (double)canvasH;
            double referenceRatio = referenceW / (double)referenceH;
            bool blackBarsAreOnTopAndBottom = referenceRatio > canvasRatio;
            if (blackBarsAreOnTopAndBottom)
            {
                double combinedBlackBarsHeight = (1 - canvasRatio / referenceRatio) * canvasH;
                double refScale = referenceH / ((double)canvasH - combinedBlackBarsHeight);

                int outX = (int)Math.Floor(canvasX * referenceW / canvasW);
                int outY = (int)Math.Floor((canvasY - combinedBlackBarsHeight / 2) * refScale);

                return new Coordinates(outX, outY);
            }
            else
            {
                double combinedBlackBarsWidth = (1 - referenceRatio / canvasRatio) * canvasW;
                double refScale = referenceW / ((double)canvasW - combinedBlackBarsWidth);

                int outX = (int)Math.Floor((canvasX - combinedBlackBarsWidth / 2) * refScale);
                int outY = (int)Math.Floor(canvasY * referenceH / canvasH);
                return new Coordinates(outX, outY);
            }
        }

        public override void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown)
        {
            if (!IsActive)
            {
                _bitmapManager.HideReferenceLayer = false;
                _bitmapManager.OnlyReferenceLayer = false;
                return;
            }

            if (ctrlIsDown)
            {
                _bitmapManager.HideReferenceLayer = false;
                _bitmapManager.OnlyReferenceLayer = true;
                ActionDisplay = "Click to pick colors from the reference layer.";
            }
            else if (shiftIsDown)
            {
                _bitmapManager.HideReferenceLayer = true;
                _bitmapManager.OnlyReferenceLayer = false;
                ActionDisplay = "Click to pick colors from the canvas.";
                return;
            }
            else
            {
                _bitmapManager.HideReferenceLayer = false;
                _bitmapManager.OnlyReferenceLayer = false;
                ActionDisplay = defaultActionDisplay;
            }
        }
    }
}
