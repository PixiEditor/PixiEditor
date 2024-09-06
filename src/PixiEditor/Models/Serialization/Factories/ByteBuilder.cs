using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public class ByteBuilder
{
    public byte[] Data { get; private set; }
    
    private List<byte> _data = new List<byte>();
    
    public ByteBuilder AddVecD(VecD vec)
    {
        _data.AddRange(BitConverter.GetBytes(vec.X));
        _data.AddRange(BitConverter.GetBytes(vec.Y));
        
        return this;
    }
    
    public ByteBuilder AddMatrix3X3(Matrix3X3 matrix)
    {
        foreach (var value in matrix.Values)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        } 
        
        return this;
    }
    
    public ByteBuilder AddColor(Color color)
    {
        _data.Add(color.R);
        _data.Add(color.G);
        _data.Add(color.B);
        _data.Add(color.A);
        
        return this;
    }
    
    public ByteBuilder AddInt(int value)
    {
        _data.AddRange(BitConverter.GetBytes(value));
        
        return this;
    }
    
    public ByteBuilder AddDouble(double value)
    {
        _data.AddRange(BitConverter.GetBytes(value));
        
        return this;
    }
    
    public byte[] Build()
    {
        Data = _data.ToArray();
        return Data;
    }

    public void AddVecDList(List<VecD> originalPoints)
    {
        AddInt(originalPoints.Count);
        foreach (var point in originalPoints)
        {
            AddVecD(point);
        }
    }
}
