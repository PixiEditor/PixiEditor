using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PixiEditor.Helpers.Converters
{
    public class BrushTuple : NotifyableObject, ITuple
    {
        public object this[int index]
        {
            get
            {
                return index switch
                {
                    0 => FirstBrush,
                    1 => SecondBrush,
                    _ => throw new IndexOutOfRangeException("Index was out of range")
                };
            }
        }

        private Brush item1;

        public Brush FirstBrush
        {
            get => item1;
            set
            {
                item1 = value;
                RaisePropertyChanged(nameof(FirstBrush));
            }
        }

        private Brush item2;

        public Brush SecondBrush
        {
            get => item2;
            set
            {
                item2 = value;
                RaisePropertyChanged(nameof(SecondBrush));
            }
        }

        public int Length => 2;
    }
}