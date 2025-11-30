namespace PixiEditor.Extensions.CommonApi.UserPreferences;

public static class PreferencesConstants
{
    public const string FavouritePalettes = "FavouritePalettes";
    public const string RecentlyOpened = "RecentlyOpened";

    public const string MaxOpenedRecently = "MaxOpenedRecently";
    public const int MaxOpenedRecentlyDefault = 8;
    public const string DisableNewsPanel = "DisableNewsPanel";
    public const string LastCheckedNewsIds = "LastCheckedNewsIds";
    public const string NewsPanelCollapsed = "NewsPanelCollapsed";

    public const string SaveSessionStateEnabled = "SaveSessionStateEnabled";
    public const bool SaveSessionStateDefault = true;

    public const string AutosaveEnabled = "AutosaveEnabled";
    public const bool AutosaveEnabledDefault = true;

    public const string AutosaveHistory = "AutosaveHistory";

    public const string AutosavePeriodMinutes = "AutosavePeriodMinutes";
    public const double AutosavePeriodDefault = 3;

    public const string AutosaveToDocumentPath = "AutosaveToDocumentPath";
    public const bool AutosaveToDocumentPathDefault = false;

    public const string LastCrashFile = "LastCrashFile";
    public const string NextSessionFiles = "NextSessionFiles";

    public const string OpenDirectoryOnExport = "OpenDirectoryOnExport";
    public const bool OpenDirectoryOnExportDefault = true;

    public const string AnalyticsEnabled = "AnalyticsEnabled";
    public const bool AnalyticsEnabledDefault = true;

    public const string PrimaryToolset = "PrimaryToolset";
    public const string PrimaryToolsetDefault = "PAINT_TOOLSET";

    public const string AutoScaleBackground = "AutoScaleBackground";
    public const bool AutoScaleBackgroundDefault = true;

    public const string CustomBackgroundScaleX = "CustomBackgroundScaleX";
    public const string CustomBackgroundScaleY = "CustomBackgroundScaleY";
    public const double CustomBackgroundScaleDefault = 16;

    public const string PrimaryBackgroundColorDefault = "#616161";
    public const string PrimaryBackgroundColor = "PrimaryBackgroundColor";

    public const string SecondaryBackgroundColorDefault = "#353535";
    public const string SecondaryBackgroundColor = "SecondaryBackgroundColor";

    public const string MaxBilinearSampleSize = "MaxBilinearSampleSize";
    public const int MaxBilinearSampleSizeDefault = 4096;

    public const string DisablePreviews = "DisablePreviews";
    public const bool DisablePreviewsDefault = false;
    public const string FavouriteBrushes = "FavouriteBrushes";
}
