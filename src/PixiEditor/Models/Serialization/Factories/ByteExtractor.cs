using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class ByteExtractor
{
    public int Position { get; private set; }
    
    private byte[] _data;
    
    public ByteExtractor(byte[] data)
    {
        _data = data;
    }
    
    public VecD GetVecD()
    {
        double x = BitConverter.ToDouble(_data, Position);
        double y = BitConverter.ToDouble(_data, Position + sizeof(double));
        
        Position += sizeof(double) * 2;
        
        return new VecD(x, y);
    }
    
    public Color GetColor()
    {
        byte r = _data[Position];
        byte g = _data[Position + 1];
        byte b = _data[Position + 2];
        byte a = _data[Position + 3];
        
        Position += 4;
        
        return new Color(r, g, b, a);
    }
    
    public int GetInt()
    {
        int value = BitConverter.ToInt32(_data, Position);
        
        Position += sizeof(int);
        
        return value;
    }

    public List<VecD> GetVecDList()
    {
        int count = GetInt();
        List<VecD> points = new List<VecD>();
        
        for (int i = 0; i < count; i++)
        {
            points.Add(GetVecD());
        }
        
        return points;
    }
}
