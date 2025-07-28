using Avalonia.Headless.XUnit;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Skia;
using DrawiEngine;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Models.IO;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Models.Serialization.Factories.Paintables;
using PixiEditor.Parser.Skia.Encoders;

namespace PixiEditor.Tests;

public class SerializationTests : PixiEditorTest
{
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