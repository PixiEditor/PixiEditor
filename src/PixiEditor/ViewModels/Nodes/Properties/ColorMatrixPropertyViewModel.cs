using System.ComponentModel;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class ColorMatrixPropertyViewModel : NodePropertyViewModel<ColorMatrix>
{
    private bool blockUpdates = false;
    public ColorMatrixPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
    
    public float M11
    {
        get => Value.M11;
        set => UpdateValue(new ColorMatrix(
            (value, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M12
    {
        get => Value.M12;
        set => UpdateValue(new ColorMatrix(
            (M11, value, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M13
    {
        get => Value.M13;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, value, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M14
    {
        get => Value.M14;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, value, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M15
    {
        get => Value.M15;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, value), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M21
    {
        get => Value.M21;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (value, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M22
    {
        get => Value.M22;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, value, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M23
    {
        get => Value.M23;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, value, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M24
    {
        get => Value.M24;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, value, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M25
    {
        get => Value.M25;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, value),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M31
    {
        get => Value.M31;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (value, M32, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M32
    {
        get => Value.M32;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, value, M33, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M33
    {
        get => Value.M33;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, value, M34, M35), (M41, M42, M43, M44, M45)));
    }

    public float M34
    {
        get => Value.M34;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, value, M35), (M41, M42, M43, M44, M45)));
    }

    public float M35
    {
        get => Value.M35;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, value), (M41, M42, M43, M44, M45)));
    }

    public float M41
    {
        get => Value.M41;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (value, M42, M43, M44, M45)));
    }

    public float M42
    {
        get => Value.M42;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, value, M43, M44, M45)));
    }

    public float M43
    {
        get => Value.M43;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, value, M44, M45)));
    }

    public float M44
    {
        get => Value.M44;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, value, M45)));
    }

    public float M45
    {
        get => Value.M45;
        set => UpdateValue(new ColorMatrix(
            (M11, M12, M13, M14, M15), (M21, M22, M23, M24, M25),
            (M31, M32, M33, M34, M35), (M41, M42, M43, M44, value)));
    }

    private void UpdateValue(ColorMatrix value)
    {
        if (blockUpdates)
            return;

        Value = value;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        blockUpdates = true;
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(M11));
            OnPropertyChanged(nameof(M12));
            OnPropertyChanged(nameof(M13));
            OnPropertyChanged(nameof(M14));
            OnPropertyChanged(nameof(M15));
            OnPropertyChanged(nameof(M21));
            OnPropertyChanged(nameof(M22));
            OnPropertyChanged(nameof(M23));
            OnPropertyChanged(nameof(M24));
            OnPropertyChanged(nameof(M25));
            OnPropertyChanged(nameof(M31));
            OnPropertyChanged(nameof(M32));
            OnPropertyChanged(nameof(M33));
            OnPropertyChanged(nameof(M34));
            OnPropertyChanged(nameof(M35));
            OnPropertyChanged(nameof(M41));
            OnPropertyChanged(nameof(M42));
            OnPropertyChanged(nameof(M43));
            OnPropertyChanged(nameof(M44));
            OnPropertyChanged(nameof(M45));
        }
        blockUpdates = false;
    }
}
