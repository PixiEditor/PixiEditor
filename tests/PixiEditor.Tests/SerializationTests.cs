using Avalonia.Headless.XUnit;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using Drawie.Skia;
using DrawiEngine;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Helpers;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.IO;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Models.Serialization.Factories.Paintables;
using PixiEditor.Parser.Skia.Encoders;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Tests;

public class SerializationTests : PixiEditorTest
{
    [Fact]
    public void TestThatAllFactoriesAreInServices()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.Where(asm => !asm.FullName.Contains("Steamworks")).SelectMany(x => x.GetTypes())
            .Where(x => typeof(SerializationFactory).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });

        var factoriesInAssemblies = types.ToList();

        var factoriesInActualServices = new ServiceCollection()
            .AddSerializationFactories()
            .Select(x => x.ImplementationType)
            .ToList();

        Assert.All(
            factoriesInAssemblies,
            expected => Assert.Contains(
                factoriesInActualServices,
                actual => actual == expected));
    }

    [Fact]
    public void TestThatAllPaintablesHaveFactories()
    {
        var allDrawiePaintableTypes = typeof(Paintable).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(Paintable))
                        && x is { IsAbstract: false, IsInterface: false }).ToList();

        var pixiEditorAssemblyPaintables = typeof(SerializationFactory).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(Paintable))
                        && x is { IsAbstract: false, IsInterface: false }).ToList();

        var allPaintables = allDrawiePaintableTypes.Concat(pixiEditorAssemblyPaintables).Distinct().ToList();

        var allFoundFactories = typeof(SerializationFactory).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IPaintableSerializationFactory))
                        && x is { IsAbstract: false, IsInterface: false }).ToList();

        List<SerializationFactory> factories = new();
        QoiEncoder encoder = new QoiEncoder();
        SerializationConfig config = new SerializationConfig(encoder, ColorSpace.CreateSrgbLinear());

        foreach (var factoryType in allFoundFactories)
        {
            var factory = (SerializationFactory)Activator.CreateInstance(factoryType);
            factories.Add(factory);
        }

        foreach (var type in allPaintables)
        {
            var factory = factories.FirstOrDefault(x => x.OriginalType == type);
            Assert.NotNull(factory);
        }
    }

    [Theory]
    [InlineData(typeof(double), "Double")]
    [InlineData(typeof(float), "Single")]
    [InlineData(typeof(int), "Int32")]
    [InlineData(typeof(string), "String")]
    [InlineData(typeof(bool), "Boolean")]
    [InlineData(typeof(Painter), "PixiEditor.Painter")]
    [InlineData(typeof(Brush), "PixiEditor.Brush")]
    [InlineData(typeof(VecI), "PixiEditor.VecI")]
    [InlineData(typeof(VecD), "PixiEditor.VecD")]
    [InlineData(typeof(TextVectorData), "PixiEditor.Text")]
    [InlineData(typeof(Texture), "PixiEditor.Texture")]
    [InlineData(typeof(Matrix3X3), "PixiEditor.Matrix3X3")]
    [InlineData(typeof(Paintable), "PixiEditor.Paintable")]
    [InlineData(typeof(VectorPath), "PixiEditor.VectorPath")]
    [InlineData(typeof(Color), "PixiEditor.Color")]
    [InlineData(typeof(FontFamilyName), "PixiEditor.FontFamilyName")]
    public void TestThatAllBlackboardVariableTypesReturnCorrectTypeName(Type type, string expectedName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var allFactories = assemblies.SelectMany(asm => asm.GetTypes()).Where(x => typeof(SerializationFactory).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
            .Select(x => (SerializationFactory)Activator.CreateInstance(x)).ToList();

        var name = SerializationUtil.GetWellKnownSerializationTypeName(type, allFactories);
        Assert.Equal(expectedName, name);
    }

    [Theory]
    [InlineData(typeof(double))]
    [InlineData(typeof(float))]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(Painter))]
    [InlineData(typeof(Brush))]
    [InlineData(typeof(VecI))]
    [InlineData(typeof(VecD))]
    [InlineData(typeof(TextVectorData))]
    [InlineData(typeof(Texture))]
    [InlineData(typeof(Matrix3X3))]
    [InlineData(typeof(Paintable))]
    [InlineData(typeof(VectorPath))]
    [InlineData(typeof(Color))] 
    public void TestThatWellKnownTypeNamesAreResolvedToCorrectTypes(Type expectedType)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var allFactories = assemblies.SelectMany(asm => asm.GetTypes()).Where(x => typeof(SerializationFactory).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
            .Select(x => (SerializationFactory)Activator.CreateInstance(x)).ToList();

        string typeName = SerializationUtil.GetWellKnownSerializationTypeName(expectedType, allFactories);
        Type resolvedType = SerializationUtil.GetTypeForWellKnownTypeName(typeName, allFactories);
        Assert.Equal(expectedType, resolvedType);
    }

    [Fact]
    public void TestTexturePaintableFactory()
    {
        Texture texture = new Texture(new VecI(32, 32));
        texture.DrawingSurface.Canvas.DrawCircle(16, 16, 16, new Paint() { Color = Colors.Red, BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.Src });
        TexturePaintable paintable = new TexturePaintable(texture);
        TexturePaintableSerializationFactory factory = new TexturePaintableSerializationFactory();
        factory.Config = new SerializationConfig(new QoiEncoder(), ColorSpace.CreateSrgbLinear());
        var serialized = factory.Serialize(paintable);
        var deserialized = (TexturePaintable)factory.Deserialize(serialized, default);

        Assert.NotNull(deserialized);
        var deserializedImage = deserialized.Image;
        Assert.NotNull(deserializedImage);
        Assert.Equal(texture.Size, deserializedImage.Size);
        for (int y = 0; y < texture.Size.Y; y++)
        {
            for (int x = 0; x < texture.Size.X; x++)
            {
                Color originalPixel = texture.GetSrgbPixel(new VecI(x, y));
                Color deserializedPixel = deserializedImage.GetSrgbPixel(new VecI(x, y));
                Assert.Equal(originalPixel, deserializedPixel);
            }
        }
    }

    [AvaloniaTheory]
    [InlineData("Fibi")]
    [InlineData("Pond")]
    [InlineData("SmlPxlCircShadWithMask")]
    [InlineData("SmallPixelArtCircleShadow")]
    [InlineData("SmlPxlCircShadWithMaskClipped")]
    [InlineData("SmlPxlCircShadWithMaskClippedInFolder")]
    [InlineData("VectorRectangleClippedToCircle")]
    [InlineData("VectorRectangleClippedToCircleShadowFilter")]
    [InlineData("VectorRectangleClippedToCircleMasked")]
    public void TestThatDeserializationOfSampleFilesDoesntThrow(string fileName)
    {
        string pixiFile = Path.Combine("TestFiles", "RenderTests", fileName + ".pixi");
        var document = Importer.ImportDocument(pixiFile);
        Assert.NotNull(document);
    }
}