using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditorDotNetCore3.Models.Position
{
    public class DoubleCords
    {
        public Coordinates Coords1 { get; set; }
        public Coordinates Coords2 { get; set; }

        public DoubleCords(Coordinates cords1, Coordinates cords2)
        {
            Coords1 = cords1;
            Coords2 = cords2;
        }
    }
}
