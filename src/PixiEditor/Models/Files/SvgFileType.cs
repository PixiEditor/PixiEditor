﻿using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.Numerics;
using PixiEditor.SVG;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Features;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.Models.Files;

internal class SvgFileType : IoFileType
{
    public override string[] Extensions { get; } = new[] { ".svg" };
    public override string DisplayName { get; } = "Scalable Vector Graphics";
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Vector;

    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config, ExportJob? job)
    {
        job?.Report(0, string.Empty);
        SvgDocument svgDocument = document.ToSvgDocument(0, config.ExportSize, config.VectorExportConfig);

        job?.Report(0.5, string.Empty); 
        string xml = svgDocument.ToXml();

        job?.Report(0.75, string.Empty);
        await using FileStream fileStream = new(pathWithExtension, FileMode.Create);
        await using StreamWriter writer = new(fileStream);
        await writer.WriteAsync(xml);
        
        job?.Report(1, string.Empty);
        return SaveResult.Success;
    }
}