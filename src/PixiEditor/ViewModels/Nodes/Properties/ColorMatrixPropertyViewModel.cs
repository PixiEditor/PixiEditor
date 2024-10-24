using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class ColorMatrixPropertyViewModel : NodePropertyViewModel<ColorMatrix>
{
    public ColorMatrixPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
    
    public float M11
    {
        get => Value.M11;
        set => Value = new ColorMatrix(
            (value, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M12
    {
        get => Value.M12;
        set => Value = new ColorMatrix(
            (M11, value, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M13
    {
        get => Value.M13;
        set => Value = new ColorMatrix(
            (M11, M12, value, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M14
    {
        get => Value.M14;
        set => Value = new ColorMatrix(
            (M11, M12, M13, value, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M15
    {
        get => Value.M15;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, value), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M21
    {
        get => Value.M21;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (value, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M22
    {
        get => Value.M22;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, value, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M23
    {
        get => Value.M23;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, value, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M24
    {
        get => Value.M24;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, value, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M25
    {
        get => Value.M25;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, value),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M31
    {
        get => Value.M31;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (value, M32, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M32
    {
        get => Value.M32;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, value, M33, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M33
    {
        get => Value.M33;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, value, M34, M35), (M41, M42, M43, M44, M45));
    }

    public float M34
    {
        get => Value.M34;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, value, M35), (M41, M42, M43, M44, M45));
    }

    public float M35
    {
        get => Value.M35;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, value), (M41, M42, M43, M44, M45));
    }

    public float M41
    {
        get => Value.M41;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (value, M42, M43, M44, M45));
    }

    public float M42
    {
        get => Value.M42;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, value, M43, M44, M45));
    }

    public float M43
    {
        get => Value.M43;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, value, M44, M45));
    }

    public float M44
    {
        get => Value.M44;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, value, M45));
    }

    public float M45
    {
        get => Value.M45;
        set => Value = new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, value));
    }
}
