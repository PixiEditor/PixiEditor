using Nodes;
using Nodes.Nodes;
using SkiaSharp;

LayerNode layerNode = new LayerNode("Layer", new SKSizeI(100, 100));

SKBitmap testBitmap = SKBitmap.Decode("test.png");
using SKSurface surface = SKSurface.Create(new SKImageInfo(100, 100));
using SKCanvas canvas = surface.Canvas;
canvas.DrawBitmap(testBitmap, 0, 0);

layerNode.LayerImage.Canvas.DrawSurface(surface, 0, 0);

LayerNode layerNode2 = new LayerNode("Layer2", new SKSizeI(100, 100));

SKBitmap testBitmap2 = SKBitmap.Decode("test2.png");
using SKSurface surface2 = SKSurface.Create(new SKImageInfo(100, 100));
using SKCanvas canvas2 = surface2.Canvas;
canvas2.DrawBitmap(testBitmap2, 0, 0);

layerNode2.LayerImage.Canvas.DrawSurface(surface2, 0, 0);

MergeNode mergeNode = new MergeNode("Merge");
OutputNode outputNode = new OutputNode("Output");

layerNode.Output.ConnectTo(mergeNode.Top);
layerNode2.Output.ConnectTo(mergeNode.Bottom);
mergeNode.Output.ConnectTo(outputNode.Input);

NodeGraph graph = new();
graph.AddNode(layerNode);
graph.AddNode(layerNode2);
graph.AddNode(mergeNode);
graph.AddNode(outputNode);

using FileStream fileStream = new("output.png", FileMode.Create);
graph.Execute().Snapshot().Encode().SaveTo(fileStream);
