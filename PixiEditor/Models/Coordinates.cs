using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models
{
    public class Coordinates
    {
        private int _X;

        public int X
        {
            get { return _X; }
            set { _X = value; }
        }

        private int _Y;

        public int Y
        {
            get { return _Y; }
            set { _Y = value; }
        }

        public Coordinates()
        {

        }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

    }
}
