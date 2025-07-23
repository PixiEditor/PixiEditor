using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.Helpers;

[assembly: LocalizeEnum<StrokeCap>(StrokeCap.Butt, "BUTT_STROKE_CAP")]
[assembly: LocalizeEnum<StrokeCap>(StrokeCap.Round, "ROUND_STROKE_CAP")]
[assembly: LocalizeEnum<StrokeCap>(StrokeCap.Square, "SQUARE_STROKE_CAP")]

[assembly: LocalizeEnum<StrokeJoin>(StrokeJoin.Bevel, "BEVEL_STROKE_JOIN")]
[assembly: LocalizeEnum<StrokeJoin>(StrokeJoin.Round, "ROUND_STROKE_JOIN")]
[assembly: LocalizeEnum<StrokeJoin>(StrokeJoin.Miter, "MITER_STROKE_JOIN")]

[assembly: LocalizeEnum<GrayscaleNode.GrayscaleMode>(GrayscaleNode.GrayscaleMode.Weighted, "WEIGHTED_GRAYSCALE_MODE")]
[assembly: LocalizeEnum<GrayscaleNode.GrayscaleMode>(GrayscaleNode.GrayscaleMode.Average, "AVERAGE_GRAYSCALE_MODE")]
[assembly: LocalizeEnum<GrayscaleNode.GrayscaleMode>(GrayscaleNode.GrayscaleMode.Custom, "CUSTOM_GRAYSCALE_MODE")]

[assembly: LocalizeEnum<ColorSampleMode>(ColorSampleMode.ColorManaged, "COLOR_MANAGED_COLOR_SAMPLE_MODE")]
[assembly: LocalizeEnum<ColorSampleMode>(ColorSampleMode.Raw, "RAW_COLOR_SAMPLE_MODE")]

[assembly: LocalizeEnum<TileMode>(TileMode.Clamp, "CLAMP_TILE_MODE")]
[assembly: LocalizeEnum<TileMode>(TileMode.Decal, "DECAL_TILE_MODE")]
[assembly: LocalizeEnum<TileMode>(TileMode.Mirror, "MIRROR_TILE_MODE")]
[assembly: LocalizeEnum<TileMode>(TileMode.Repeat, "REPEAT_TILE_MODE")]

[assembly: LocalizeEnum<CombineSeparateColorMode>(CombineSeparateColorMode.RGB, "R_G_B_COMBINE_SEPARATE_COLOR_MODE")] 
[assembly: LocalizeEnum<CombineSeparateColorMode>(CombineSeparateColorMode.HSV, "H_S_V_COMBINE_SEPARATE_COLOR_MODE")]
[assembly: LocalizeEnum<CombineSeparateColorMode>(CombineSeparateColorMode.HSL, "H_S_L_COMBINE_SEPARATE_COLOR_MODE")]
