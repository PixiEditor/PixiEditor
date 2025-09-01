using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.ChangeableDocument.Changeables.Graph.ColorSpaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Animable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Effects;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Tools;
using PixiEditor.Views.Overlays.BrushShapeOverlay;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

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

[assembly: LocalizeEnum<VectorPathOp>(VectorPathOp.Difference, "DIFFERENCE_VECTOR_PATH_OP")]
[assembly: LocalizeEnum<VectorPathOp>(VectorPathOp.Intersect, "INTERSECT_VECTOR_PATH_OP")]
[assembly: LocalizeEnum<VectorPathOp>(VectorPathOp.Union, "UNION_VECTOR_PATH_OP")]
[assembly: LocalizeEnum<VectorPathOp>(VectorPathOp.Xor, "XOR_VECTOR_PATH_OP")]
[assembly: LocalizeEnum<VectorPathOp>(VectorPathOp.ReverseDifference, "REVERSE_DIFFERENCE_VECTOR_PATH_OP")]

[assembly: LocalizeEnum<QualityPreset>(QualityPreset.VeryLow, "VERY_LOW_QUALITY_PRESET")]
[assembly: LocalizeEnum<QualityPreset>(QualityPreset.Low, "LOW_QUALITY_PRESET")]
[assembly: LocalizeEnum<QualityPreset>(QualityPreset.Medium, "MEDIUM_QUALITY_PRESET")]
[assembly: LocalizeEnum<QualityPreset>(QualityPreset.High, "HIGH_QUALITY_PRESET")]
[assembly: LocalizeEnum<QualityPreset>(QualityPreset.VeryHigh, "VERY_HIGH_QUALITY_PRESET")]

[assembly: LocalizeEnum<ColorSpaceType>(ColorSpaceType.Inherit, "INHERIT_COLOR_SPACE_TYPE")]
[assembly: LocalizeEnum<ColorSpaceType>(ColorSpaceType.Srgb, "SRGB_COLOR_SPACE_TYPE")]
[assembly: LocalizeEnum<ColorSpaceType>(ColorSpaceType.LinearSrgb, "LINEAR_SRGB_COLOR_SPACE_TYPE")]

[assembly: LocalizeEnum<EasingType>(EasingType.Linear, "LINEAR_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InSine, "IN_SINE_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutSine, "OUT_SINE_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutSine, "IN_OUT_SINE_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InQuad, "IN_QUAD_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutQuad, "OUT_QUAD_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutQuad, "IN_OUT_QUAD_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InCubic, "IN_CUBIC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutCubic, "OUT_CUBIC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutCubic, "IN_OUT_CUBIC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InQuart, "IN_QUART_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutQuart, "OUT_QUART_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutQuart, "IN_OUT_QUART_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InQuint, "IN_QUINT_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutQuint, "OUT_QUINT_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutQuint, "IN_OUT_QUINT_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InExpo, "IN_EXPO_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutExpo, "OUT_EXPO_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutExpo, "IN_OUT_EXPO_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InCirc, "IN_CIRC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutCirc, "OUT_CIRC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutCirc, "IN_OUT_CIRC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InBack, "IN_BACK_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutBack, "OUT_BACK_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutBack, "IN_OUT_BACK_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InElastic, "IN_ELASTIC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutElastic, "OUT_ELASTIC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutElastic, "IN_OUT_ELASTIC_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InBounce, "IN_BOUNCE_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.OutBounce, "OUT_BOUNCE_EASING_TYPE")]
[assembly: LocalizeEnum<EasingType>(EasingType.InOutBounce, "IN_OUT_BOUNCE_EASING_TYPE")]

[assembly: LocalizeEnum<RotationType>(RotationType.Degrees, "DEGREES_ROTATION_TYPE")]
[assembly: LocalizeEnum<RotationType>(RotationType.Radians, "RADIANS_ROTATION_TYPE")]

[assembly: LocalizeEnum<NoiseType>(NoiseType.TurbulencePerlin, "TURBULENCE_PERLIN_NOISE_TYPE")]
[assembly: LocalizeEnum<NoiseType>(NoiseType.FractalPerlin, "FRACTAL_PERLIN_NOISE_TYPE")]
[assembly: LocalizeEnum<NoiseType>(NoiseType.Voronoi, "VORONOI_NOISE_TYPE")]

[assembly: LocalizeEnum<OutlineType>(OutlineType.Simple, "SIMPLE_OUTLINE_TYPE")]
[assembly: LocalizeEnum<OutlineType>(OutlineType.Gaussian, "GAUSSIAN_OUTLINE_TYPE")]
[assembly: LocalizeEnum<OutlineType>(OutlineType.PixelPerfect, "PIXEL_PERFECT_OUTLINE_TYPE")]

[assembly: LocalizeEnum<VoronoiFeature>(VoronoiFeature.F1, "F1_VORONOI_FEATURE")]
[assembly: LocalizeEnum<VoronoiFeature>(VoronoiFeature.F2, "F2_VORONOI_FEATURE")]
[assembly: LocalizeEnum<VoronoiFeature>(VoronoiFeature.F2MinusF1, "F2_MINUS_F1_VORONOI_FEATURE")]

[assembly: LocalizeEnum<BlendMode>(BlendMode.Normal, "NORMAL_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Darken, "DARKEN_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Lighten, "LIGHTEN_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Multiply, "MULTIPLY_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Screen, "SCREEN_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Overlay, "OVERLAY_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.HardLight, "HARD_LIGHT_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.SoftLight, "SOFT_LIGHT_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.ColorDodge, "COLOR_DODGE_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.ColorBurn, "COLOR_BURN_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Hue, "HUE_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Saturation, "SATURATION_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Color, "COLOR_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Luminosity, "LUMINOSITY_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Difference, "DIFFERENCE_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Exclusion, "EXCLUSION_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.Erase, "ERASE_BLEND_MODE")]
[assembly: LocalizeEnum<BlendMode>(BlendMode.LinearDodge, "LINEAR_DODGE_BLEND_MODE")]

[assembly: LocalizeEnum<PaintBrushShape>(PaintBrushShape.Circle, "PAINT_BRUSH_SHAPE_CIRCLE")]
[assembly: LocalizeEnum<PaintBrushShape>(PaintBrushShape.Square, "PAINT_BRUSH_SHAPE_SQUARE")]
