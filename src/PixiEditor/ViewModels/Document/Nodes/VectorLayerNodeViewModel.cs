
using System.ComponentModel;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Changes.Vectors;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Properties;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("VECTOR_LAYER", "STRUCTURE", PixiPerfectIcons.VectorPen)]
internal class VectorLayerNodeViewModel : StructureMemberViewModel<VectorLayerNode>, IVectorLayerHandler,
    IPropertiesObject, ITransformableMemberHandler
{
    public LocalizedString LabelKey => DisplayName;
    public IReadOnlyCollection<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
    public event Action<IReadOnlyCollection<PropertyViewModel>>? PropertiesUpdated;
    public event Action GeometryChanged;

    private Type? lastVectorType;
    private IReadOnlyVectorNode? cachedNode;
    private bool isUpdatingProperties;

    private Dictionary<string, PropertyViewModel> properties = new();

    private readonly Dictionary<Type, Type> quickToolsMap = new()
    {
        { typeof(IReadOnlyEllipseData), typeof(VectorEllipseToolViewModel) },
        { typeof(IReadOnlyRectangleData), typeof(VectorRectangleToolViewModel) },
        { typeof(IReadOnlyLineData), typeof(VectorLineToolViewModel) },
        { typeof(IReadOnlyTextData), typeof(TextToolViewModel) },
        { typeof(IReadOnlyPathData), typeof(VectorPathToolViewModel) }
    };

    private bool lockTransparency;

    public void SetLockTransparency(bool lockTransparency)
    {
        this.lockTransparency = lockTransparency;
        OnPropertyChanged(nameof(LockTransparencyBindable));
    }

    public bool LockTransparencyBindable
    {
        get => lockTransparency;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(Id, value));
        }
    }

    private bool shouldDrawOnMask;

    public bool ShouldDrawOnMask
    {
        get => shouldDrawOnMask;
        set
        {
            if (value == shouldDrawOnMask)
                return;

            shouldDrawOnMask = value;
            OnPropertyChanged(nameof(ShouldDrawOnMask));
        }
    }

    public Type? QuickEditTool
    {
        get
        {
            var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameTime);
            if (shapeData is null)
                return null;

            foreach (var tool in quickToolsMap)
            {
                if (shapeData.GetType().IsAssignableTo(tool.Key))
                    return tool.Value;
            }

            return null;
        }
    }

    public string LayerTypeIcon
    {
        get
        {
            var shapeData = GetShapeData(0);

            return shapeData switch
            {
                LineVectorData => PixiPerfectIcons.Line,
                TextVectorData => PixiPerfectIcons.TextAntialiased,
                RectangleVectorData => PixiPerfectIcons.Square,
                EllipseVectorData => PixiPerfectIcons.Circle,
                _ => PixiPerfectIcons.Tangent
            };
        }
    }

    public bool CanLockTransparency { get; } = false;

    public VectorLayerNodeViewModel()
    {
    }

    private void DocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.MeasurementConversionData) ||
            e.PropertyName == nameof(DocumentViewModel.MeasurementUnitBindable))
        {
            UpdateProperties();
        }
    }

    private void RecreateProperties()
    {
        if (Document == null)
            return;

        Document.PropertyChanged -= DocumentOnPropertyChanged;
        Document.PropertyChanged += DocumentOnPropertyChanged;

        properties = new Dictionary<string, PropertyViewModel>();

        var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameBindable);

        switch (shapeData)
        {
            case RectangleVectorData rectangle:
                AddVectorProperties(
                    "Center",
                    "X",
                    "Y",
                    rectangle.Center,
                    data => ((RectangleVectorData)data).Center,
                    (data, value) => ((RectangleVectorData)data).Center = value);

                AddSizeProperty(
                    "Width",
                    "WIDTH",
                    rectangle.Size.X,
                    (data, value) =>
                    {
                        var rectangleData = (RectangleVectorData)data;
                        rectangleData.Size = new VecD(value, rectangleData.Size.Y);
                    });

                AddSizeProperty(
                    "Height",
                    "HEIGHT",
                    rectangle.Size.Y,
                    (data, value) =>
                    {
                        var rectangleData = (RectangleVectorData)data;
                        rectangleData.Size = new VecD(rectangleData.Size.X, value);
                    });
                break;

            case LineVectorData line:
                AddVectorProperties(
                    "Start",
                    "X",
                    "Y",
                    line.Start,
                    data => ((LineVectorData)data).Start,
                    (data, value) => ((LineVectorData)data).Start = value);

                AddVectorProperties(
                    "End",
                    "X",
                    "Y",
                    line.End,
                    data => ((LineVectorData)data).End,
                    (data, value) => ((LineVectorData)data).End = value);
                break;

            case EllipseVectorData ellipse:
                AddVectorProperties(
                    "Center",
                    "X",
                    "Y",
                    ellipse.Center,
                    data => ((EllipseVectorData)data).Center,
                    (data, value) => ((EllipseVectorData)data).Center = value);
                AddVectorProperties("Radius", "RADIUS_X", "RADIUS_Y", ellipse.Radius,
                    data => ((EllipseVectorData)data).Radius,
                    (data, value) => ((EllipseVectorData)data).Radius = value);
                break;

            case TextVectorData text:
                break;
        }

        Properties = properties.Values;
        PropertiesUpdated?.Invoke(Properties);
    }

    private void AddVectorProperties(
        string prefix,
        string xLocalizationKey,
        string yLocalizationKey,
        VecD value,
        Func<ShapeVectorData, VecD> getter,
        Action<ShapeVectorData, VecD> setter)
    {
        AddSizeProperty( $"{prefix}X", xLocalizationKey, value.X,
            (data, x) =>
            {
                var current = getter(data);
                setter(data, new VecD(x, current.Y));
            });

        AddSizeProperty($"{prefix}Y", yLocalizationKey, value.Y,
            (data, y) =>
            {
                var current = getter(data);
                setter(data, new VecD(current.X, y));
            });
    }

    private void AddSizeProperty(
        string name,
        string localizationKey,
        double value,
        Action<ShapeVectorData, double> setter)
    {
        properties.Add(name, new SizePropertyViewModel(name, localizationKey, (newValue, state) =>
        {
            var cloned = (ShapeVectorData)GetShapeData(
                Document.AnimationDataViewModel.ActiveFrameTime).Clone();

            setter(cloned, newValue);
            UpdateGeometryData(cloned, state);
        }, value)
        {
            MeasurementUnit = Document.MeasurementUnitBindable,
            ConverterData = Document.MeasurementConversionData
        });
    }

    private void UpdateGeometryData(ShapeVectorData data, UpdateState state)
    {
        if (isUpdatingProperties) return;

        if (state == UpdateState.Updating)
        {
            Document.Operations.TryStopActiveExecutor();
        }

        Internals.ActionAccumulator.AddActions(
            new SetShapeGeometry_Action(Id, data, VectorShapeChangeType.GeometryData));

        if (state == UpdateState.Finish)
        {
            Internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());
            Document.Tools.UseLastKnownTool();
        }
    }

    public IReadOnlyShapeVectorData? GetShapeData(KeyFrameTime frameTime)
    {
        if (cachedNode == null)
        {
            cachedNode = (IReadOnlyVectorNode)Internals.Tracker.Document.FindMember(Id);
        }

        return cachedNode?.ShapeData;
    }

    public void VectorShapeChanged()
    {
        Type? currentType = GetShapeData(
            Document.AnimationDataViewModel.ActiveFrameTime)?.GetType();

        if (lastVectorType != currentType)
        {
            OnPropertyChanged(nameof(LayerTypeIcon));
            RecreateProperties();
            lastVectorType = currentType;
        }
        else
        {
            UpdateProperties();
        }

        GeometryChanged?.Invoke();
    }

    private void UpdateProperties()
    {
        isUpdatingProperties = true;
        var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameBindable);

        switch (shapeData)
        {
            case RectangleVectorData rectangle:
                UpdateVectorProperties("Center", rectangle.Center);
                UpdateSizeProperty("Width", rectangle.Size.X);
                UpdateSizeProperty("Height", rectangle.Size.Y);
                break;

            case LineVectorData line:
                UpdateVectorProperties("Start", line.Start);
                UpdateVectorProperties("End", line.End);
                break;

            case EllipseVectorData ellipse:
                UpdateVectorProperties("Center", ellipse.Center);
                UpdateVectorProperties("Radius", ellipse.Radius);
                break;
        }

        isUpdatingProperties = false;
    }

    private void UpdateVectorProperties(string prefix, VecD value)
    {
        UpdateSizeProperty($"{prefix}X", value.X);
        UpdateSizeProperty($"{prefix}Y", value.Y);
    }

    private void UpdateSizeProperty(string name, double value)
    {
        if (properties.TryGetValue(name, out var property))
        {
            UpdateSizeProp(property).Value = value;
        }
    }

    private SizePropertyViewModel UpdateSizeProp(PropertyViewModel property)
    {
        var sizeProp = (SizePropertyViewModel)property;
        sizeProp.ConverterData = Document.MeasurementConversionData;
        sizeProp.MeasurementUnit = Document.MeasurementUnitBindable;
        return sizeProp;
    }

    public override bool CanQuickColorChange()
    {
        var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameTime);

        if (shapeData is null)
            return false;

        return Document.TransformViewModel.TransformActive;
    }
}
