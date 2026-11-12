namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

internal readonly record struct BrushGraphRequirements(int CacheHash, bool UsesTargetSample, bool UsesLatestSample, bool UsesStartingSample, bool UsesTargetFull, bool UsesLatestFull, bool UsesStartingFull);
