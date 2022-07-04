namespace PixiEditor.Models.Position;

public struct DoubleCoords
{
    public DoubleCoords(Coordinates cords1, Coordinates cords2)
    {
        Coords1 = cords1;
        Coords2 = cords2;
    }

    public Coordinates Coords1 { get; set; }

    public Coordinates Coords2 { get; set; }
}