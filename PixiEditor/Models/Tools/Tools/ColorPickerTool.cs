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

        public ColorPickerTool(DocumentProvider documentProvider)
        {
            ActionDisplay = "Press on pixel to make it the primary color.";
            _docProvider = documentProvider;
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
            SKColor? color;
            Document activeDocument = _docProvider.GetDocument();
            Layer referenceLayer;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && (referenceLayer = _docProvider.GetReferenceLayer()) is not null)
            {
                double actualX = activeDocument.MouseXOnCanvas * referenceLayer.Width / activeDocument.Width;
                double actualY = activeDocument.MouseYOnCanvas * referenceLayer.Height / activeDocument.Height;

                x = (int)Round(actualX, MidpointRounding.ToZero);
                y = (int)Round(actualY, MidpointRounding.ToZero);

                color = referenceLayer.LayerBitmap.GetSRGBPixel(x, y);
            }
            else
            {
                color = _docProvider.GetRenderer()?.FinalSurface.GetSRGBPixel(x, y);
            }

            return color ?? SKColors.Transparent;
        }
    }
}
