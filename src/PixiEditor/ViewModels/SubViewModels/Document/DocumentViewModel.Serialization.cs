using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Parser;
using TerraFX.Interop.Windows;

namespace PixiEditor.ViewModels.SubViewModels.Document;

internal partial class DocumentViewModel
{
    public SerializableDocument ToSerializable()
    {
        IReadOnlyDocument doc = Internals.Tracker.Document;

        SerializableDocument document = new SerializableDocument(Width, Height,
            ToSerializableGroups(doc.StructureRoot, doc),
            ToSerializableLayers(doc))
            .AddSwatches(Swatches)
            .AddPalette(Palette);

        return document;
    }

    private static List<SerializableLayer> ToSerializableLayers(IReadOnlyDocument document)
    {
        List<SerializableLayer> layers = new List<SerializableLayer>();
        
        document.ForEveryReadonlyMember(member =>
        {
            if (member is IReadOnlyLayer layer)
            {
                SerializableLayer serializable = ToSerializable(layer, document);
                if (serializable != null)
                {
                    layers.Add(serializable);
                }
            }
        });
        
        return layers;
    }

    private static SerializableLayer ToSerializable(IReadOnlyLayer layer, IReadOnlyDocument document)
    {
        var result = document.GetLayerImage(layer.GuidValue);

        if (result != null)
        {
            RectI tightBounds = document.GetLayerTightBounds(layer.GuidValue).Value;
            var serializable = new SerializableLayer(result.Size.X, result.Size.Y, tightBounds.X, tightBounds.Y)
                { IsVisible = layer.IsVisible, Name = layer.Name, Opacity = layer.Opacity };
            using var data = result.DrawingSurface.Snapshot().Encode();
            byte[] bytes = data.AsSpan().ToArray();
            serializable.PngBytes = bytes;
            
            return serializable;
        }

        return new SerializableLayer(1, 1) { Name = layer.Name, IsVisible = layer.IsVisible, Opacity = layer.Opacity };
    }

    private static List<SerializableGroup> ToSerializableGroups(IReadOnlyFolder documentStructureRoot, IReadOnlyDocument document, int passIndex = 0)
    {
        List<SerializableGroup> group = new List<SerializableGroup>();
        
        int currentLayerIndex = passIndex;
        foreach (var memberViewModel in documentStructureRoot.Children)
        {
            if (memberViewModel is IReadOnlyFolder folder && folder != document.StructureRoot)
            {
                int startIndex = currentLayerIndex;
                int endIndex = GetEndIndex(folder, startIndex);
                group.Add(new SerializableGroup(folder.Name, startIndex, endIndex, ToSerializableGroups(folder, document, startIndex)));        
            }
            else if(memberViewModel is IReadOnlyLayer)
            {
                currentLayerIndex++;
            }
        }
        
        return group;
    }

    private static int GetEndIndex(IReadOnlyFolder folder, int startIndex)
    {
        int endIndex = startIndex - 1;
        Traverse(folder, member =>
        {
            if (member is IReadOnlyLayer)
            {
                endIndex++;
            }
        });
        
        return endIndex;
    }
    
    private static void Traverse(IReadOnlyFolder folder, Action<IReadOnlyStructureMember> action)
    {
        action(folder);
        foreach (var member in folder.Children)
        {
            if (member is IReadOnlyLayer)
            {
                action(member);
            }
            if (member is IReadOnlyFolder subFolder)
            {
                Traverse(subFolder, action);
            }
        }
    }
}
