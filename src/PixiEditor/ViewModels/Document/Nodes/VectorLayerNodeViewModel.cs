using System.ComponentModel;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
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

        isUpdatingProperties = true;

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
                    (data, value) => ((RectangleVectorData)data).Center = value,
                    new VecD(double.NegativeInfinity));

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
                    (data, value) => ((LineVectorData)data).Start = value,
                    new VecD(double.NegativeInfinity));

                AddVectorProperties(
                    "End",
                    "X",
                    "Y",
                    line.End,
                    data => ((LineVectorData)data).End,
                    (data, value) => ((LineVectorData)data).End = value,
                    new VecD(double.NegativeInfinity));
                break;

            case EllipseVectorData ellipse:
                AddVectorProperties(
                    "Center",
                    "X",
                    "Y",
                    ellipse.Center,
                    data => ((EllipseVectorData)data).Center,
                    (data, value) => ((EllipseVectorData)data).Center = value,
                    new VecD(double.NegativeInfinity));
                AddVectorProperties("Radius", "RADIUS_X", "RADIUS_Y", ellipse.Radius,
                    data => ((EllipseVectorData)data).Radius,
                    (data, value) => ((EllipseVectorData)data).Radius = value);
                break;

            case TextVectorData text:
                AddVectorProperties("Position", "POSITION_X", "POSITION_Y", text.Position,
                    data =>  ((TextVectorData)data).Position,
                    (data, value) => ((TextVectorData)data).Position = value);
                break;


            case ShapeVectorData shape:
                if (Document.PathOverlayViewModel.ActivePoints.Count > 0)
                {
                    int pointIndex = Document.PathOverlayViewModel.ActivePoints[0];
                    var point = GetPoint(pointIndex);

                    if (point != null)
                    {
                        AddVectorProperties(
                            "Point",
                            "POINT_X",
                            "POINT_Y",
                            (VecD)point.Position,
                            data => (VecD?)GetPoint(pointIndex, data)?.Position ?? VecD.Zero,
                            (data, value) =>
                            {
                                if (data is not PathVectorData pathData) return;

                                var editable = new EditableVectorPath(pathData.Path);
                                var subshape = editable.GetSubShapeContainingIndex(pointIndex);
                                if (subshape == null) return;

                                int localIndex = editable.GetSubShapePointIndex(pointIndex, subshape);
                                subshape.SetPointPosition(localIndex, (VecF)value, true);
                                pathData.Path = editable.ToVectorPath();
                            },
                            new VecD(double.NegativeInfinity));

                        if (point.Verb.VerbType == PathVerb.Conic)
                        {
                            AddCustomSizeProperty(
                                "ConicWeight",
                                "CONIC_WEIGHT",
                                point.Verb.ConicWeight,
                                0.001,
                                "",
                                3,
                                (data, value) =>
                                {
                                    UpdatePathPoint(data, pointIndex, p =>
                                    {
                                        p.Verb.ConicWeight = (float)value;
                                    });
                                });
                        }
                        else
                        {
                            var (incoming, outgoing) = GetAdjacentControlPoints(pointIndex);

                            if (incoming != null)
                            {
                                AddPathControlPointProperties("ControlPoint1",
                                    "CONTROL_POINT_1_X",
                                    "CONTROL_POINT_1_Y",
                                    incoming.Verb.ControlPoint2!.Value,
                                    false,
                                    incoming.Index);
                            }

                            if (outgoing != null)
                            {
                                AddPathControlPointProperties("ControlPoint2",
                                    "CONTROL_POINT_2_X",
                                    "CONTROL_POINT_2_Y",
                                    outgoing.Verb.ControlPoint1!.Value,
                                    true,
                                    outgoing.Index);
                            }
                        }
                    }
                }

                break;
        }

        AddCustomSizeProperty("Rotation", "RELATIVE_ROTATION", shapeData.TransformationCorners.RectRotation,
            double.MinValue, "°", 2,
            (data, value) =>
            {
                var vector = data;
                var matrix = vector.TransformationMatrix;

                var delta = float.DegreesToRadians((float)value) - (float)vector.TransformationCorners.RectRotation;

                vector.TransformationMatrix = matrix.PostConcat(
                    Matrix3X3.CreateRotation(
                        delta,
                        (float)vector.TransformationCorners.RectCenter.X,
                        (float)vector.TransformationCorners.RectCenter.Y));
            });
        Properties = properties.Values;
        PropertiesUpdated?.Invoke(Properties);

        isUpdatingProperties = false;
    }

    private (ShapePoint? Incoming, ShapePoint? Outgoing) GetAdjacentControlPoints(
        int pointIndex,
        ShapeVectorData? data = null)
    {
        var editable = new EditableVectorPath(
            (data as PathVectorData)?.Path ?? Document.PathOverlayViewModel.Path);

        var subshape = editable.GetSubShapeContainingIndex(pointIndex);

        if (subshape == null)
            return (null, null);

        int localIndex = editable.GetSubShapePointIndex(pointIndex, subshape);

        ShapePoint? incoming = null;
        ShapePoint? outgoing = null;

        var previous = subshape.GetPreviousPoint(pointIndex);

        if (previous.Verb.ControlPoint2.HasValue)
            incoming = previous;

        var current = subshape.Points[localIndex];

        if (current.Verb.ControlPoint1.HasValue)
            outgoing = current;

        return (incoming, outgoing);
    }

    private void AddPathControlPointProperties(string prefix, string xLocalizationKey, string yLocalizationKey,
        VecF initialValue, bool first, int ownerPointIndex)
    {
        AddVectorProperties(
            prefix,
            xLocalizationKey,
            yLocalizationKey,
            (VecD)initialValue,
            data =>
            {
                var owner = GetPoint(ownerPointIndex, data);

                var controlPoint = first
                    ? owner?.Verb.ControlPoint1
                    : owner?.Verb.ControlPoint2;

                return controlPoint is { } value
                    ? (VecD)value
                    : VecD.Zero;
            },
            (data, value) =>
            {
                UpdatePathPoint(data, ownerPointIndex, point =>
                {
                    if (first)
                        point.Verb.ControlPoint1 = (VecF)value;
                    else
                        point.Verb.ControlPoint2 = (VecF)value;
                });
            },
            new VecD(double.NegativeInfinity));
    }

    private void UpdatePathPoint(ShapeVectorData data, int pointIndex, Action<ShapePoint> update)
    {
        if (data is not PathVectorData pathData)
            return;

        var editable = new EditableVectorPath(pathData.Path);
        var subshape = editable.GetSubShapeContainingIndex(pointIndex);

        if (subshape == null)
            return;

        int localIndex = editable.GetSubShapePointIndex(pointIndex, subshape);
        var point = subshape.Points[localIndex];

        update(point);

        pathData.Path = editable.ToVectorPath();
    }


    private ShapePoint? GetPoint(int pointIndex, ShapeVectorData? data = null)
    {
        var editable = new EditableVectorPath((data as PathVectorData)?.Path ?? Document.PathOverlayViewModel.Path);
        var subshape = editable.GetSubShapeContainingIndex(pointIndex);
        var point = subshape?.Points[editable.GetSubShapePointIndex(pointIndex, subshape)];
        return point;
    }

    private void SelectedPointsChanged(IReadOnlyCollection<int> pointIndices)
    {
        RecreateProperties();
    }

    private void AddVectorProperties(
        string prefix,
        string xLocalizationKey,
        string yLocalizationKey,
        VecD value,
        Func<ShapeVectorData, VecD> getter,
        Action<ShapeVectorData, VecD> setter,
        VecD? min = null)
    {
        AddSizeProperty($"{prefix}X", xLocalizationKey, value.X,
            (data, x) =>
            {
                var current = getter(data);
                setter(data, new VecD(x, current.Y));
            }, min?.X ?? 0.00001);

        AddSizeProperty($"{prefix}Y", yLocalizationKey, value.Y,
            (data, y) =>
            {
                var current = getter(data);
                setter(data, new VecD(current.X, y));
            }, min?.Y ?? 0.00001);
    }

    private void AddSizeProperty(string name, string localizationKey, double value,
        Action<ShapeVectorData, double> setter, double min = 0.00001)
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
            ConverterData = Document.MeasurementConversionData,
            Min = min
        });
    }

    private void AddCustomSizeProperty(string name, string localizationKey, double value, double min, string unit,
        int decimals, Action<ShapeVectorData, double> setter)
    {
        properties.Add(name, new CustomSizePropertyViewModel(name, localizationKey, (newValue, state) =>
        {
            var cloned = (ShapeVectorData)GetShapeData(
                Document.AnimationDataViewModel.ActiveFrameTime).Clone();

            setter(cloned, newValue);
            UpdateGeometryData(cloned, state);
        }, value) { Min = min, Unit = unit, Decimals = decimals });
    }

    private void UpdateGeometryData(ShapeVectorData data, UpdateState state)
    {
        if (isUpdatingProperties) return;

        bool wasPathOverlayActive = Document.PathOverlayViewModel.IsActive;
        bool wasTransformOverlayActive = Document.TransformViewModel.TransformActive;
        bool wasLineToolActive = Document.LineToolOverlayViewModel.IsEnabled;
        bool wasTextOverlayActive = Document.TextOverlayViewModel.IsActive;

        Document.PathOverlayViewModel.IsActive = false;
        Document.TransformViewModel.TransformActive = false;
        Document.LineToolOverlayViewModel.IsEnabled = false;
        Document.TextOverlayViewModel.IsActive = false;

        if (state is UpdateState.OneShot or UpdateState.Updating)
        {
            Internals.ActionAccumulator.AddActions(
                new SetShapeGeometry_Action(Id, data, VectorShapeChangeType.GeometryData));
        }

        if (state is UpdateState.OneShot or UpdateState.Finish)
        {
            Internals.ActionAccumulator.AddFinishedActions(new EndSetShapeGeometry_Action());
        }

        if (state == UpdateState.Finish)
        {
            Document.LineToolOverlayViewModel.IsEnabled = wasLineToolActive;
            Document.PathOverlayViewModel.IsActive = wasPathOverlayActive;
            Document.TransformViewModel.TransformActive = wasTransformOverlayActive;
            Document.TextOverlayViewModel.IsActive = wasTextOverlayActive;
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

    public void TransformChanged()
    {
        UpdateProperties(true);
    }

    private void UpdateProperties(bool transformOnly = false)
    {
        if (isUpdatingProperties) return;
        isUpdatingProperties = true;
        var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameBindable);
        if (!transformOnly)
        {
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

                case TextVectorData text:
                    UpdateVectorProperties("Position", text.Position);
                    break;

                case PathVectorData shape:
                    if (Document.PathOverlayViewModel.ActivePoints.Count > 0)
                    {
                        int pointIndex = Document.PathOverlayViewModel.ActivePoints[0];
                        var point = GetPoint(pointIndex, shape);

                        if (point != null)
                        {
                            UpdateVectorProperties("Point", (VecD)point.Position);

                            if (point.Verb.VerbType == PathVerb.Conic)
                                UpdateSizeProperty("ConicWeight", point.Verb.ConicWeight);
                            else
                            {
                                var (incoming, outgoing) = GetAdjacentControlPoints(pointIndex, shape);
                                if (incoming != null)
                                    UpdateVectorProperties("ControlPoint1", (VecD)incoming.Verb.ControlPoint2!.Value);

                                if (outgoing != null)
                                    UpdateVectorProperties("ControlPoint2", (VecD)outgoing.Verb.ControlPoint1!.Value);
                            }
                        }
                    }

                    break;
            }
        }

        UpdateSizeProperty("Rotation", double.RadiansToDegrees(shapeData.TransformationCorners.RectRotation));

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
            UpdateSizeProp(property).SetValueWithoutSyncBack(value);
        }
        else
        {
            RecreateProperties();
        }
    }

    private PropertyViewModel<double> UpdateSizeProp(PropertyViewModel property)
    {
        if (property is SizePropertyViewModel sizeProp)
        {
            sizeProp.ConverterData = Document.MeasurementConversionData;
            sizeProp.MeasurementUnit = Document.MeasurementUnitBindable;
            return sizeProp;
        }

        if (property is CustomSizePropertyViewModel customSizeProp)
        {
            customSizeProp.ConverterData = Document.MeasurementConversionData;
            customSizeProp.MeasurementUnit = Document.MeasurementUnitBindable;
            return customSizeProp;
        }

        return null;
    }

    public override bool CanQuickColorChange()
    {
        var shapeData = GetShapeData(Document.AnimationDataViewModel.ActiveFrameTime);

        if (shapeData is null)
            return false;

        return Document.TransformViewModel.TransformActive;
    }

    void IPropertiesObject.StartUpdates()
    {
        Document.PropertyChanged -= DocumentOnPropertyChanged;
        Document.PropertyChanged += DocumentOnPropertyChanged;

        Document.PathOverlayViewModel.SelectedPointsChanged -= SelectedPointsChanged;
        Document.PathOverlayViewModel.SelectedPointsChanged += SelectedPointsChanged;
    }

    void IPropertiesObject.StopUpdates()
    {
        Document.PropertyChanged -= DocumentOnPropertyChanged;
        Document.PathOverlayViewModel.SelectedPointsChanged -= SelectedPointsChanged;
    }
}
