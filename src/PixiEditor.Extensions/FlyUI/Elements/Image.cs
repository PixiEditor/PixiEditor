using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Extensions;
using PixiEditor.Extensions.FlyUI.Converters;
using PixiEditor.Extensions.UI;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Image : LayoutElement
{
    private string _source = null!;
    private double _width = -1;
    private double _height = -1;
    private FillMode _fillMode = FillMode.Uniform;
    private FilterQuality _filterQuality = FilterQuality.None;
    private Avalonia.Controls.Image? _image = null;
    private SvgImage? _svgImage = null;

    public string Source { get => _source; set => SetField(ref _source, value); }
    public double Width { get => _width; set => SetField(ref _width, value); }
    public double Height { get => _height; set => SetField(ref _height, value); }
    public FillMode FillMode { get => _fillMode; set => SetField(ref _fillMode, value); }

    public FilterQuality FilterQuality
    {
        get => _filterQuality;
        set
        {
            if (SetField(ref _filterQuality, value) && _image != null)
            {
                RenderOptions.SetBitmapInterpolationMode(_image, (BitmapInterpolationMode)(byte)FilterQuality);
            }
        }
    }

    protected override Control CreateNativeControl()
    {
        _image = new();

        Binding sourceBinding = new()
        {
            Source = this,
            Path = nameof(Source),
            Converter = new PathToImgSourceConverter(),
        };

        Binding widthBinding = new()
        {
            Source = this,
            Path = nameof(Width),
        };

        Binding heightBinding = new()
        {
            Source = this,
            Path = nameof(Height),
        };

        Binding fillModeBinding = new()
        {
            Source = this,
            Path = nameof(FillMode),
            Converter = new EnumToEnumConverter<FillMode, Stretch>()
        };

        _image.Bind(Avalonia.Controls.Image.SourceProperty, sourceBinding);
        _image.Bind(Layoutable.WidthProperty, widthBinding);
        _image.Bind(Layoutable.HeightProperty, heightBinding);
        _image.Bind(Avalonia.Controls.Image.StretchProperty, fillModeBinding);
        RenderOptions.SetBitmapInterpolationMode(_image, (BitmapInterpolationMode)(byte)FilterQuality);

        return _image;
    }


    protected override IEnumerable<object> GetControlProperties()
    {
        yield return Source;

        yield return Width;
        yield return Height;

        yield return FillMode;
        yield return FilterQuality;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        var valuesList = values.ToList();
        Source = (string)valuesList.ElementAtOrDefault(0);

        Width = (double)valuesList.ElementAtOrDefault(1, double.NaN);
        Height = (double)valuesList.ElementAtOrDefault(2, double.NaN);

        Width = Width < 0 ? double.NaN : Width;
        Height = Height < 0 ? double.NaN : Height;

        FillMode = (FillMode)valuesList.ElementAtOrDefault(3, FillMode.Uniform);
        FilterQuality = (FilterQuality)valuesList.ElementAtOrDefault(4, FilterQuality.Unspecified);
    }
}
