using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    public class BrushTuple : NotifyableObject, ITuple
    {
        public object this[int index] => index switch
        {
            0 => FirstBrush,
            1 => SecondBrush,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };

        private Brush item1;

        public Brush FirstBrush
        {
            get => item1;
            set => SetProperty(ref item1, value);
        }

        private Brush item2;

        public Brush SecondBrush
        {
            get => item2;
            set => SetProperty(ref item2, value);
        }

        public int Length => 2;
    }
}